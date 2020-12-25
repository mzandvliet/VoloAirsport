using System.Collections.Generic;
using System.Net;
using RamjetAnvil.Padrone.Client;

namespace RamjetAnvil.Volo.Ui {
    public class HostEntry {
        public RemoteHost Host;
        public bool IsBeingJoined;
    }

    public enum MasterServerStatus { Unknown, Offline, Online }

    public class ServerBrowserViewState {
        public readonly IList<HostEntry> Hosts;

        public ServerBrowserViewState() {
            Hosts = new List<HostEntry>();
            IsBusy = false;
            HideFull = true;
        }

        public HostEntry JoiningHost { get; set; }
        public bool IsBusy { get; set; }
        public bool HideFull { get; set; }
        public MasterServerStatus MasterServerStatus { get; set; }
    }
}
