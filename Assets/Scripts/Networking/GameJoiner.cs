using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RamjetAnvil.Volo.Networking {
    public class GameJoiner {

        public enum JoinStatus { Success, Failure }

        private readonly VoloNetworkClient _client;

        public GameJoiner(VoloNetworkClient client) {
            _client = client;
        }

        public void Join(IPEndPoint endPoint, Action<JoinStatus> onDone) {
            //_client.Join(endPoint, );
        }

        public void CancelJoin() {
            _client.Leave();
        }
    }
}
