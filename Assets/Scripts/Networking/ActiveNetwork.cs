using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.RamNet;
using RamjetAnvil.Padrone.Client;

namespace RamjetAnvil.Volo.Networking {
    public class ActiveNetwork {
        private ConnectionId _authorityConnectionId;
        public readonly NetworkSystems NetworkSystems;
        private readonly ICoroutineScheduler _coroutineScheduler;
        private readonly VoloNetworkServer _networkServer;
        private readonly VoloNetworkClient _networkClient;

        private bool _isSingleplayer;
        private bool _isJoinInProgress;

        public ActiveNetwork(NetworkSystems networkSystems, VoloNetworkServer networkServer, VoloNetworkClient networkClient, 
            ICoroutineScheduler coroutineScheduler) {

            NetworkSystems = networkSystems;
            _networkServer = networkServer;
            _networkClient = networkClient;
            _coroutineScheduler = coroutineScheduler;
            _authorityConnectionId = ConnectionId.NoConnection;
            _isSingleplayer = true;
        }

        public void StartSingleplayerServer() {
            Shutdown();
            _networkServer.HostAsSingleplayer();
            _authorityConnectionId = ConnectionId.Self;
            _isSingleplayer = true;
        }

        public void StartServer(string hostName, int maxPlayers, bool isPrivate = false, int port = -1) {
            Shutdown();
            _networkServer.Host(hostName, maxPlayers, isPrivate, port);
            _authorityConnectionId = ConnectionId.Self;
            _isSingleplayer = false;
        }

        public void JoinServer(IPEndPoint endPoint, 
            int clientPort = -1,
            OnConnectionEstablished onEstablished = null, 
            OnConnectionFailure onFailure = null, OnDisconnected onDisconnected = null) {

            Shutdown();
            _isJoinInProgress = true;
            _isSingleplayer = false;
            _coroutineScheduler.Run(_networkClient.Join(endPoint,
                clientPort,
                (connectionId, endpoint) => {
                    _authorityConnectionId = connectionId;
                    _isJoinInProgress = false;
                    if (onEstablished != null) {
                        onEstablished(connectionId, endPoint);
                    }
                },
                onFailure,
                onDisconnected));
        }

        public void CancelJoin() {
            _coroutineScheduler.Run(CancelJoinInternal());
        }

        private IEnumerator<WaitCommand> CancelJoinInternal() {
            yield return _networkClient.Leave().AsWaitCommand();
            _isJoinInProgress = false;
        } 

        public bool IsJoinInProgress {
            get { return _isJoinInProgress; }
        }

        public bool IsSingleplayer {
            get { return _isSingleplayer; }
        }

        public ConnectionId AuthorityConnectionId {
            get { return _authorityConnectionId; }
        }

        public void Shutdown() {
            if (!_isJoinInProgress) {
                _networkServer.Stop();
                _networkClient.Leave();
                _authorityConnectionId = ConnectionId.NoConnection;
                _isSingleplayer = true;
            } else {
                throw new Exception("Join is already in progress");
            }
        }

        public void SendToAuthority(INetworkMessage message) {
            NetworkSystems.MessageSender.Send(_authorityConnectionId, message);
        }
    }
}
