using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo.Networking {
    public class NetworkConfig : MonoBehaviour {
        public const int MaxConnections = 16;
        public const string GameId = "Volo Airsport";

        [SerializeField] private bool _isServer;
        [SerializeField] private int _serverPort;

        public bool IsServer {
            get { return _isServer; }
        }

        public int ServerPort {
            get { return _serverPort; }
        }
    }
}
