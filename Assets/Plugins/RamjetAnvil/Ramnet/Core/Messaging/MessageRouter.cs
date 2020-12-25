using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    public class MessageMetaData {
        public ConnectionId ConnectionId;
        public IPEndPoint EndPoint;
        public MessageType MessageType;
        public SequenceNumber SequenceNumber;
        public float Latency;
    }
    
    public delegate void MessageHandler<T>(ConnectionId connectionId, IPEndPoint endpoint, T message, NetBuffer reader);

    public class MessageRouter : ITransportDataHandler {
        private readonly IReadOnlyDictionary<Type, MessageType> _networkMessageTypes; 
        private readonly IDictionary<MessageType, Action<MessageMetaData, NetBuffer>> _handlers;
        private readonly MessageMetaData _messageMetaData;
        private readonly ILatencyInfo _latencyInfo;

        public MessageRouter(IReadOnlyDictionary<Type, MessageType> networkMessageTypes, ILatencyInfo latencyInfo) {
            _messageMetaData = new MessageMetaData();
            _networkMessageTypes = networkMessageTypes;
            _latencyInfo = latencyInfo;
            _handlers = new ArrayDictionary<MessageType, Action<MessageMetaData, NetBuffer>>(_networkMessageTypes.Count);

//            foreach (var networkMessageType in _networkMessageTypes) {
//                Debug.Log("network message type: " + networkMessageType.Key + " has id: " + networkMessageType.Value);
//            }
        }

        public void OnDataReceived(ConnectionId connectionId, IPEndPoint endpoint, NetBuffer reader) {
            _messageMetaData.SequenceNumber = reader.ReadSequenceNumber();
            _messageMetaData.ConnectionId = connectionId;
            _messageMetaData.EndPoint = endpoint;
            _messageMetaData.Latency = _latencyInfo.GetLatency(connectionId);

            //Debug.Log("receiving data from " + endpoint);
            while (reader.PositionInBytes < reader.LengthBytes) {
                var messageSize = reader.ReadVariableUInt32();
                var messageStartPosition = reader.PositionInBytes;
                var messageType = reader.ReadMessageType();

                _messageMetaData.MessageType = messageType;

                Action<MessageMetaData, NetBuffer> handler;
                if (_handlers.TryGetValue(messageType, out handler)) {
                    handler(_messageMetaData, reader);
                }
                // Skip bytes that weren't read
                var bytesRead = reader.PositionInBytes - messageStartPosition;
                var bytesToSkip = messageSize - bytesRead;
                if (bytesRead > messageSize) {
                    // TODO How to handle this error?
                    throw new Exception("Error! Bytes read (" + bytesRead + ") > messageSizeInBytes (" + messageSize + ")" + ", reader posInBytes: " + reader.PositionInBytes);
                }
                reader.SkipBytes(bytesToSkip);
            }
        }
        
        public MessageRouter RegisterHandler<T>(Action<MessageMetaData, T, NetBuffer> handler) where T : IMessage, new() {
            var message = new T();
            var messageTypeId = _networkMessageTypes[typeof (T)];
            _handlers[messageTypeId] = (messageMetaData, reader) => {
                message.Deserialize(reader);
                handler(messageMetaData, message, reader);
            };
            return this;
        }

        public void ClearHandlers() {
            _handlers.Clear();
        }
    }

    public static class BasicMessageRouterExtensions {
        
        public static MessageRouter RegisterHandler<T>(this MessageRouter router, Action<T> handler) where T : IMessage, new() {
            return router.RegisterHandler<T>((metadata, msg, reader) => handler(msg));
        }

        public static MessageRouter RegisterHandler<T>(this MessageRouter router, Action<MessageMetaData, T> handler) where T : IMessage, new() {
            return router.RegisterHandler<T>((metadata, msg, reader) => handler(metadata, msg));
        }
    }
}