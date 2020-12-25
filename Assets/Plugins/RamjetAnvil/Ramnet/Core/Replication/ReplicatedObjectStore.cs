using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using UnityEngine.Networking;
using Guid = RamjetAnvil.Unity.Utility.Guid;

namespace RamjetAnvil.RamNet {

    public class ObjectMessageParser {
        private readonly IDictionary<MessageType, NetworkMessage<IObjectMessage>> _messages;

        public ObjectMessageParser(IReadOnlyDictionary<Type, MessageType> messageTypes) {
            _messages = new ArrayDictionary<MessageType, NetworkMessage<IObjectMessage>>(messageTypes.Count);
            foreach (var messageType in messageTypes) {
                _messages.Add(messageType.Value, new NetworkMessage<IObjectMessage>(
                    returnToPool: null,
                    messageType: messageType.Value,
                    content: (IObjectMessage)Activator.CreateInstance(messageType.Key)));
            }
        }

        public NetworkMessage<IObjectMessage> Parse(NetBuffer reader) {
            var messageType = reader.ReadMessageType();
            var message = _messages[messageType];
            message.Content.Deserialize(reader);
            return message;
        }
    }

    public class ReplicatedObjectStore : IReplicatedObjectDatabase {

        public event Action<ReplicatedObject> ObjectAdded;
        public event Action<ReplicatedObject> ObjectRemoved;

        private readonly Func<GameObject, IReplicatedObjectDatabase, ReplicatedObject> _replicationDecorator; 
        private readonly IReadOnlyDictionary<ObjectType, IObjectPool<ReplicatedObject>> _objectPools;
        private IDictionary<ObjectId, IPooledObject<ReplicatedObject>> _instances;

        private readonly GameObject _spawnedAuthorityObjects;
        private readonly GameObject _spawnedOwnerObjects;
        private readonly GameObject _spawnedOtherObjects;

        public ReplicatedObjectStore(
            IReadOnlyDictionary<ObjectType, Func<GameObject>> objectFactories,
            Func<GameObject, IReplicatedObjectDatabase, ReplicatedObject> replicationDecorator, 
            int initialInstanceCapacity = 256) {

            GameObject spawnedReplicatedObjects = GameObject.Find("ReplicatedObjects");
            if (spawnedReplicatedObjects == null) {
                spawnedReplicatedObjects = new GameObject("ReplicatedObjects");
                _spawnedAuthorityObjects = new GameObject("Authority").SetParent(spawnedReplicatedObjects);
                _spawnedOwnerObjects = new GameObject("Owner").SetParent(spawnedReplicatedObjects);
                _spawnedOtherObjects = new GameObject("Others").SetParent(spawnedReplicatedObjects);
            } else {
                _spawnedAuthorityObjects = spawnedReplicatedObjects.transform.FindChild("Authority").gameObject;
                _spawnedOwnerObjects = spawnedReplicatedObjects.transform.FindChild("Owner").gameObject;
                _spawnedOtherObjects = spawnedReplicatedObjects.transform.FindChild("Others").gameObject;
            }   

            _objectPools = new ImmutableArrayDictionary<ObjectType, IObjectPool<ReplicatedObject>>(size: objectFactories.Count)
                .AddRange(objectFactories.Select(kvPair => {
                    var objectType = kvPair.Key;
                    var objectFactory = kvPair.Value;
                    return new KeyValuePair<ObjectType, IObjectPool<ReplicatedObject>>(
                        objectType, 
                        CreateReplicatedPool(replicationDecorator, this, objectFactory));
                }));
            _replicationDecorator = replicationDecorator;
            _instances = new ArrayDictionary<ObjectId, IPooledObject<ReplicatedObject>>(initialInstanceCapacity);
        }

        public IList<ReplicatedObject> FindObjects(ObjectType type, IList<ReplicatedObject> results, ObjectRole role = ObjectRoles.Everyone) {
            var objects = _instances.Values.ToListOptimized();
            for (int i = 0; i < objects.Count; i++) {
                var replicatedObject = objects[i];
                if(replicatedObject.Instance.Type == type && (replicatedObject.Instance.Role & role) != 0) {
                    results.Add(replicatedObject.Instance);
                }
            }
            return results;
        }

        public Maybe<ReplicatedObject> Find(ObjectId id) {
            return _instances.Get(id).Select(o => o.Instance);
        }

        private readonly IList<ReplicatedObject> _replicatedObjectCache = new List<ReplicatedObject>();
        public IList<ReplicatedObject> AllObjects {
            get {
                _replicatedObjectCache.Clear();
                var objects = _instances.Values.ToListOptimized();
                for (int i = 0; i < objects.Count; i++) {
                    var pooledObject = objects[i];
                    _replicatedObjectCache.Add(pooledObject.Instance);
                }
                return _replicatedObjectCache;
            }
        } 

        public ReplicatedObject Instantiate(ObjectType type, ObjectRole role, ObjectId objectId, ConnectionId connectionId,
            Vector3? position, Quaternion? rotation) {

            IPooledObject<ReplicatedObject> replicatedObject;
            if (!_instances.TryGetValue(objectId, out replicatedObject)) {
                var objectPool = _objectPools[type];
                replicatedObject = objectPool.Take();
                Instantiate(replicatedObject, type, role, objectId, connectionId);
                replicatedObject.Instance.GameObject.transform.position = position ?? Vector3.zero;
                replicatedObject.Instance.GameObject.transform.rotation = rotation ?? Quaternion.identity;
            }
            return replicatedObject.Instance;
        }

        public ReplicatedObject AddExistingInstance(ObjectRole role, ConnectionId hostConnectionId, 
            GameObject instance, ObjectId objectId, Guid globalId) {

            IPooledObject<ReplicatedObject> replicatedObject;
            if (!_instances.TryGetValue(objectId, out replicatedObject)) {
                replicatedObject = new UnmanagedObject<ReplicatedObject>(_replicationDecorator(instance, this));
                Instantiate(replicatedObject, null, role, objectId, hostConnectionId);
                replicatedObject.Instance.IsPreExisting = true;
                replicatedObject.Instance.GlobalObjectId.CopyFrom(globalId);
            }

            return replicatedObject.Instance;
        }

        public void ClearStore() {
            var replicatedObjects = _instances.Values.ToListOptimized();
            for (int i = 0; i < replicatedObjects.Count; i++) {
                replicatedObjects[i].Dispose();
            }
            _instances.Clear();
        }

        // TODO Add ObjectType: PreExisting
        private void Instantiate(IPooledObject<ReplicatedObject> pooledObject, ObjectType? type, ObjectRole role,
            ObjectId objectId, ConnectionId connectionId) {

            var @object = pooledObject.Instance;
            @object.GameObjectNetworkInfo.ObjectType = type.HasValue ? type.Value : new ObjectType(0);
            @object.GameObjectNetworkInfo.ObjectId = objectId;
            @object.GameObjectNetworkInfo.Role = role;
            @object.Type = type;
            @object.Role = role;
            @object.Id = objectId;
            @object.IsPreExisting = false;
            @object.GlobalObjectId.CopyFrom(Guid.Empty);
            @object.OwnerConnectionId = role.IsOwner() ? ConnectionId.Self : connectionId;
            @object.AuthorityConnectionId = role.IsAuthority() ? ConnectionId.Self : connectionId;
            role.ApplyTo(@object.GameObject);
            if (!_instances.ContainsKey(@object.Id)) {
                _instances[@object.Id] = pooledObject;
            } else {
                throw new Exception("Cannot replicate instance of " + type + " with " + objectId + " cause the object id is already assigned");
            }
        }

        public void RemoveReplicatedInstance(ConnectionId connectionId, ObjectId id) {
            if (!_instances.ContainsKey(id)) {
                Debug.LogError("Trying to remove " + id + " but it doesn't exist");
                return;
            }

            var pooledInstance = _instances[id];
            if (pooledInstance.Instance.AuthorityConnectionId == connectionId || connectionId == ConnectionId.Self) {
                pooledInstance.Instance.OnDespawn();
                // Turn off network scripts
                ObjectRole.Nobody.ApplyTo(pooledInstance.Instance.GameObject);
                pooledInstance.Dispose();
                _instances.Remove(id);

                if (ObjectRemoved != null) {
                    ObjectRemoved(pooledInstance.Instance);
                }
            } else {
                Debug.LogError("" + connectionId + " is not allowed to delete " + pooledInstance.Instance.Id 
                    + " with name " + pooledInstance.Instance.GameObject + " because he is not the authority");
            }
        }

        public void DispatchMessage(ConnectionId connectionId, ObjectId receiverObjectId, INetworkMessage<IObjectMessage> objectMessage,
            SequenceNumber sequenceNumber, float latency) {

            var pooledInstance = _instances[receiverObjectId];
            if (pooledInstance == null) {
                Debug.LogWarning("object with ID " + receiverObjectId + " does not exist");
            } else {
                var instance = pooledInstance.Instance;

                var senderRole = ObjectRole.Nobody;
                {
                    var isOwner = connectionId == instance.OwnerConnectionId;
                    var isAuthority = connectionId == instance.AuthorityConnectionId;
                    senderRole = senderRole | (isOwner ? ObjectRole.Owner : ObjectRole.Nobody);
                    senderRole = senderRole | (isAuthority ? ObjectRole.Authority : ObjectRole.Nobody);
                    senderRole = senderRole | (!isOwner && !isAuthority ? ObjectRole.Others : ObjectRole.Nobody);
                }

                instance.GameObjectNetworkInfo.LastReceivedMessageTimestamp = Time.realtimeSinceStartup;
                instance.MessageHandler.Dispatch(new Sender(connectionId, senderRole, sequenceNumber, latency), objectMessage);
                objectMessage.Dispose();
            }
        }

        public void Activate(ReplicatedObject replicatedObject) {
            replicatedObject.GameObject.SetActive(true);
            replicatedObject.OnSpawn();

            if (!replicatedObject.IsPreExisting) {
                if (replicatedObject.Role.IsAuthority()) {
                    replicatedObject.GameObject.SetParent(_spawnedAuthorityObjects);
                } else if (replicatedObject.Role.IsOwner()) {
                    replicatedObject.GameObject.SetParent(_spawnedOwnerObjects);
                } else {
                    replicatedObject.GameObject.SetParent(_spawnedOtherObjects);
                }   
            }

            replicatedObject.MessageHandler.ClearLastReceivedMessages();

            if (ObjectAdded != null) {
                ObjectAdded(replicatedObject);
            }
        }

        public uint Capacity {
            get { return (uint) _instances.Count; }
            set {
                if (_instances.Count != value) {
                    _instances = new ArrayDictionary<ObjectId, IPooledObject<ReplicatedObject>>(size: (int)value, existingDict: _instances);
                }
            }
        }

        public static Func<GameObject, IReplicatedObjectDatabase, ReplicatedObject> GameObjectReplicationDecorator(
            Func<GameObject, ObjectMessageRouter> objectMessageDispatcherFactory,
            Func<ReplicatedObject, ObjectMessageSender> messageSenderFactory,
            DependencyContainer globalDependencies,
            IReadOnlyDictionary<Type, MessageType> objectMessageTypes) {

            var objectMessageCache = new ImmutableArrayDictionary<MessageType, IObjectMessage>(objectMessageTypes.Count)
                .AddRange(objectMessageTypes.Select(kvPair => {
                    var type = kvPair.Key;
                    var serializableType = kvPair.Value;
                    return new KeyValuePair<MessageType, IObjectMessage>(serializableType,
                        (IObjectMessage) Activator.CreateInstance(type));
                }));

            return (gameObject, replicationDatabase) => {
                var messageHandler = objectMessageDispatcherFactory(gameObject);

                var constructors = InitialStateLogic.FindInitialStateConstructors(gameObject, objectMessageTypes);
                
                var spawnables = gameObject.GetComponentsOfInterfaceInChildren<ISpawnable>()
                    .ToImmutableList();
                var replicatedObject = new ReplicatedObject(gameObject, messageHandler, 
                    new ReplicationConstructor(constructors, objectMessageCache),
                    spawnables);

                // Inject message sender
                var messageSender = messageSenderFactory(replicatedObject);
                DependencyInjector.Default.Inject(gameObject, globalDependencies, overrideExisting: true);
                DependencyInjector.Default.InjectSingle(gameObject, messageSender, overrideExisting: true);
                DependencyInjector.Default.InjectSingle(gameObject, replicationDatabase, overrideExisting: true);
                DependencyInjector.Default.InjectSingle(gameObject, replicatedObject.GameObjectNetworkInfo, overrideExisting: true);

                return replicatedObject;
            };
        }

        public static IObjectPool<ReplicatedObject> CreateReplicatedPool(
            Func<GameObject, IReplicatedObjectDatabase, ReplicatedObject> gameObjectReplicationDecorator,
            IReplicatedObjectDatabase objectDatabase,
            Func<GameObject> gameObjectFactory) {

            var gameObjectPool = GameObject.Find("ObjectPool") ?? new GameObject("ObjectPool");
            
            return new ObjectPool<ReplicatedObject>(() => {
                var gameObject = gameObjectFactory();

                // Force awake
                gameObject.SetActive(true);

                var replicatedObject = gameObjectReplicationDecorator(gameObject, objectDatabase);

                return new ManagedObject<ReplicatedObject>(
                    instance: replicatedObject,
                    onReturnedToPool: () => {
                        if (!gameObject.IsDestroyed() && !gameObjectPool.IsDestroyed()) {
                            gameObject.SetActive(false);
                            gameObject.transform.parent = gameObjectPool.transform;
                        }
                        replicatedObject.MessageHandler.ClearLastReceivedMessages();
                    },
                    onTakenFromPool: () => {});
            });
        }

        public class ObjectMessageSender : IObjectMessageSender {

            private readonly ReplicatedObject _object;
            private readonly IBasicObjectPool<MulticastNetworkMessage> _networkMessages;
            private readonly IMessageSender _sender;

            private readonly TransportGroupRouter _groupRouter;
            private readonly TransportGroupId _group;

            public static Func<ReplicatedObject, ObjectMessageSender> CreateFactory(
                IMessageSender sender,
                TransportGroupRouter groupRouter,
                TransportGroupId group,
                IBasicObjectPool<MulticastNetworkMessage> networkMessages) {

                return @object => new ObjectMessageSender(sender, groupRouter, group, networkMessages, @object);
            }

            public ObjectMessageSender(IMessageSender sender,
                TransportGroupRouter groupRouter,
                TransportGroupId group,
                IBasicObjectPool<MulticastNetworkMessage> networkMessages,
                ReplicatedObject o) {

                _sender = sender;
                _object = o;
                _groupRouter = groupRouter;
                _group = group;
                _networkMessages = networkMessages;
            }

            private readonly IList<ConnectionId> _receiverConnectionIds = new List<ConnectionId>(); 

            public void Send<TMessage>(INetworkMessage<TMessage> message, ObjectRole recipient) where TMessage : IObjectMessage {
                var activeConnections = _groupRouter.GetActiveConnections(_group);

                _receiverConnectionIds.Clear();
                if ((recipient & ObjectRole.Authority) != 0 && (recipient & ObjectRole.Owner) != 0) {
                    if (_object.AuthorityConnectionId == _object.OwnerConnectionId) {
                        _receiverConnectionIds.Add(_object.AuthorityConnectionId);
                    } else {
                        _receiverConnectionIds.Add(_object.AuthorityConnectionId);
                        _receiverConnectionIds.Add(_object.OwnerConnectionId);
                    }
                } else if ((recipient & ObjectRole.Authority) != 0) {
                    _receiverConnectionIds.Add(_object.AuthorityConnectionId);
                } else if ((recipient & ObjectRole.Owner) != 0) {
                    _receiverConnectionIds.Add(_object.OwnerConnectionId);    
                }

                if ((recipient & ObjectRole.Others) != 0) {
                    for (int i = 0; i < activeConnections.Count; i++) {
                        var connectionId = activeConnections[i];
                        if (connectionId != _object.OwnerConnectionId && connectionId != _object.AuthorityConnectionId) {
                            _receiverConnectionIds.Add(connectionId);    
                        }
                    }
                }

                var networkMessage = CreateMulticastMessage(message, usageCount: _receiverConnectionIds.Count);
                for (int i = 0; i < _receiverConnectionIds.Count; i++) {
                    var connectionId = _receiverConnectionIds[i];
                    //Debug.Log("sending message of type " + message.Content.GetType() + " to " + connectionId);
                    _sender.Send(connectionId, networkMessage);    
                }
            }

            private INetworkMessage CreateMulticastMessage(INetworkMessage message, int usageCount) {
                var multicastMessage = _networkMessages.Take();
                multicastMessage.ObjectId = _object.Id;
                multicastMessage.NetworkMessage = message;
                multicastMessage.UsageCount = usageCount;
                return multicastMessage;
            }

            public class MulticastNetworkMessage : INetworkMessage {
                private readonly IBasicObjectPool<MulticastNetworkMessage> _pool;
                private readonly MessageType _toObjectIdMessageType;
                public ObjectId ObjectId;
                public INetworkMessage NetworkMessage;
                public int UsageCount;

                public MulticastNetworkMessage(IBasicObjectPool<MulticastNetworkMessage> pool, MessageType toObjectIdMessageType) {
                    _pool = pool;
                    _toObjectIdMessageType = toObjectIdMessageType;
                }
                
                public void Serialize(NetBuffer writer) {
                    // TODO Put reading an writing of object messages together in
                    // one place for readability
                    writer.Write(_toObjectIdMessageType);
                    writer.Write(ObjectId);
                    NetworkMessage.Serialize(writer);
                }

                public NetDeliveryMethod QosType { get { return NetworkMessage.QosType; } }

                public void Dispose() {
                    if (UsageCount > 1) {
                        UsageCount--;
                    } else {
                        NetworkMessage.Dispose();
                        _pool.Return(this);
                    }
                }
            }

        }

    }
}
