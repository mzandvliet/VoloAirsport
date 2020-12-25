    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Lidgren.Network;
    using UnityEngine;
    using Guid = RamjetAnvil.Unity.Utility.Guid;

    namespace RamjetAnvil.RamNet {

        // TODO Get rid of the Replicator and make two different types of ReplicationStores
        // one for the server and one for the client

        /// <summary>
        /// Allows you to create instance of objects that can be used for replication
        /// over the network. Manages unique ids.
        /// </summary>
        public class Replicator : IDisposable {

            private readonly ReplicatedObjectStore _store;
            private readonly IConnectionGroup _connectionGroup;
            private readonly NetworkReplicator _networkReplicator;
            private readonly Queue<ObjectId> _unusedObjectIds;
            private readonly ObjectMessageParser _messageParser;
            private readonly NetBuffer _initialStateBuffer;
            private readonly int _growth;
            private uint _currentCapacity;

            public Replicator(
                ReplicatedObjectStore store,
                IConnectionGroup connectionGroup,
                NetworkReplicator networkReplicator,
                ObjectMessageParser messageParser,
                int growth = 256) {

                var initialCapacity = (int)store.Capacity;
                _initialStateBuffer = new NetBuffer();
                _currentCapacity = 0;
                _growth = growth;
                _messageParser = messageParser;
                _store = store;
                _unusedObjectIds = new Queue<ObjectId>(initialCapacity);
                _networkReplicator = networkReplicator;
                GenerateAdditionalObjectIds(initialCapacity);

                _connectionGroup = connectionGroup;
                _connectionGroup.PeerJoined += PeerJoined;
                _connectionGroup.PeerLeft += PeerLeft;
            }
        
            private void PeerJoined(ConnectionId connectionId) {
                if (connectionId.IsRemote) {
                    var objects = _store.AllObjects;
                    for (int i = 0; i < objects.Count; i++) {
                        // TODO Only replicate objects that are activated
                        _networkReplicator.Replicate(connectionId, objects[i]);
                    }   
                }
            }

            private void PeerLeft(ConnectionId connectionId) {
                if (connectionId.IsRemote) {
                    var objects = _store.AllObjects.FilterOwner(connectionId);
                    for (int i = 0; i < objects.Count; i++) {
                        var replicatedObject = objects[i];
                        RemoveReplicatedInstance(replicatedObject.Id);
                    }   
                }
            }

            public ReplicatedObject AddPreExistingInstance(ConnectionId ownerConnectionId, GameObject instance, Guid globalId) {
                var objectId = RequestObjectId();
                var role = ObjectRole.Authority | ((ownerConnectionId == ConnectionId.Self)
                    ? ObjectRole.Owner
                    : ObjectRole.Nobody);
                var replicatedInstance = _store.AddExistingInstance(role, ownerConnectionId, instance, objectId, globalId);
                _networkReplicator.Replicate(_connectionGroup.ActiveConnections, replicatedInstance);
                return replicatedInstance;
            }

            public ReplicatedObject InstantiateLocally(
                ObjectType type,
                Vector3? position = null, 
                Quaternion? rotation = null) {

                return Instantiate(type, ConnectionId.Self, position, rotation);
            }

            public ReplicatedObject Instantiate(
                ObjectType type, 
                ConnectionId ownerConnectionId, 
                Vector3? position = null, 
                Quaternion? rotation = null) {

                var objectId = RequestObjectId();
                var role = ObjectRole.Authority | ((ownerConnectionId == ConnectionId.Self)
                    ? ObjectRole.Owner
                    : ObjectRole.Nobody);
                return _store.Instantiate(type, role, objectId, ownerConnectionId, position, rotation);
            }

            public void RemoveReplicatedInstance(ObjectId objectId) {
                _store.RemoveReplicatedInstance(ConnectionId.Self, objectId);
                _networkReplicator.Delete(_connectionGroup.ActiveConnections, objectId);
                _unusedObjectIds.Enqueue(objectId);
            }

            public void RemoveAllReplicatedInstances() {
                var objects = _store.AllObjects;
                for (int i = 0; i < objects.Count; i++) {
                    var replicatedObject = objects[i];
                    RemoveReplicatedInstance(replicatedObject.Id);
                }
            }

            private ObjectId RequestObjectId() {
                if (_unusedObjectIds.Count == 0) {
                    GenerateAdditionalObjectIds(_growth);
                }
                return _unusedObjectIds.Dequeue();
            }

            private void GenerateAdditionalObjectIds(int growth) {
                var newCapacity = _currentCapacity + (uint)growth;
                for (uint i = _currentCapacity; i < newCapacity; i++) {
                    _unusedObjectIds.Enqueue(new ObjectId(i));
                }
                _currentCapacity = newCapacity;
                _store.Capacity = newCapacity;
            }

            public ReplicatedObjectStore Store {
                get { return _store; }
            }

            public void Activate(ReplicatedObject replicatedObject) {
                // Generate initial state and send it to self
                _initialStateBuffer.Reset();
                replicatedObject.ReplicationConstructor.SerializeInitialState(_initialStateBuffer);
                var bytesRead = 0;
                while (bytesRead < _initialStateBuffer.WriterPosition()) {
                    var objectMessage = _messageParser.Parse(_initialStateBuffer);
                    bytesRead = _initialStateBuffer.ReaderPosition();
                    _store.DispatchMessage(ConnectionId.Self, replicatedObject.Id, objectMessage, 
                        new SequenceNumber(1), latency: 0f);
                }

                _store.Activate(replicatedObject);

                _networkReplicator.Replicate(_connectionGroup.ActiveConnections, replicatedObject);
            }

            public void Dispose() {
                _connectionGroup.PeerJoined -= PeerJoined;
                _connectionGroup.PeerLeft -= PeerLeft;
            }
        }
    }
