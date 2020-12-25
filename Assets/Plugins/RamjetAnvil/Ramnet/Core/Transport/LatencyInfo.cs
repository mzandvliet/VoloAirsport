using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.RamNet {
    public class LatencyInfo : ILatencyInfo {

        private readonly IDictionary<ConnectionId, float> _latencyPerConnection;

        public LatencyInfo(int maxConnections) {
            _latencyPerConnection = new ArrayDictionary<ConnectionId, float>(maxConnections);
        }

        public float GetLatency(ConnectionId connectionId) {
            if (connectionId.Value < 0) {
                return 0f;
            }
            return _latencyPerConnection.GetOrDefault(connectionId, 0f);
        }

        public void UpdateLatency(ConnectionId connectionId, float newLatency) {
            _latencyPerConnection[connectionId] = newLatency;
        }
    }
}
