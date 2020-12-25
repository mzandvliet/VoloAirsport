using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    public enum NatFacilitatorRequestType : byte {
        RegisterPeer = 0,
        UnregisterPeer = 1,
        RequestIntroduction = 2,
        RequestExternalEndpoint = 3
    }

    public enum NatFacilitatorMessageType : byte {
        HostNotRegistered = 0,
        PeerRegistrationSuccess = 1
    }

    public class LidgrenPunchThroughFacilitator : IConnectionManager {
        public event RequestApproval RequestApproval;

        private static readonly OnConnectionEstablished EmptyOnConnectionEstablished = (id, point) => { };
        private static readonly OnConnectionFailure EmptyOnConnectionFailure = (endpoint, exception) => { };
        private static readonly OnDisconnected EmptyOnDisconnected = id => { };

        private readonly IObjectPool<ConnectionRegistration> _connectionRegistrationPool;

        private readonly float _connectionAttemptTimeout;
        private readonly LidgrenNetworkTransporter _transporter;
        private readonly LidgrenNatPunchClient _natPunchClient;

        private readonly ConnectionIdPool _connectionIdPool;
        private readonly IDictionary<NatPunchId, IPooledObject<ConnectionRegistration>> _punchAttempts;
        private readonly ConnectionId[] _cancelledAttempts; 
        private readonly IDictionary<ConnectionId, IPooledObject<ConnectionRegistration>> _connectionAttempts;
        private readonly IDictionary<ConnectionId, IPooledObject<ConnectionRegistration>> _activeConnections;

        private readonly IDisposable _cleanupRoutine;

        public LidgrenPunchThroughFacilitator(LidgrenNetworkTransporter transporter, 
            ICoroutineScheduler coroutineScheduler, LidgrenNatPunchClient natPunchClient, float connectionAttemptTimeout, 
            ConnectionIdPool connectionIdPool) {

            _connectionIdPool = connectionIdPool; 
            _connectionRegistrationPool = new ObjectPool<ConnectionRegistration>(() => new ConnectionRegistration());

            _punchAttempts = new Dictionary<NatPunchId, IPooledObject<ConnectionRegistration>>();
            _connectionAttempts = new ArrayDictionary<ConnectionId, IPooledObject<ConnectionRegistration>>(
                connectionIdPool.MaxConnectionIds);
            _activeConnections = new ArrayDictionary<ConnectionId, IPooledObject<ConnectionRegistration>>(
                connectionIdPool.MaxConnectionIds);

            _cancelledAttempts = new ConnectionId[_connectionIdPool.MaxConnectionIds];
            for (int i = 0; i < _cancelledAttempts.Length; i++) {
                _cancelledAttempts[i] = ConnectionId.NoConnection;
            }

            _transporter = transporter;
            _connectionAttemptTimeout = connectionAttemptTimeout;
            _connectionIdPool = connectionIdPool;
            _natPunchClient = natPunchClient;

            _transporter.RequestApproval += PassRequestApproval;
            _transporter.OnConnectionOpened += OnConnectionOpened;
            _transporter.OnConnectionClosed += OnConnectionClosed;

            _cleanupRoutine = coroutineScheduler.Run(PunchTimeoutCleanup());
        }

        public ConnectionId Connect(IPEndPoint hostEndpoint, 
            ApprovalSecret approvalSecret,
            OnConnectionEstablished onConnectionEstablished = null, 
            OnConnectionFailure onConnectionFailure = null, 
            OnDisconnected onDisconnected = null) {

            var connectionId = _connectionIdPool.Take();
            var connectionRegistration = _connectionRegistrationPool.Take();
            connectionRegistration.Instance.Timestamp = Time.realtimeSinceStartup;
            connectionRegistration.Instance.ConnectionId = connectionId;
            connectionRegistration.Instance.ApprovalSecret = approvalSecret;
            connectionRegistration.Instance.PublicEndpoint = hostEndpoint;
            connectionRegistration.Instance.OnConnectionEstablished = onConnectionEstablished ?? EmptyOnConnectionEstablished;
            connectionRegistration.Instance.OnConnectionFailure = onConnectionFailure ?? EmptyOnConnectionFailure;
            connectionRegistration.Instance.OnDisconnected = onDisconnected ?? EmptyOnDisconnected;

            var punchId = _natPunchClient.Punch(hostEndpoint, OnPunchSuccess, OnPunchFailure);

            _punchAttempts.Add(punchId, connectionRegistration);

            return connectionId;
        }

        public void Disconnect(ConnectionId connectionId) {
            _transporter.Disconnect(connectionId);
        }

        public void CancelPending(ConnectionId connectionId) {
            _cancelledAttempts[connectionId.Value] = connectionId;
        }

        private void OnPunchSuccess(NatPunchId punchId, IPEndPoint endPoint) {
            IPooledObject<ConnectionRegistration> registration;
            if (_punchAttempts.TryGetValue(punchId, out registration)) {
                //Debug.Log("NAT introduction succeeded to " + endPoint);

                if (IsCancelled(registration.Instance)) {
                    CancelPunchAttempt(punchId, registration.Instance);
                } else {
                    RemovePunchAttempt(punchId);
                    registration.Instance.ConnectionEndpoint = endPoint;
                    var connectionId = registration.Instance.ConnectionId;
                    _transporter.Connect(connectionId, registration.Instance.ApprovalSecret, endPoint);
                    AddConnectionAttempt(registration);
                }
            }
        }

        private void OnPunchFailure(NatPunchId punchId) {
            IPooledObject<ConnectionRegistration> registration;
            if (_punchAttempts.TryGetValue(punchId, out registration)) {
                if (!IsCancelled(registration.Instance)) {
                    // TODO Add punch exception
                    registration.Instance.OnConnectionFailure(registration.Instance.PublicEndpoint, exception: null);
                }
                CancelPunchAttempt(punchId, registration.Instance);
            }
        }

        private void OnConnectionOpened(ConnectionId connectionId, IPEndPoint endpoint) {
            IPooledObject<ConnectionRegistration> registration;
            if (_connectionAttempts.TryGetValue(connectionId, out registration)) {
                if (IsCancelled(registration.Instance)) {
                    CancelConnectionAttempt(connectionId);
                    _transporter.Disconnect(connectionId);
                } else {
                    registration.Instance.OnConnectionEstablished(connectionId, endpoint);
                    RemoveConnectionAttempt(connectionId);
                    _activeConnections.Add(connectionId, registration);    
                }
            }
        }

        private void OnConnectionClosed(ConnectionId connectionId) {
            IPooledObject<ConnectionRegistration> connectionRegistration;
            if (_activeConnections.TryGetValue(connectionId, out connectionRegistration)) {
                connectionRegistration.Instance.OnDisconnected(connectionId);
                _activeConnections.Remove(connectionId);
            } else if (_connectionAttempts.TryGetValue(connectionId, out connectionRegistration)) {
                if (!IsCancelled(connectionRegistration.Instance)) {
                    // TODO Fill exception
                    connectionRegistration.Instance.OnConnectionFailure(connectionRegistration.Instance.PublicEndpoint, exception: null);    
                }
                CancelConnectionAttempt(connectionId);
            }
            _connectionIdPool.Put(connectionId);
        }

        private IEnumerator<WaitCommand> PunchTimeoutCleanup() {
            var cancellableRegistrations = new List<ConnectionId>();
            while (true) {
                cancellableRegistrations.Clear();
                var connectionAttemptRegistrations = _punchAttempts.Values.ToListOptimized();
                for (int i = 0; i < connectionAttemptRegistrations.Count; i++) {
                    var registration = connectionAttemptRegistrations[i];
                    if (registration.Instance.Timestamp + _connectionAttemptTimeout < Time.realtimeSinceStartup ||
                        IsCancelled(registration.Instance)) {

                        registration.Instance.OnConnectionFailure(registration.Instance.PublicEndpoint, exception: null);
                        cancellableRegistrations.Add(registration.Instance.ConnectionId);
                    }
                }

                for (int i = 0; i < cancellableRegistrations.Count; i++) {
                    var connectionId = cancellableRegistrations[i];
                    CancelConnectionAttempt(connectionId);
                }

                yield return WaitCommand.WaitSeconds(_connectionAttemptTimeout);
            }
        } 

        private void AddConnectionAttempt(IPooledObject<ConnectionRegistration> registration) {
            _connectionAttempts.Add(registration.Instance.ConnectionId, registration);
        }

        private bool IsCancelled(ConnectionRegistration registration) {
            return _cancelledAttempts[registration.ConnectionId.Value] == registration.ConnectionId;
        }

        private void RemoveConnectionAttempt(ConnectionId connectionId) {
            IPooledObject<ConnectionRegistration> registration;
            if (_connectionAttempts.TryGetValue(connectionId, out registration)) {
                _connectionAttempts.Remove(connectionId);
                registration.Dispose();
            }
        }

        private void CancelConnectionAttempt(ConnectionId connectionId) {
            RemoveConnectionAttempt(connectionId);
            _connectionIdPool.Put(connectionId);
            _cancelledAttempts[connectionId.Value] = ConnectionId.NoConnection;
        }

        private void RemovePunchAttempt(NatPunchId punchId) {
            _punchAttempts.Remove(punchId);
        }

        private void CancelPunchAttempt(NatPunchId punchId, ConnectionRegistration registration) {
            _punchAttempts.Remove(punchId);
            _cancelledAttempts[registration.ConnectionId.Value] = ConnectionId.NoConnection;
            _connectionIdPool.Put(registration.ConnectionId);
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

        public void Dispose() {
            _transporter.RequestApproval -= PassRequestApproval;
            _transporter.OnConnectionOpened -= OnConnectionOpened;
            _transporter.OnConnectionClosed -= OnConnectionClosed;

            _cleanupRoutine.Dispose();
        }

        private class ConnectionRegistration {
            public float Timestamp;
            public ConnectionId ConnectionId;
            public IPEndPoint PublicEndpoint;
            public IPEndPoint ConnectionEndpoint;
            public ApprovalSecret ApprovalSecret;
            public OnConnectionEstablished OnConnectionEstablished;
            public OnConnectionFailure OnConnectionFailure;
            public OnDisconnected OnDisconnected;
        }
    }
}
