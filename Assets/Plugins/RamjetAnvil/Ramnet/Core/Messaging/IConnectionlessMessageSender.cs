using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Util;

namespace RamjetAnvil.RamNet {

    public interface IConnectionlessMessageSender {
        void Send<TMessage>(IPEndPoint endpoint, INetworkMessage<TMessage> message) where TMessage : IMessage;
    }

    public class ConnectionlessMessageSender : IConnectionlessMessageSender {
        private readonly IConnectionlessTransporter _transporter;
        private readonly NetBuffer _netBuffer;

        public ConnectionlessMessageSender(IConnectionlessTransporter transporter) {
            _transporter = transporter;
            _netBuffer = new NetBuffer();
        }

        public void Send<TMessage>(IPEndPoint endpoint, INetworkMessage<TMessage> message) where TMessage : IMessage {
            _netBuffer.Reset();
            message.Serialize(_netBuffer);
            message.Dispose();
            _transporter.SendUnconnected(endpoint, _netBuffer);    
        }
    }

//    public class ConnectionLessNatPunchMessageSender : IConnectionlessMessageSender {
//
//        private readonly INatPunchClient _natPunchClient;
//        private readonly IConnectionlessMessageSender _inner;
//        private readonly IBasicObjectPool<IList<INetworkMessage>> _networkMessageQueuePool; 
//        private readonly IDictionary<IPEndPoint, IList<INetworkMessage>> _queuedMessages;
//
//        public ConnectionLessNatPunchMessageSender(INatPunchClient natPunchClient, IConnectionlessMessageSender inner) {
//            _networkMessageQueuePool = new BasicObjectPool<IList<INetworkMessage>>(
//                pool => new List<INetworkMessage>(),
//                l => l.Clear());
//            _queuedMessages = new Dictionary<IPEndPoint, IList<INetworkMessage>>();
//            _natPunchClient = natPunchClient;
//            _inner = inner;
//        }
//
//        public void Send(IPEndPoint endpoint, INetworkMessage message) {
//            IList<INetworkMessage> queuedMessages;
//            if (!_queuedMessages.TryGetValue(endpoint, out queuedMessages)) {
//                queuedMessages = _networkMessageQueuePool.Take();
//                _queuedMessages[endpoint] = queuedMessages;
//            }
//            queuedMessages.Add(message);
//            _natPunchClient.Punch(endpoint, OnNatPunchSuccess, OnNatPunchFailure);
//        }
//        
//        private void OnNatPunchSuccess(IPEndPoint endPoint) {
//            IList<INetworkMessage> queuedMessages;
//            if (_queuedMessages.TryGetValue(endPoint, out queuedMessages)) {
//                for (int i = 0; i < queuedMessages.Count; i++) {
//                    var message = queuedMessages[i];
//                    _inner.Send(endPoint, message);
//                }
//                _networkMessageQueuePool.Return(queuedMessages);
//                _queuedMessages.Remove(endPoint);
//            }
//        }
//
//        private void OnNatPunchFailure(IPEndPoint endPoint) {
//            IList<INetworkMessage> queuedMessages;
//            if (_queuedMessages.TryGetValue(endPoint, out queuedMessages)) {
//                for (int i = 0; i < queuedMessages.Count; i++) {
//                    var message = queuedMessages[i];
//                    message.Dispose();
//                }
//                _networkMessageQueuePool.Return(queuedMessages);
//                _queuedMessages.Remove(endPoint);
//            }
//        }
//    }
}
