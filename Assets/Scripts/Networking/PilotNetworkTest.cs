using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using UnityEngine;
using Guid = RamjetAnvil.Unity.Utility.Guid;

namespace RamjetAnvil.Volo.Networking {
    public class PilotNetworkTest : MonoBehaviour {

        [SerializeField] private int _serverPort = 22151;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private VoloNetworkServer _networkServer;
        [SerializeField] private VoloNetworkClient _networkClient;

        void Awake() {
            _cameraManager.CreateCameraRig(VrMode.None, new DependencyContainer());
        }

        void Start() {
            const bool isPrivate = false;
            const int maxPlayers = 2;
            _networkServer.Host("sillyHostName", maxPlayers, isPrivate, _serverPort);
            var localEndpoint = _networkServer.InternalEndpoint;

            _networkClient.Join(localEndpoint);
        }
    }
}
