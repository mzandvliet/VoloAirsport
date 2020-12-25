using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.RamNet {
    public class DefaultConnectionManager : IConnectionManager {
        public event RequestApproval RequestApproval;

        public enum ConnectionStatus { Pending, Established, Cancelled }

        private static readonly OnConnectionEstablished EmptyOnConnectionEstablished = (id, point) => { };
        private static readonly OnConnectionFailure EmptyOnConnectionFailure = (id, exception) => { };
        private static readonly OnDisconnected EmptyOnDisconnected = id => { };

        private readonly IConnectionTransporter _transporter;

        private readonly ConnectionIdPool _connectionIdPool;
        private readonly IDictionary<ConnectionId, ConnectionRegistration> _connections; 

        public DefaultConnectionManager(IConnectionTransporter transporter, ConnectionIdPool connectionIdPool) {
            _transporter = transporter;
            _connectionIdPool = connectionIdPool;
            _connections = new ArrayDictionary<ConnectionId, ConnectionRegistration>(connectionIdPool.MaxConnectionIds);

            _transporter.RequestApproval += PassRequestApproval;
            _transporter.OnConnectionOpened += OnConnectionOpened;
            _transporter.OnConnectionClosed += OnConnectionClosed;
        }

        public ConnectionId Connect(IPEndPoint hostEndpoint, 
            ApprovalSecret approvalSecret, 
            OnConnectionEstablished onConnectionEstablished = null,
            OnConnectionFailure onConnectionFailure = null, 
            OnDisconnected onDisconnected = null) {

            var connectionId = _connectionIdPool.Take();
            _transporter.Connect(connectionId, approvalSecret, hostEndpoint);
            _connections[connectionId] = new ConnectionRegistration {
                Endpoint = hostEndpoint,
                OnConnectionEstablished = onConnectionEstablished ?? EmptyOnConnectionEstablished,
                OnConnectionFailure = onConnectionFailure ?? EmptyOnConnectionFailure,
                OnDisconnected = onDisconnected ?? EmptyOnDisconnected,
                Status = ConnectionStatus.Pending,
            };
            return connectionId;
        }

        public void Disconnect(ConnectionId connectionId) {
            _transporter.Disconnect(connectionId);
        }

        public void CancelPending(ConnectionId connectionId) {
            ConnectionRegistration registration;
            if (_connections.TryGetValue(connectionId, out registration)) {
                registration.Status = ConnectionStatus.Cancelled;
                _connections[connectionId] = registration;
            }
        }

        public void Dispose() {
            _transporter.RequestApproval -= PassRequestApproval;
            _transporter.OnConnectionOpened -= OnConnectionOpened;
            _transporter.OnConnectionClosed -= OnConnectionClosed;
        }
        
        private void OnConnectionOpened(ConnectionId connectionId, IPEndPoint endPoint) {
            ConnectionRegistration registration;
            if (_connections.TryGetValue(connectionId, out registration)) {
                if (registration.Status == ConnectionStatus.Cancelled) {
                    Debug.Log("Cancelled connection " + connectionId);
                    RemoveRegistration(connectionId);
                    _transporter.Disconnect(connectionId);
                } else {
                    registration.Status = ConnectionStatus.Established;
                    _connections[connectionId] = registration;
                    registration.OnConnectionEstablished(connectionId, endPoint);
                }
            }
        }

        private void OnConnectionClosed(ConnectionId connectionId) {
            ConnectionRegistration registration;
            if (_connections.TryGetValue(connectionId, out registration)) {
                if (registration.Status == ConnectionStatus.Established) {
                    registration.OnDisconnected(connectionId);
                } else if (registration.Status == ConnectionStatus.Pending) {
                    // TODO Fill Exception
                    registration.OnConnectionFailure(registration.Endpoint, exception: null);
                }

                RemoveRegistration(connectionId);
            }
        }

        private void RemoveRegistration(ConnectionId connectionId) {
            _connections.Remove(connectionId);
            _connectionIdPool.Put(connectionId);
        }

        private void PassRequestApproval(
            ConnectionId connectionId,
            ApprovalSecret secret, 
            Action<ApprovalSecret> approve,
            Action<ApprovalSecret> deny) {

            if (RequestApproval != null) {
                RequestApproval(connectionId, secret, approve, deny);
            } else {
                approve(secret);
            }
        }

        private struct ConnectionRegistration {
            public IPEndPoint Endpoint;
            public OnConnectionEstablished OnConnectionEstablished;
            public OnConnectionFailure OnConnectionFailure;
            public OnDisconnected OnDisconnected;
            public ConnectionStatus Status;
        }
    }
}
