using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.RamNet;
using RamjetAnvil.Util;
using UnityEngine;
using UnityEngine.Networking;

public class QueueingMessageSender : MonoBehaviour, IMessageSender {

    private struct QueuedMessage {
        public readonly INetworkMessage Message;
        public readonly ConnectionId ConnectionId;

        public QueuedMessage(INetworkMessage message, ConnectionId connectionId) {
            Message = message;
            ConnectionId = connectionId;
        }
    }

    private class QueuedMessageComparer : IComparer<QueuedMessage> {
        public static readonly QueuedMessageComparer Instance = new QueuedMessageComparer();

        private QueuedMessageComparer() {}

        public int Compare(QueuedMessage x, QueuedMessage y) {
            if (x.Message.QosType < y.Message.QosType) return -1;
            if (x.Message.QosType > y.Message.QosType) return 1;
            if (x.ConnectionId.Value < y.ConnectionId.Value) return -1;
            if (x.ConnectionId.Value > y.ConnectionId.Value) return 1; 
            return -1;
        }
    }

    [SerializeField] private LidgrenNetworkTransporter _transporter;

    private SequenceNumber _sequenceNumberCounter;
    private List<QueuedMessage> _messageQueue;

    private NetBuffer _tempMessageBuffer;
    private NetBuffer _packetBuffer;

    void Awake() {
        _sequenceNumberCounter = new SequenceNumber(500);
        _messageQueue = new List<QueuedMessage>();
        _tempMessageBuffer = new NetBuffer();
        _packetBuffer = new NetBuffer();
    }

    public void Send(ConnectionId connectionId, INetworkMessage message) {
        if (connectionId == ConnectionId.NoConnection) {
            throw new ArgumentException("Cannot send message to ConnectionId.NoConnection");
        }

        if (connectionId == ConnectionId.Self) {
            PrepareNewPacket();
            WriteMessage(message);
            _transporter.Send(connectionId, message.QosType, _packetBuffer);
            message.Dispose();
        } else {
            _messageQueue.Add(new QueuedMessage(message, connectionId));    
        }
    }

    public IPEndPoint InternalEndpoint {
        get { return _transporter.InternalEndpoint; }
    }

    void LateUpdate() {
        if (_messageQueue.Count > 0) {
            _messageQueue.Sort(QueuedMessageComparer.Instance);

            var currentConnectionId = _messageQueue[0].ConnectionId;
            var currentChannel = _messageQueue[0].Message.QosType;
            PrepareNewPacket();
            for (int messageIndex = 0; messageIndex < _messageQueue.Count; messageIndex++) {
                var queuedMessage = _messageQueue[messageIndex];
                var message = queuedMessage.Message;
                if (currentConnectionId == queuedMessage.ConnectionId && currentChannel == queuedMessage.Message.QosType) {
                    WriteMessage(message);
                } else {
                    // TODO Split packets if they become too big. 
                    // Especially think about how to deal with unreliable sequenced packets that are too big
                    _transporter.Send(currentConnectionId, currentChannel, _packetBuffer);    

                    currentConnectionId = queuedMessage.ConnectionId;
                    currentChannel = queuedMessage.Message.QosType;
                    PrepareNewPacket();
                    WriteMessage(message);
                }
                message.Dispose();
            }

            _transporter.Send(currentConnectionId, currentChannel, _packetBuffer);

            _transporter.Flush();
            _messageQueue.Clear();
        }
    }

    void PrepareNewPacket() {
        _packetBuffer.Reset();
        var sequenceNumber = _sequenceNumberCounter.Increment();
        _sequenceNumberCounter = sequenceNumber;
        _packetBuffer.Write(sequenceNumber);
    }

    void WriteMessage(INetworkMessage message) {
        _tempMessageBuffer.Reset();
        message.Serialize(_tempMessageBuffer);
        var size = _tempMessageBuffer.WriterPosition();
//        Debug.Log("serializing message with size " + size);
        _packetBuffer.WriteVariableUInt32((uint) size);
        _packetBuffer.Write(_tempMessageBuffer);
    }

    public LidgrenNetworkTransporter Transporter {
        set { _transporter = value; }
    }
}
