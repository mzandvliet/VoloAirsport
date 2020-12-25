using System;
using System.Collections.Generic;

namespace RamjetAnvil.RamNet {
    public interface IConnectionGroup {
        event Action<ConnectionId> PeerJoined;
        event Action<ConnectionId> PeerLeft;
        IList<ConnectionId> ActiveConnections { get; } 
    }
}
