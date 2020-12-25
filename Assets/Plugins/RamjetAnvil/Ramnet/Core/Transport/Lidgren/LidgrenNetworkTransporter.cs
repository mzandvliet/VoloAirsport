using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace RamjetAnvil.RamNet {
    public class LidgrenNetworkTransporter : MonoBehaviour, IConnectionTransporter, IConnectionlessTransporter, ILatencyInfo {

        public event RequestApproval RequestApproval;
        public event OnConnectionEstablished OnConnectionOpened;
        public event OnDisconnected OnConnectionClosed;
        public event OnUnconnectedDataReceived OnUnconnectedDataReceived;
        public event OnDataReceived OnDataReceived;
        public event Action<string, IPEndPoint> OnNatPunchSuccess;

        private Action<ApprovalSecret> _approveConnection;
        private Action<ApprovalSecret> _denyConnection;

        private NetPeer _netPeer;
        private IPEndPoint _internalEndpoint;
        private TransporterStatus _status = TransporterStatus.Closed;
        private int _lastSampledFrame;

        private ConnectionIdPool _connectionIdPool; 
        private IDictionary<ApprovalSecret, NetConnection> _awaitingApprovals;
        private IDictionary<NetConnection, ConnectionId> _connections;
        private IDictionary<ConnectionId, NetConnection> _connectionsById;
        private LatencyInfo _latencyInfo;

        public void Open(NetPeerConfiguration config) {
            _approveConnection = ApproveConnection;
            _denyConnection = DenyConnection;

            config.UseMessageRecycling = true;
            config.NetworkThreadName = "LidgrenNetworkTransporter";

            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.EnableMessageType(NetIncomingMessageType.Error);

            _lastSampledFrame = -1;

            _awaitingApprovals = new Dictionary<ApprovalSecret, NetConnection>(config.MaximumConnections);
            _connections = new Dictionary<NetConnection, ConnectionId>(config.MaximumConnections);
            _connectionsById = new ArrayDictionary<ConnectionId, NetConnection>(_connectionIdPool.MaxConnectionIds);
            _latencyInfo = new LatencyInfo(_connectionIdPool.MaxConnectionIds);

            _netPeer = new NetPeer(config);
            _netPeer.Start();
            
            Debug.Log("internal endpoint is: " + UnityEngine.Network.player.ipAddress);
            _internalEndpoint = new IPEndPoint(IPAddress.Parse(UnityEngine.Network.player.ipAddress), _netPeer.Port);
            _status = TransporterStatus.Open;

            Debug.Log("Opened Lidgren transporter at " + _netPeer.Socket.LocalEndPoint);
        }

        public void Close() {
            if (_status == TransporterStatus.Open) {
                _netPeer.Shutdown("Close");
                var connectionIds = _connections.Values;
                foreach (var connectionId in connectionIds) {
                    if (OnConnectionClosed != null) {
                        OnConnectionClosed(connectionId);
                    }
                }
                _connections.Clear();
                _connectionsById.Clear();

                _status = TransporterStatus.Closed;
            }
        }

        public float GetLatency(ConnectionId connectionId) {
            return _latencyInfo.GetLatency(connectionId);
        }

        public IPEndPoint InternalEndpoint {
            get { return _internalEndpoint; }
        }

        public TransporterStatus Status {
            get { return _status; }
        }

        public ConnectionIdPool ConnectionIdPool {
            set { _connectionIdPool = value; }
        }

        public void Connect(ConnectionId connectionId, ApprovalSecret approvalSecret, IPEndPoint endpoint) {
            var approvalSecretMessage = _netPeer.CreateMessage();
            approvalSecretMessage.Write(approvalSecret.Value);
            var connection = _netPeer.Connect(endpoint, approvalSecretMessage);
            AddConnection(connectionId, connection);
        }

        public void Disconnect(ConnectionId connectionId) {
            if (connectionId != ConnectionId.NoConnection && 
                connectionId != ConnectionId.Self && 
                _status == TransporterStatus.Open) {
                NetConnection connection;
                if (_connectionsById.TryGetValue(connectionId, out connection)) {
                    connection.Disconnect("Disconnect");
                }
            }
        }

        public void SendUnconnected(IPEndPoint endPoint, NetBuffer buffer) {
            if (_status == TransporterStatus.Open) {
                var message = _netPeer.CreateMessage();
                if (buffer.LengthBytes > 0) {
                    message.Write(buffer);    
                }
                Console.WriteLine("Sending data " + buffer.ToHexString() + " to " + endPoint);
                _netPeer.SendUnconnectedMessage(message, endPoint);   
            }
        }

        private static readonly IPEndPoint DefaultLocalEndpoint = new IPEndPoint(IPAddress.None, 0);
        public void Send(ConnectionId connectionId, NetDeliveryMethod deliveryMethod, NetBuffer buffer) {
            if (connectionId == ConnectionId.Self) {
                if (OnDataReceived != null) {
                    var localEndpoint = _status == TransporterStatus.Open
                        ? _netPeer.Socket.LocalEndPoint as IPEndPoint
                        : DefaultLocalEndpoint;
                    OnDataReceived(ConnectionId.Self, localEndpoint, buffer);
                }
            } else {
                var connection = _connectionsById[connectionId];
                if (connection != null && connection.Status == NetConnectionStatus.Connected) {
                    var message = _netPeer.CreateMessage();
                    //Debug.Log("sending data " + buffer.Data.ToHexString(0, buffer.LengthBytes) + " to " + connection.RemoteEndPoint);
                    message.Write(buffer);
                    _netPeer.SendMessage(message, connection, deliveryMethod);
                }
            }
        }

        public void Flush() {
            _netPeer.FlushSendQueue();
        }

        void FixedUpdate() {
            Receive();
        }

        void Update() {
            Receive();
        }

        void Receive() {
            if (_lastSampledFrame >= Time.renderedFrameCount || _status == TransporterStatus.Closed) {
                return;
            }

            NetIncomingMessage msg;
            while ((msg = _netPeer.ReadMessage()) != null) {
                //Debug.Log("incoming message of type " + msg.MessageType + " from " + msg.SenderEndPoint);
                ConnectionId connectionId;
                switch (msg.MessageType) {
                    case NetIncomingMessageType.Error:
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        var approvalSecret = new ApprovalSecret(msg.ReadString());
                        if (RequestApproval != null) {
                            _awaitingApprovals.Add(approvalSecret, msg.SenderConnection);
                            connectionId = _connectionIdPool.Take();
                            AddConnection(connectionId, msg.SenderConnection);
                            RequestApproval(connectionId, approvalSecret, _approveConnection, _denyConnection);
                        } else {
                            msg.SenderConnection.Approve();
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        var connectionStatus = (NetConnectionStatus)msg.ReadByte();
                        //Debug.Log("connection status " + connectionStatus);
						switch (connectionStatus) {
						    case NetConnectionStatus.None:
						    case NetConnectionStatus.ReceivedInitiation:
						    case NetConnectionStatus.RespondedAwaitingApproval:
						    case NetConnectionStatus.RespondedConnect:
                            case NetConnectionStatus.InitiatedConnect:
                            case NetConnectionStatus.Disconnecting:
                                break;
                            case NetConnectionStatus.Connected:
						        if (!_connections.TryGetValue(msg.SenderConnection, out connectionId)) {
						            connectionId = _connectionIdPool.Take();
                                    AddConnection(connectionId, msg.SenderConnection);
						        } 
                                
                                Debug.Log("Connection opened to " + msg.SenderConnection.RemoteEndPoint + " with connection id " +
						                      connectionId);

						        if (OnConnectionOpened != null) {
						            OnConnectionOpened(connectionId, msg.SenderEndPoint);
						        }
						        break;
                            case NetConnectionStatus.Disconnected:
						        if (_connections.TryGetValue(msg.SenderConnection, out connectionId)) {
                                    Debug.Log("Disconnected: " + connectionId);
						            RemoveConnection(msg.SenderConnection);
						            if (OnConnectionClosed != null) {
						                OnConnectionClosed(connectionId);
						            }
						        }
						        break;
							default:
                                Debug.LogError("Unhandled connection status: " + connectionStatus);
								break;
						}
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        Console.WriteLine("Receiving unconnected data from " + msg.SenderEndPoint + ": " + msg.ToHexString());
                        if (OnUnconnectedDataReceived != null) {
                            OnUnconnectedDataReceived(msg.SenderEndPoint, msg);    
                        }
                        break;
//                    case NetIncomingMessageType.ConnectionApproval:
//                        Debug.Log("Approving connection to " + msg.SenderEndPoint);
//                        msg.SenderConnection.Approve();
//                        break;
                    case NetIncomingMessageType.Data:
                        if (OnDataReceived != null) {
                            connectionId = (ConnectionId) msg.SenderConnection.Tag;
                            OnDataReceived(connectionId, msg.SenderEndPoint, msg);
                        }
                        break;
                    case NetIncomingMessageType.NatIntroductionSuccess:
                        var token = msg.ReadString();
                        if (OnNatPunchSuccess != null) {
                            OnNatPunchSuccess(token, msg.SenderEndPoint);
                        }
                        break;
                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                        connectionId = (ConnectionId) msg.SenderConnection.Tag;
                        var roundtripTime = msg.ReadFloat();
                        var latency = roundtripTime * 0.5f;
                        _latencyInfo.UpdateLatency(connectionId, latency);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Console.WriteLine("LIDGREN (trace): " + msg.ReadString());
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine("LIDGREN (info): " + msg.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Console.WriteLine("LIDGREN (warning): " + msg.ReadString());
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine("LIDGREN (error): " + msg.ReadString());
                        break;
                    default:
                        Console.WriteLine("LIDGREN (warning): Unhandled message type: " + msg.MessageType);
                        break;
                }
                _netPeer.Recycle(msg);
            }

            _lastSampledFrame = Time.renderedFrameCount;
        }

        private void AddConnection(ConnectionId id, NetConnection connection) {
            connection.Tag = id;
            _connections.Add(connection, id);
            _connectionsById.Add(id, connection);
        }

        private void RemoveConnection(NetConnection connection) {
            var connectionId = _connections[connection];
            _connectionIdPool.Put(connectionId);
            _connections.Remove(connection);
            _connectionsById.Remove(connectionId);
            _latencyInfo.UpdateLatency(connectionId, 0f);
        }

        private void ApproveConnection(ApprovalSecret secret) {
            NetConnection connection;
            if (_awaitingApprovals.TryGetValue(secret, out connection)) {
                Console.WriteLine("approving connection " + connection.RemoteEndPoint);
                connection.Approve();
            }
            _awaitingApprovals.Remove(secret);
        }

        private void DenyConnection(ApprovalSecret secret) {
            NetConnection connection;
            if (_awaitingApprovals.TryGetValue(secret, out connection)) {
                ConnectionId connectionId;
                if (_connections.TryGetValue(connection, out connectionId)) {
                    RemoveConnection(connection);
                    if (OnConnectionClosed != null) {
						OnConnectionClosed(connectionId);
					}
                }
                Console.WriteLine("denying connection " + connection.RemoteEndPoint);
                connection.Deny();
            }
            _awaitingApprovals.Remove(secret);
        }

        void OnDestroy() {
            Close();
        }
    }
}
