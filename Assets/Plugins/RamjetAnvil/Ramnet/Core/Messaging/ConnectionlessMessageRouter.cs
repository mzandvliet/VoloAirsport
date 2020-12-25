using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.RamNet {

    public class ConnectionlessMessageRouter : IConnectionlessDataHandler {

        private readonly IDictionary<Type, MessageType> _networkMessageTypes;
        private readonly IDictionary<MessageType, Action<IPEndPoint, NetBuffer>> _handlers;

        public ConnectionlessMessageRouter(IDictionary<Type, MessageType> networkMessageTypes) {
            _networkMessageTypes = networkMessageTypes;
            _handlers = new ArrayDictionary<MessageType, Action<IPEndPoint, NetBuffer>>(_networkMessageTypes.Count);
        }

        public ConnectionlessMessageRouter RegisterHandler<TMessage>(Action<IPEndPoint, TMessage> handler) where TMessage : IMessage, new() {
            var messageType = _networkMessageTypes[typeof (TMessage)];
            var message = new TMessage();
            _handlers[messageType] = (source, reader) => {
                message.Deserialize(reader);
                handler(source, message);
            };
            return this;
        }

        public void ClearHandlers() {
            _handlers.Clear();
        }

        public void OnDataReceived(IPEndPoint source, NetBuffer reader) {
            var messageType = reader.ReadMessageType();
            Action<IPEndPoint, NetBuffer> handler;
            if (_handlers.TryGetValue(messageType, out handler)) {
                handler(source, reader);
            }
        }
    }
}
