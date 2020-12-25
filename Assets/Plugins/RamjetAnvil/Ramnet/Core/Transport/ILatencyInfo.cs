using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RamjetAnvil.RamNet {

    public interface ILatencyInfo {
        float GetLatency(ConnectionId connectionId);
    }
}
