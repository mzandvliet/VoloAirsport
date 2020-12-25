using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using Guid = RamjetAnvil.Unity.Utility.Guid;

namespace RamjetAnvil.RamNet {

    public class NetworkSystems : IDisposable {
        private readonly IReadOnlyDictionary<Type, MessageType> _objectMessageTypes;
        private readonly IReadOnlyDictionary<Type, MessageType> _messageTypes;
        private readonly IConnectionlessMessageSender _connectionlessMessageSender;
        private readonly IMessageSender _messageSender;
        private readonly MessagePool _messagePool;
        private readonly IConnectionManager _connectionManager;
        private readonly TransportGroupRouter _groupRouter;
        private readonly MessageRouter _defaultMessageRouter;
        private readonly ObjectMessageParser _objectMessageParser;
        private readonly Replicator _replicator;
        private readonly ReplicatedObjectStore _objectStore;

        public NetworkSystems(IMessageSender messageSender,
            IConnectionlessMessageSender connectionlessMessageSender,
            MessagePool messagePool,
            IConnectionManager connectionManager, 
            TransportGroupRouter groupRouter,
            MessageRouter defaultMessageRouter,
            ReplicatedObjectStore objectStore, 
            IReadOnlyDictionary<Type, MessageType> messageTypes, 
            IReadOnlyDictionary<Type, MessageType> objectMessageTypes, 
            Replicator replicator,
            ObjectMessageParser objectMessageParser) {

            _messageSender = messageSender;
            _messagePool = messagePool;
            _connectionlessMessageSender = connectionlessMessageSender;
            _connectionManager = connectionManager;
            _groupRouter = groupRouter;
            _defaultMessageRouter = defaultMessageRouter;
            _objectStore = objectStore;
            _objectMessageParser = objectMessageParser;
            _objectMessageTypes = objectMessageTypes;
            _messageTypes = messageTypes;
            
            _replicator = replicator;
        }

        public IMessageSender MessageSender {
            get { return _messageSender; }
        }

        public IConnectionManager ConnectionManager {
            get { return _connectionManager; }
        }

        public TransportGroupRouter GroupRouter {
            get { return _groupRouter; }
        }

        public MessageRouter DefaultMessageRouter {
            get { return _defaultMessageRouter; }
        }

        public ReplicatedObjectStore ObjectStore {
            get { return _objectStore; }
        }

        public Replicator Replicator {
            get { return _replicator; }
        }

        public MessagePool MessagePool {
            get { return _messagePool; }
        }

        public IReadOnlyDictionary<Type, MessageType> MessageTypes {
            get { return _messageTypes; }
        }

        public IReadOnlyDictionary<Type, MessageType> ObjectMessageTypes {
            get { return _objectMessageTypes; }
        }

        public ObjectMessageParser ObjectMessageParser {
            get { return _objectMessageParser; }
        }

        public IConnectionlessMessageSender ConnectionlessMessageSender {
            get { return _connectionlessMessageSender; }
        }

        public void Dispose() {
            _connectionManager.Dispose();
            _groupRouter.Dispose();
        }
    }

    public static class NetworkSystem {

        public static NetworkSystems Create(
            IConnectionTransporter transporter,
            IConnectionlessTransporter connectionlessTransporter,
            TransportRouterConfig groupRouterConfig,
            TransportGroupId objectGroupId,
            IReadOnlyDictionary<ObjectType, Func<GameObject>> replicatedObjectPools,
            IMessageSender messageSender,
            IConnectionManager connectionManager,
            ILatencyInfo latencyInfo,
            DependencyContainer globalDependencies) {

            var connectionlessMessageSender = new ConnectionlessMessageSender(connectionlessTransporter);

            var messagePools = MessageTypes.CreateMessagePool(MessageTypes.NetworkMessageTypes);
            var objectMessagePools = MessageTypes.CreateMessagePool(MessageTypes.ObjectMessageTypes);
            var objectMessageParser = new ObjectMessageParser(MessageTypes.ObjectMessageTypes);

            var dependencies = globalDependencies.Copy();

            dependencies.AddDependency("latencyInfo", latencyInfo);
            foreach (var pool in messagePools.Pools) {
                dependencies.AddDependency(pool.ToString(), pool);
            }
            foreach (var pool in objectMessagePools.Pools) {
                dependencies.AddDependency(pool.ToString(), pool);
            }

            var groupRouter = new TransportGroupRouter(transporter, groupRouterConfig);

            Func<GameObject, ObjectMessageRouter> objectMessageDispatcherFactory =
                gameObject => new ObjectMessageRouter(MessageTypes.ObjectMessageTypes, gameObject);
            var networkMessagePool = new BasicObjectPool<ReplicatedObjectStore.ObjectMessageSender.MulticastNetworkMessage>(
                pool => new ReplicatedObjectStore.ObjectMessageSender.MulticastNetworkMessage(pool, MessageTypes.NetworkMessageTypes[typeof(BasicMessage.ToObject)]));
            var objectMessageSenderFactory = ReplicatedObjectStore.ObjectMessageSender.CreateFactory(
                messageSender,
                groupRouter,
                objectGroupId,
                networkMessagePool);
            
            var replicationDecorator = ReplicatedObjectStore.GameObjectReplicationDecorator(objectMessageDispatcherFactory,
                objectMessageSenderFactory,
                dependencies,
                MessageTypes.ObjectMessageTypes);
            int replicatedObjectCapacity = 256;
            var replicatedObjectStore = new ReplicatedObjectStore(replicatedObjectPools,
                replicationDecorator, replicatedObjectCapacity);

            var replicator = new Replicator(replicatedObjectStore,
                groupRouter.GetConnectionGroup(objectGroupId),
                new NetworkReplicator(messageSender, messagePools),
                objectMessageParser);

            var defaultMessageRouter = new MessageRouter(MessageTypes.NetworkMessageTypes, latencyInfo);
            groupRouter.SetDataHandler(groupRouterConfig.DefaultGroup, defaultMessageRouter);

            return new NetworkSystems(
                messageSender,
                connectionlessMessageSender,
                messagePools,
                connectionManager,
                groupRouter,
                defaultMessageRouter, 
                replicatedObjectStore,
                MessageTypes.NetworkMessageTypes,
                MessageTypes.ObjectMessageTypes,
                replicator,
                objectMessageParser);
        }

        public static void InstallBasicServerHandlers(MessageRouter messageRouter,
            IClock clock, IClock fixedClock, NetworkSystems networkSystems) {

            var messagePools = networkSystems.MessagePool;
            var messageSender = networkSystems.MessageSender;
            messageRouter
                .RegisterHandler(DefaultMessageHandlers.Ping(clock, fixedClock, messagePools.GetPool<BasicMessage.Pong>(), messageSender))
                .RegisterHandler(DefaultMessageHandlers.ToObject(networkSystems.ObjectMessageParser, networkSystems.ObjectStore));
        }

        public static void InstallBasicClientHandlers(MessageRouter messageRouter, NetworkSystems networkSystems, IDictionary<Guid, GameObject> preExistingObjects) {
            messageRouter
                .RegisterHandler(DefaultMessageHandlers.ReplicatePreExistingObject(networkSystems.ObjectStore, preExistingObjects))
                .RegisterHandler(DefaultMessageHandlers.CreateObject(networkSystems.ObjectMessageParser, networkSystems.ObjectStore))
                .RegisterHandler(DefaultMessageHandlers.DeleteObject(networkSystems.ObjectStore))
                .RegisterHandler(DefaultMessageHandlers.ToObject(networkSystems.ObjectMessageParser, networkSystems.ObjectStore));
        }

        public static class DefaultMessageHandlers {

            public static Action<MessageMetaData, BasicMessage.ReplicatePreExistingObject, NetBuffer> ReplicatePreExistingObject(
                ReplicatedObjectStore objectStore,
                IDictionary<Guid, GameObject> preExistingObjects) {

                return (metadata, message, reader) => {
                    var instance = preExistingObjects[message.GlobalObjectId];
                    objectStore.AddExistingInstance(message.ObjectRole, metadata.ConnectionId,
                        instance, message.NetworkObjectId, message.GlobalObjectId);
                };
            }

            public static Action<MessageMetaData, BasicMessage.CreateObject, NetBuffer> CreateObject(
                ObjectMessageParser messageParser,
                ReplicatedObjectStore objectStore) {

                return (metadata, message, reader) => {
                    var instance = objectStore.Instantiate(message.ObjectType, message.ObjectRole,
                        message.ObjectId, metadata.ConnectionId, message.Position, message.Rotation);
                    var bytesRead = 0;
                    while (bytesRead < message.AdditionalData.WriterPosition()) {
                        var objectMessage = messageParser.Parse(message.AdditionalData);
                        bytesRead = message.AdditionalData.ReaderPosition();
                        objectStore.DispatchMessage(metadata.ConnectionId, message.ObjectId, objectMessage, 
                            new SequenceNumber(1), metadata.Latency);
                    }
                    objectStore.Activate(instance);
                };
            }

            public static Action<MessageMetaData, BasicMessage.DeleteObject> DeleteObject(ReplicatedObjectStore objectStore) {
                return (metadata, message) => {
                    objectStore.RemoveReplicatedInstance(metadata.ConnectionId, message.ObjectId);
                };
            }

            public static Action<MessageMetaData, BasicMessage.ToObject, NetBuffer> ToObject(
                ObjectMessageParser messageParser,
                ReplicatedObjectStore objectStore) {

                return (metadata, message, reader) => {
                    var objectMessage = messageParser.Parse(reader);
                    //Debug.Log("receiving message of type " + objectMessage.Content.GetType() + " for " + message.ReceiverId);
                    objectStore.DispatchMessage(metadata.ConnectionId, message.ReceiverId, objectMessage, 
                        metadata.SequenceNumber, metadata.Latency);
                };
            }

            public static Action<MessageMetaData, BasicMessage.Ping> Ping(
                IClock clock,
                IClock fixedClock,
                INetworkMessagePool<BasicMessage.Pong> messagePool,
                IMessageSender messageSender) {

                return (metadata, ping) => {
                    var pong = messagePool.Create();
                    pong.Content.FrameId = ping.FrameId;
                    pong.Content.Timestamp = clock.CurrentTime;
                    pong.Content.FixedTimestamp = fixedClock.CurrentTime;
                    messageSender.Send(metadata.ConnectionId, pong);
                };
            }

//            public static Action<IPEndPoint, BasicMessage.Ping> UnconnectedPing(
//                IClock clock,
//                IClock fixedClock,
//                INetworkMessagePool<BasicMessage.Pong> messagePool,
//                IConnectionlessMessageSender messageSender) {
//
//                return (endpoint, ping) => {
//                    var pong = messagePool.Create();
//                    pong.Content.SenderTimestamp = ping.Timestamp;
//                    pong.Content.SenderFixedTimestamp = ping.FixedTimestamp;
//                    pong.Content.Timestamp = clock.CurrentTime;
//                    pong.Content.FixedTimestamp = fixedClock.CurrentTime;
//                    messageSender.Send(endpoint, pong);
//                };
//            }

            public static Action<MessageMetaData, BasicMessage.RequestHostInfo> HostInfoRequest(
                INetworkMessagePool<BasicMessage.HostInfoResponse> messagePool,
                Func<ushort> playerCount,
                IConnectionlessMessageSender messageSender) {

                return (metadata, message) => {
                    var response = messagePool.Create();
                    response.Content.SenderTimestamp = message.Timestamp;
                    response.Content.PlayerCount = playerCount();
                    messageSender.Send(metadata.EndPoint, response);
                };
            }

        }

    }
}
