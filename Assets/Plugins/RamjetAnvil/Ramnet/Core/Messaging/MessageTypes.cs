using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lidgren.Network;

namespace RamjetAnvil.RamNet {

    public interface IMessage {
        void Serialize(NetBuffer writer);
        void Deserialize(NetBuffer reader);
        NetDeliveryMethod QosType { get; }
    }

    public interface INetworkMessage : IDisposable {
        NetDeliveryMethod QosType { get; }
        void Serialize(NetBuffer writer);
    }

    public interface INetworkMessage<TMessage> : INetworkMessage where TMessage : IMessage {
        MessageType MessageType { get; }
        TMessage Content { get; }
    }

    public class NetworkMessage<T> : INetworkMessage<T> where T : class, IMessage {
        private static readonly Action<INetworkMessage<T>> EmptyReturnToPool = message => { };

        private readonly Action<INetworkMessage<T>> _returnToPool;
        private readonly MessageType _messageType;
        private readonly T _content;

        public NetworkMessage(MessageType messageType, T content, Action<INetworkMessage<T>> returnToPool) {
            _returnToPool = returnToPool ?? EmptyReturnToPool;
            _messageType = messageType;
            _content = content;
        }

        public void Serialize(NetBuffer writer) {
            writer.Write(MessageType);
            _content.Serialize(writer);
        }

        public MessageType MessageType {
            get { return _messageType; }
        }

        public NetDeliveryMethod QosType { get { return _content.QosType; } }

        public T Content {
            get { return _content; }
        }

        public void Dispose() {
            _returnToPool(this);
        }
    }
    
    public interface INetworkMessagePool<T> where T : class, IMessage {
        INetworkMessage<T> Create();
    }

    public class NetworkMessagePool<T> : INetworkMessagePool<T> where T : class, IMessage, new() {
        private readonly MessageType _messageTypeId;
        private readonly Queue<INetworkMessage<T>> _pool;
        private readonly Action<INetworkMessage<T>> _returnToPool;

        public NetworkMessagePool(IReadOnlyDictionary<Type, MessageType> networkMessageIds) {
            _messageTypeId = networkMessageIds[typeof (T)];
            _pool = new Queue<INetworkMessage<T>>(1);
            _returnToPool = message => _pool.Enqueue(message);
            GrowPool();
        }

        public INetworkMessage<T> Create() {
            if (_pool.Count < 1) {
                GrowPool();
            }
            return _pool.Dequeue();
        }

        private void GrowPool() {
            _pool.Enqueue(new NetworkMessage<T>(_messageTypeId, new T(), _returnToPool));
        }
    }

    public interface IObjectMessage : IMessage {}

    public interface IOrderedObjectMessage : IObjectMessage {}

    public struct MessageType : IEquatable<MessageType> {
        public readonly uint Value;

        public MessageType(uint value) {
            Value = value;
        }

        public bool Equals(MessageType other) {
            return Value == other.Value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MessageType && Equals((MessageType) obj);
        }

        public override int GetHashCode() {
            return (int) Value;
        }

        public static bool operator ==(MessageType left, MessageType right) {
            return left.Equals(right);
        }

        public static bool operator !=(MessageType left, MessageType right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("MessageType({0})", Value);
        }

        public static explicit operator uint(MessageType objectType) {
            return objectType.Value;
        }
        public static explicit operator MessageType(uint value) {
            return new MessageType(value);
        }
    }

    public class MessagePool {
        private readonly IDictionary<Type, object> _pools;

        public MessagePool(IDictionary<Type, object> pools) {
            _pools = pools;
        }

        public INetworkMessagePool<T> GetPool<T>() where T : class, IMessage, new() {
            return (INetworkMessagePool<T>)_pools[typeof (T)];
        }

        public INetworkMessage<T> GetMessage<T>() where T : class, IMessage {
            var pool = (INetworkMessagePool<T>)_pools[typeof (T)];
            return pool.Create();
        }

        public IEnumerable<object> Pools { get { return _pools.Values; } } 
    }

    public static class MessageTypes {

        public static readonly IReadOnlyDictionary<Type, MessageType> NetworkMessageTypes =
            AppDomain.CurrentDomain.GetAssemblies()
                .GetAllMessageTypes()
                .Where(type => !typeof(IObjectMessage).IsAssignableFrom(type))
                .GenerateNetworkIds();

        public static readonly IReadOnlyDictionary<Type, MessageType> ObjectMessageTypes =
            AppDomain.CurrentDomain.GetAssemblies()
                .GetAllMessageTypes()
                .Where(type => typeof (IObjectMessage).IsAssignableFrom(type))
                .GenerateNetworkIds();

        private static IEnumerable<Type> GetAllMessageTypes(this IEnumerable<Assembly> assemblies) {
            return assemblies
                .Distinct()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IMessage).IsAssignableFrom(type) &&
                               type.IsClass &&
                               !type.IsAbstract);
        } 

        public static IReadOnlyDictionary<Type, MessageType> GenerateNetworkIds(this IEnumerable<Type> networkMessageTypes) {
            var networkMessages = networkMessageTypes
                .OrderBy(type => type.FullName);

            var messageTypes = new Dictionary<Type, MessageType>();
            var messageId = 0u;
            foreach (var networkMessageType in networkMessages) {
                messageTypes.Add(networkMessageType, new MessageType(messageId));
                messageId++;
            }
            return messageTypes.ToReadOnly();
        }

        public static MessagePool CreateMessagePool(IReadOnlyDictionary<Type, MessageType> messageTypes) {
            var objectPools = new Dictionary<Type, object>();
            foreach (var kvPair in messageTypes) {
                var messageType = kvPair.Key;
                var messagePool = Activator.CreateInstance(typeof (NetworkMessagePool<>).MakeGenericType(messageType), messageTypes);
                objectPools.Add(messageType, messagePool);
            }
            return new MessagePool(objectPools);
        } 

    }
}
