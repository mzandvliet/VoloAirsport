using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.RamNet {

    public class LocalLatencyInfo : ILatencyInfo {
        public static readonly ILatencyInfo Default = new LocalLatencyInfo();

        private LocalLatencyInfo() {}

        public float GetLatency(ConnectionId connectionId) {
            return 0f;
        }
    }
}
