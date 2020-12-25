using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Networking;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Padrone.Client;
using UnityEngine;

namespace RamjetAnvil.Volo.Networking {

    public class TempMultiplayerGui : MonoBehaviour {

        [SerializeField] private UnityMasterServerClient _masterServerClient;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private VoloNetworkClient _networkClient;
        [SerializeField] private VoloNetworkServer _networkServer;
        [SerializeField] private VersionInfo _versionInfo;

        private bool _isHostListRefreshing;
        private IList<RemoteHost> _availableHosts;
        private GameState _state = GameState.Disconnected;
//        private AuthToken _authToken;
        private string _hostName = "SomeHost";
        private string _port = "22151";
        private Vector2 _hostListScrollPosition;

        private string _hostEndpoint;

        void Awake() {
            Application.runInBackground = true;

            _cameraManager.CreateCameraRig(VrMode.None, new DependencyContainer());
            _hostEndpoint = "127.0.0.1:22151";
            _availableHosts = new List<RemoteHost>();
//            _authToken = new AuthToken("dummy", "dummy");
        }

        private void OnGUI() {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Status: " + _state);
                switch (_state) {
                    case GameState.Disconnected:
    //                    _endpoint = GUILayout.TextField(_endpoint);
    //                    if (GUILayout.Button("Connect")) {
    //                        StartClient();
    //                    }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Host");
                        _hostEndpoint = GUILayout.TextField(_hostEndpoint, GUILayout.Width(150));
                        if (GUILayout.Button("Connect")) {
                            StartClient(Ipv4Endpoint.Parse(_hostEndpoint).ToIpEndPoint());
                        }
                        GUILayout.EndHorizontal();

                        GUI.color = Color.white;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Host", GUILayout.Width(200));
                        GUILayout.Label("Endpoint", GUILayout.Width(150));
                        GUILayout.Label("Distance", GUILayout.Width(100));
                        GUILayout.Label("Country", GUILayout.Width(100));
                        GUILayout.Label("Version", GUILayout.Width(50));
                        GUILayout.EndHorizontal();

                        _hostListScrollPosition = GUILayout.BeginScrollView(
                            _hostListScrollPosition,
                            GUILayout.MaxHeight(400));
                        if (_availableHosts != null) {
                            for (int i = 0; i < _availableHosts.Count || i >= 40; i++) {
                                var hostInfo = _availableHosts[i];
                                GUI.color = Color.white;
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(hostInfo.Name, GUILayout.Width(200));
                                GUILayout.Label(hostInfo.PeerInfo.External.ToString(), GUILayout.Width(150));
                                GUILayout.Label(Mathf.RoundToInt((float) hostInfo.DistanceInKm) + " km", GUILayout.Width(100));
                                GUILayout.Label(hostInfo.Country, GUILayout.Width(100));
                                GUILayout.Label("v" + hostInfo.Version, GUILayout.Width(50));
                                GUI.color = Color.white;
                                if (GUILayout.Button("Connect")) {
                                    StartClient(hostInfo.PeerInfo.External);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        GUILayout.EndScrollView();

                        GUI.color = Color.white;
                        GUI.enabled = !_isHostListRefreshing;
                        if (GUILayout.Button("Refresh host list")) {
                            _isHostListRefreshing = true;
                            const bool hideFull = false;
                            const int limit = 100;
                            _masterServerClient.Client.ListHosts(
                                hideFull,
                                limit,
                                (statusCode, hosts) => {
                                if (statusCode == HttpStatusCode.OK) {
                                    _availableHosts.Clear();
                                    for (int i = 0; i < hosts.Count; i++) {
                                        var host = hosts[i];
                                        _availableHosts.Add(host);
                                    }
                                }
                                _isHostListRefreshing = false;
                            });
                        }
                        GUI.enabled = true;

                        GUILayout.BeginHorizontal();
                        _hostName = GUILayout.TextField(_hostName, maxLength: 100);
                        GUILayout.Label("Port: ");
                        _port = GUILayout.TextField(_port, maxLength: 20);
                        if (GUILayout.Button("Host")) {
                            var port = Convert.ToInt32(_port);
                            StartListeningServer(_hostName, port);
                        }
                        GUILayout.EndHorizontal();
    //                    if (GUILayout.Button("Host Listening Server")) {
    //                        StartListeningServer();
    //                    }
                        break;
                    case GameState.Connecting:
                        // Todo: cancel
                        break;
                    case GameState.Client:
                        //GUILayout.Label("Latency: " + Mathd.ToMillis(_client.RoundtripTime * 0.5));
                        if (GUILayout.Button("Disconnect")) {
                            StopClient();
                        }
//                        GUILayout.Label("Game Clock: " + _synchedGameClock.CurrentTime);
//                        GUILayout.Label("Fixed Clock: " + _syncedFixedClock.CurrentTime);

                    
//                        var packetsReceived = _client.UpdatePlayerPacketsReceived;
//                        if (packetsReceived != null) {
//                            for (int i = packetsReceived.Count - 1; i >= 0; i--) {
//                                var packetTimestamp = packetsReceived[i];
//                                if (packetTimestamp < _localRealtimeClock.CurrentTime - 10f) {
//                                    break;
//                                } 
//
//                                var graphSize = new Vector2(100, 50);
//                                var graphPosition = new Vector2(Screen.width - graphSize.x, Screen.height - graphSize.y);
//                                GUI.color = Color.black;
//                                _graphStyle.fontSize = 30;
//                                var distanceFromOrigin = (int)Math.Round((_localRealtimeClock.CurrentTime - packetTimestamp) * 50.0);
//                                GUI.Label(
//                                    new Rect(new Vector2((Screen.width - 50) - distanceFromOrigin, graphPosition.y), graphSize), 
//                                    "|",
//                                    _graphStyle);
//                            }
//                        }

                        break;
                    case GameState.ListeningServer:
                        if (GUILayout.Button("Disconnect")) {
                            StopListeningServer();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            GUILayout.EndVertical();

            GUI.color = Color.black;
            GUI.Label(new Rect(Screen.width - 40, 10, 40, 50), "v" + _versionInfo.VersionNumber);
        }

        private void StartClient(IPEndPoint remoteEndpoint) {
            _state = GameState.Connecting;
            _networkClient.enabled = true;
            var port = -1;
            _networkClient.Join(remoteEndpoint, port, OnClientConnected, OnClientDisconnected, OnClientDisconnected);
            _networkServer.enabled = false;
        }

        private void StartListeningServer(string hostName, int port) {
            _state = GameState.ListeningServer;
            var isPrivate = false;
            var maxPlayers = 4;
            _networkServer.enabled = true;
            _networkServer.Host(hostName, maxPlayers, isPrivate, port);
            _networkClient.enabled = false;
        }

        private void StopClient() {
            _networkClient.Leave();
        }

        private void StopListeningServer() {
            _networkServer.Stop();
            _state = GameState.Disconnected;
        }

        private void OnClientConnected(ConnectionId connectionId, IPEndPoint endpoint) {
            _state = GameState.Client;                   
        }

        private void OnClientDisconnected(ConnectionId connectionId) {
            _state = GameState.Disconnected;
        }

        private void OnClientDisconnected(IPEndPoint endpoint, Exception e) {
            _state = GameState.Disconnected;
        }
        
        private enum GameState {
            Disconnected,
            Connecting,
            Client,
            ListeningServer,
        }
    }

}
