using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Padrone.Client;
using UnityEngine;

namespace RamjetAnvil.Volo.Networking {
    public class VoloNetworkClient : MonoBehaviour {

        [SerializeField] private UnityMasterServerClient _masterServerClient;
        [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;

        [SerializeField] private AbstractUnityClock _localRealtimeClock;
        [SerializeField, Dependency("gameClock")] private AbstractUnityClock _gameClock;
        [SerializeField, Dependency("fixedClock")] private AbstractUnityClock _fixedClock;
        [SerializeField] private LidgrenNetworkTransporter _transporter;
        private NetworkSystems _networkSystems;

        private ConnectionId _hostConnectionId;
        private ClientSessionId? _sessionId;
        private IDisposable _pingRoutine;

        private bool _isJoinInProgress;

        private ushort _frameId;
        private IList<OutstandingPing> _outstandingPings;
        private LatencyInfo _latencyInfo;

        void Awake() {
            _outstandingPings = new List<OutstandingPing>(10);
            _hostConnectionId = ConnectionId.NoConnection;
        }

        public IEnumerator<WaitCommand> Join(IPEndPoint endPoint,
            int clientPort = -1,
            OnConnectionEstablished onEstablished = null,
            OnConnectionFailure onFailure = null, 
            OnDisconnected onDisconnected = null) {

            _outstandingPings.Clear();
            _frameId = 0;
            _isJoinInProgress = true;

            var sessionIdResult = new AsyncResult<JoinResponse>();
            string password = null;
            _masterServerClient.Client.Join(endPoint, password, (statusCode, s) => {
                sessionIdResult.SetResult(s);
            });

            while (!sessionIdResult.IsResultAvailable) {
                yield return WaitCommand.WaitForNextFrame;
            }

            if (sessionIdResult.Result != null) {
                _sessionId = sessionIdResult.Result.SessionId;

                var netConfig = new NetPeerConfiguration(NetworkConfig.GameId);
                // Kind of arbitrary, we need to have enough room for connection attempts 
                // and cancellation running simultaneously but we don't need to have an unlimited number
                netConfig.MaximumConnections = 16;
                if (clientPort > -1) {
                    netConfig.Port = clientPort;
                }
                _transporter.Open(netConfig);

                var approvalSecret = new ApprovalSecret(_sessionId.Value.Value);
                _hostConnectionId = _networkSystems.ConnectionManager.Connect(endPoint,
                    approvalSecret,
                    (connId, endpoint) => {
                        _isJoinInProgress = false;
                        OnConnectionEstablished(connId, endpoint);
                        if (onEstablished != null) onEstablished(connId, endpoint);
                    },
                    (endpoint, exception) => {
                        _isJoinInProgress = false;
                        OnConnectionFailure(endpoint, exception);
                        if (onFailure != null) onFailure(endpoint, exception);
                    },
                    connId => {
                        _isJoinInProgress = false;
                        OnDisconnected(connId);
                        if (onDisconnected != null) onDisconnected(connId);
                    });
            } else {
                OnConnectionFailure(endPoint, new Exception("Master server did not allow join on " + endPoint));
            }
        }

        public IEnumerator<WaitCommand> Leave() {
            if (_isJoinInProgress) {
                _networkSystems.ConnectionManager.CancelPending(_hostConnectionId);
            } else if (_hostConnectionId.IsRemote) {
                _networkSystems.ConnectionManager.Disconnect(_hostConnectionId);

                var leaveResult = new AsyncResult<HttpStatusCode>();
                _masterServerClient.Client.Leave(statusCode => leaveResult.SetResult(statusCode));

                while (!leaveResult.IsResultAvailable) {
                    yield return WaitCommand.WaitForNextFrame;
                }
            }

            _hostConnectionId = ConnectionId.NoConnection;
            _sessionId = null;
        }

        private void OnConnectionEstablished(ConnectionId hostConnectionId, IPEndPoint endPoint) {
            Debug.Log("Connection established to " + endPoint + " with " + hostConnectionId);
            var preExistingObjects = PreExistingObjects.FindAll();

            var messageRouter = _networkSystems.DefaultMessageRouter;
            NetworkSystem.InstallBasicClientHandlers(messageRouter, _networkSystems, preExistingObjects);
            messageRouter.RegisterHandler<BasicMessage.Pong>(OnClockSync);
            _networkSystems.GroupRouter.SetDataHandler(ConnectionGroups.Default, messageRouter);

            _pingRoutine = _coroutineScheduler.Run(PingRepeatedly(hostConnectionId));
        }

        private void OnDisconnected(ConnectionId connectionId) {
            _hostConnectionId = ConnectionId.NoConnection;
            Debug.Log(connectionId + " got disconnected");
            if (_pingRoutine != null) {
                _pingRoutine.Dispose();
            }
            _networkSystems.DefaultMessageRouter.ClearHandlers();
            _networkSystems.ObjectStore.ClearStore();
        }

        private void OnConnectionFailure(IPEndPoint endpoint, Exception exception) {
            _hostConnectionId = ConnectionId.NoConnection;
            Debug.LogError("Connection failure on " + endpoint + " exception: " + exception);
        }

        private IEnumerator<WaitCommand> PingRepeatedly(ConnectionId hostId) {
            while (true) {
                var pingMessage = _networkSystems.MessagePool.GetMessage<BasicMessage.Ping>();
                pingMessage.Content.FrameId = _frameId;

                //Debug.Log("sending ping in frame " + Time.frameCount + " with frame id " + _frameId + " current time " + _localRealtimeClock.CurrentTime);
                _outstandingPings.Add(new OutstandingPing(_frameId, _localRealtimeClock.CurrentTime));
                _frameId++;

                _networkSystems.MessageSender.Send(hostId, pingMessage);
                yield return WaitCommand.WaitSeconds(1f);
            }
        }

        private float _roundTripTime;

        private void OnClockSync(MessageMetaData metadata, BasicMessage.Pong pong) {
            bool isPongReceived = false;
            for (int i = _outstandingPings.Count - 1; i >= 0; i--) {
                var outstandingPing = _outstandingPings[i];

                // Handle the ping frame, set the clocks remove any old outstanding pings
                if (outstandingPing.FrameId == pong.FrameId) {
                    _roundTripTime = (float) (_localRealtimeClock.CurrentTime - outstandingPing.Timestamp);
                    var latency = _roundTripTime / 2f;
                    // TODO Add latency smoothing
                    _latencyInfo.UpdateLatency(metadata.ConnectionId, latency);

                    isPongReceived = true;
                }

                if (isPongReceived) {
                    _outstandingPings.RemoveAt(i);
                }
            }
        }

        public NetworkSystems NetworkSystems {
            set { _networkSystems = value; }
        }

        public LatencyInfo LatencyInfo {
            set { _latencyInfo = value; }
        }

        public struct OutstandingPing {
            public ushort FrameId;
            public double Timestamp;

            public OutstandingPing(ushort frameId, double timestamp) {
                FrameId = frameId;
                Timestamp = timestamp;
            }
        }

        private void OnGUI() {
            if (_hostConnectionId != ConnectionId.NoConnection) {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(string.Format("Render time {0:0.000}", _gameClock.CurrentTime));
                GUILayout.Label(string.Format("Fixed time {0:0.000}", _fixedClock.CurrentTime));
                GUILayout.Label(string.Format("RTT {0:0.000}", _roundTripTime));
                GUILayout.EndVertical();
            }
        }
    }
}
