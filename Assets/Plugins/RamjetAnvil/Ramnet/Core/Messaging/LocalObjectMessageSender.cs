using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RamjetAnvil.RamNet {
    public class LocalObjectMessageSender : IObjectMessageSender {

        private SequenceNumber _sequenceNumberCounter;
        private readonly ObjectMessageRouter _messageRouter;

        public LocalObjectMessageSender(ObjectMessageRouter messageRouter) {
            _messageRouter = messageRouter;
            _sequenceNumberCounter = new SequenceNumber(0);
        }

        public void Send<TMessage>(INetworkMessage<TMessage> message, ObjectRole recipient) where TMessage : IObjectMessage {
            if ((recipient & ObjectRole.Owner) != 0 || (recipient & ObjectRole.Authority) != 0) {
                var sequenceNumber = _sequenceNumberCounter;
                _messageRouter.Dispatch(new Sender(ConnectionId.NoConnection, ObjectRoles.Everyone, sequenceNumber, 0), message);
                _sequenceNumberCounter = sequenceNumber.Increment();
            }
            message.Dispose();
        }
    }
}
