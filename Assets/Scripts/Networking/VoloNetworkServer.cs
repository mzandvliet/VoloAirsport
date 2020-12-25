using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Padrone.Client;
using UnityEngine;

namespace RamjetAnvil.Volo.Networking {

    // TODO Implement report leave and unregistering clients from internal client sessions list

    public class VoloNetworkServer : MonoBehaviour {

        [SerializeField] private UnityMasterServerClient _masterServerClient;
        [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;

        [SerializeField, Dependency("gameClock")] private AbstractUnityClock _gameClock;
        [SerializeField, Dependency("fixedClock")] private AbstractUnityClock _fixedClock;
        [SerializeField] private VersionInfo _versionInfo;
        [SerializeField] private bool _runOnLocalHost = false;

        [SerializeField] private LidgrenNetworkTransporter _transporter;

        private NetworkSystems _networkSystems;
        private LidgrenNatFacilitatorConnection _natFacilitatorConnection;
        private IDictionary<ConnectionId, PlayerInfo> _connectedClients;
        private IDictionary<ConnectionId, ClientSessionId> _clientSessions;
        private PlayerInfo _serverPlayerInfo;

        private IDisposable _masterServerRegistration;

        private bool _isHosting;

        void Awake() {
            _masterServerRegistration = Disposables.Empty;
            _connectedClients = new ArrayDictionary<ConnectionId, PlayerInfo>(256);
            _clientSessions = new ArrayDictionary<ConnectionId, ClientSessionId>(256);
        }
        
        public IPEndPoint InternalEndpoint {
            get { return _transporter.InternalEndpoint; }
        }

        public void HostAsSingleplayer() {
            _serverPlayerInfo = new PlayerInfo("", "", false, false);
            RegisterHandlers();
            ReplicatePreExistingInstances();
        }

        public void Host(string hostName, int maxPlayers, bool isPrivate = false, int port = -1) {
            RegisterHandlers();

            var hostConfig = new NetPeerConfiguration(NetworkConfig.GameId);
            hostConfig.MaximumConnections = Mathf.Clamp(maxPlayers - 1, 0, NetworkConfig.MaxConnections);
            hostConfig.AcceptIncomingConnections = true;
            if (port > -1) {
                hostConfig.Port = port;
            }
            _transporter.Open(hostConfig);

            _transporter.RequestApproval += RegisterPlayer;

            _coroutineScheduler.Run(_natFacilitatorConnection.Register());
            _masterServerRegistration = _coroutineScheduler.Run(RegisterAtMasterServer(hostName, maxPlayers, isPrivate));
            _networkSystems.GroupRouter.GetConnectionGroup(ConnectionGroups.Default)
                .PeerLeft += OnPeerLeft;

            ReplicatePreExistingInstances();

            _isHosting = true;
        }


        private void RegisterHandlers() {
            NetworkSystem.InstallBasicServerHandlers(_networkSystems.DefaultMessageRouter, _gameClock, _fixedClock, _networkSystems);
            _networkSystems.DefaultMessageRouter.RegisterHandler<GameMessages.RespawnPlayerRequest>(RespawnPlayer);
            _networkSystems.DefaultMessageRouter.RegisterHandler<GameMessages.DespawnPlayerRequest>(DespawnPlayer);
        }

        private void RegisterPlayer(ConnectionId connectionId, ApprovalSecret approvalSecret, Action<ApprovalSecret> approve, Action<ApprovalSecret> deny) {
            _coroutineScheduler.Run(RegisterPlayerInternal(connectionId, approvalSecret, approve, deny));
        }

        private IEnumerator<WaitCommand> RegisterPlayerInternal(
            ConnectionId connectionId,
            ApprovalSecret approvalSecret, 
            Action<ApprovalSecret> approve, 
            Action<ApprovalSecret> deny) {

            while (!_natFacilitatorConnection.ExternalEndpoint.IsResultAvailable) {
                yield return WaitCommand.WaitForNextFrame;
            }

            var externalEndpoint = _natFacilitatorConnection.ExternalEndpoint.Result;

            var playerSessionResult = new AsyncResult<Maybe<PlayerSessionInfo>>();
            var clientSessionId = new ClientSessionId(approvalSecret.Value);
            _masterServerClient.Client.GetPlayerInfo(externalEndpoint, clientSessionId,
                (statusCode, p) => {
                    if (statusCode == HttpStatusCode.OK) {
                        playerSessionResult.SetResult(Maybe.Just(p));
                    } else {
                        playerSessionResult.SetResult(Maybe.Nothing<PlayerSessionInfo>());
                    }
                });

            while (!playerSessionResult.IsResultAvailable) {
                yield return WaitCommand.WaitForNextFrame;
            }

            var playerSessionInfo = playerSessionResult.Result;
            if (playerSessionInfo.IsJust && !_clientSessions.Values.Contains(clientSessionId)) {
                var playerInfo = playerSessionInfo.Value.PlayerInfo;
                Debug.Log("'" + playerInfo.Name + "' connected");
                // TODO:
                // Store player info and use it to enhance the replicated pilot so
                // that players can see who's flying that pilot
                _connectedClients[connectionId] = playerInfo;
                _clientSessions[connectionId] = clientSessionId;
                approve(approvalSecret);
            } else {
                Debug.Log("Player never registered at the master server, or player was already registered. Denying connection");
                deny(approvalSecret);
            }
        }
        
        private void OnPeerLeft(ConnectionId connectionId) {
            ClientSessionId sessionId;
            if (_clientSessions.TryGetValue(connectionId, out sessionId)) {
                var externalEndpoint = _natFacilitatorConnection.ExternalEndpoint.Result;
                _masterServerClient.Client.ReportLeave(externalEndpoint, sessionId, statusCode => {});
            }
            _clientSessions.Remove(connectionId);
            _connectedClients.Remove(connectionId);
        }

        // TODO Maybe this is logic that can be moved to the Ramnet library?
        private void ReplicatePreExistingInstances() {
            var preExistingObjects = PreExistingObjects.FindAll();
            foreach (var preExistingObject in preExistingObjects) {
                var gameObject = preExistingObject.Value;
                //Debug.Log("replicating existing instance " + gameObject + " under id " + preExistingObject.Key);
                _networkSystems.Replicator.AddPreExistingInstance(ConnectionId.Self, gameObject, preExistingObject.Key);
            }
        }

        private readonly IList<ReplicatedObject> _replicatedObjectsCache = new List<ReplicatedObject>(); 

        public void Stop() {
            _networkSystems.Replicator.RemoveAllReplicatedInstances();
            _networkSystems.DefaultMessageRouter.ClearHandlers();

            _networkSystems.GroupRouter.GetConnectionGroup(ConnectionGroups.Default)
                .PeerLeft -= OnPeerLeft;

            if (_transporter.Status == TransporterStatus.Open) {
                _transporter.Close();    
                _transporter.RequestApproval -= RegisterPlayer;
                
                _masterServerClient.Client.UnregisterHost(_natFacilitatorConnection.ExternalEndpoint.Result, code => {});
                _natFacilitatorConnection.Unregister();
                _masterServerRegistration.Dispose();
            }

            _connectedClients.Clear();
            _clientSessions.Clear();

            _isHosting = false;
        }

        public void OnConnectionEstablished(ConnectionId newPeerConnectionId, IPEndPoint endpoint) {
            //CreateWhisp(newPeerConnectionId);
        }

        private void RespawnPlayer(MessageMetaData metadata, GameMessages.RespawnPlayerRequest respawnRequest) {
            var ownerConnectionId = metadata.ConnectionId;

            PlayerInfo playerInfo;
            if (ownerConnectionId == ConnectionId.Self) {
                playerInfo = _serverPlayerInfo;
            } else if (!_connectedClients.TryGetValue(ownerConnectionId, out playerInfo)) {
                Debug.LogWarning("unable to find player info for " + ownerConnectionId);
                playerInfo = new PlayerInfo("", "", false, false);
            }

            DespawnPlayer(metadata, null);

            var pilot = _networkSystems.Replicator.Instantiate(ReplicationObjects.Pilot, ownerConnectionId,
            respawnRequest.Spawnpoint.Position, respawnRequest.Spawnpoint.Rotation);

            //PlayerPilotSpawner.JumpForward(respawnRequest, pilot);

            // TODO Communicate player information to the player instance such that we can
            // render name tags and avatars etc.
            //            var playerInfoMessage = new PilotMessage.SetPlayerInfo();
            //            playerInfoMessage.PlayerInfo = playerInfo;
            //            pilot.GameObject.GetComponentInChildren<PilotPlayerInfoView>(includeInactive: true).SetPlayerInfo(playerInfoMessage);

            _networkSystems.Replicator.Activate(pilot);
        }

        private void DespawnPlayer(MessageMetaData metadata, GameMessages.DespawnPlayerRequest despawnRequest) {
            var ownerConnectionId = metadata.ConnectionId;
            _replicatedObjectsCache.Clear();
            _networkSystems.ObjectStore.FindObjects(ReplicationObjects.Pilot, _replicatedObjectsCache);
            _replicatedObjectsCache.FilterOwner(ownerConnectionId);
            if (_replicatedObjectsCache.Count > 0) {
                var existingPilot = _replicatedObjectsCache[0];
                _networkSystems.Replicator.RemoveReplicatedInstance(existingPilot.Id);
            }
        }

//        private void CreateWhisp(ConnectionId ownerConnectionId) {
//            var whisp = _networkSystems.Replicator.Instantiate(ReplicationObjects.Whisp, ownerConnectionId,
//                _pilotSpawnpoint.position);
//            _networkSystems.Replicator.Activate(whisp);
//        }

        private IEnumerator<WaitCommand> RegisterAtMasterServer(string hostName, int maxPlayers, bool isPrivate = false) {
            while (!_natFacilitatorConnection.ExternalEndpoint.IsResultAvailable) {
                yield return WaitCommand.WaitForNextFrame;
            }

            var isRegistrationSuccessful = false;
            while (true) {
                var externalEndpoint = _natFacilitatorConnection.ExternalEndpoint.Result;

                while (!isRegistrationSuccessful) {
                    var registrationConfirmation = new AsyncResult<HttpStatusCode>();
                    var endpoint = _runOnLocalHost
                        ? new IPEndPoint(IPAddress.Parse("127.0.0.1"), externalEndpoint.Port)
                        : externalEndpoint;

                    Debug.Log("External endpoint is: " + endpoint);
                    string password = null;
                    var request = new HostRegistrationRequest(hostName,
                        new PeerInfo(endpoint, _transporter.InternalEndpoint),
                        password,
                        isPrivate,
                        _versionInfo.VersionNumber,
                        maxPlayers);
                    _masterServerClient.Client.RegisterHost(request, statusCode => registrationConfirmation.SetResult(statusCode));

                    while (!registrationConfirmation.IsResultAvailable) {
                        yield return WaitCommand.WaitForNextFrame;
                    }

                    var asyncPlayerInfo = new AsyncResult<Maybe<PlayerInfo>>();
                    _masterServerClient.Client.Me((statusCode, playerInfo) => {
                        if (statusCode == HttpStatusCode.OK) {
                            asyncPlayerInfo.SetResult(Maybe.Just(playerInfo));
                        } else {
                            Debug.LogWarning("Unable to retrieve server player info from Padrone");
                            asyncPlayerInfo.SetResult(Maybe<PlayerInfo>.Nothing);
                        }
                    });
                    
                    while (!asyncPlayerInfo.IsResultAvailable) {
                        yield return WaitCommand.WaitForNextFrame;
                    }

                    if (asyncPlayerInfo.Result.IsJust) {
                        _serverPlayerInfo = asyncPlayerInfo.Result.Value;
                    } else {
                        _serverPlayerInfo = new PlayerInfo("Unknown player", "", false, false);
                    }

                    if (registrationConfirmation.Result != HttpStatusCode.OK) {
                        Debug.LogWarning("Failed to register at the master server due to: " + registrationConfirmation.Result);
                    } else {
                        isRegistrationSuccessful = true;
                        Debug.Log("Successfully registered at master server");
                    }

                    yield return WaitCommand.WaitSeconds(3f);
                }

                while (isRegistrationSuccessful) {
                    yield return WaitCommand.WaitSeconds(30f);

                    var pingResult = new AsyncResult<HttpStatusCode>();
                    var endpoint = _runOnLocalHost
                        ? new IPEndPoint(IPAddress.Parse("127.0.0.1"), externalEndpoint.Port)
                        : externalEndpoint;

                    var sessions = _clientSessions.Values.ToListOptimized();
                    _masterServerClient.Client.Ping(endpoint, sessions, statusCode => pingResult.SetResult(statusCode));
                    while (!pingResult.IsResultAvailable) {
                        yield return WaitCommand.WaitForNextFrame;
                    }

                    isRegistrationSuccessful = pingResult.Result == HttpStatusCode.OK;
                }
            }
        }

        public NetworkSystems NetworkSystems {
            set { _networkSystems = value; }
        }

        public LidgrenNatFacilitatorConnection NatFacilitatorConnection {
            set { _natFacilitatorConnection = value; }
        }

        private void OnGUI() {
            if (_isHosting) {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(string.Format("Render time {0:0.000}", _gameClock.CurrentTime));
                GUILayout.Label(string.Format("Fixed time {0:0.000}", _fixedClock.CurrentTime));
                GUILayout.EndVertical();
            }
        }
    }
}
