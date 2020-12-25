using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.RamNet;

namespace RamjetAnvil.Volo.Networking {
    public static class ConnectionGroups {
        public static readonly TransportGroupId Default = new TransportGroupId(0);
        public static readonly TransportRouterConfig RouterConfig =
            new TransportRouterConfig(
                new Dictionary<TransportGroupId, TransportGroupConfig> {
                    {Default, new TransportGroupConfig(16)}
                },
                Default);
    }
}
