using System;
using System.Collections.Generic;
using System.Net;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Padrone.Client;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public class ServerBrowserViewModel : MonoBehaviour {
        public event Action<HostEntry> RequestJoin;
        public event Action RequestCancelJoin;

        public event Action<ServerBrowserViewState> StateUpdated;

        [SerializeField, Dependency] private UnityMasterServerClient _masterServerClient;

        private ServerBrowserViewState _viewState;

        void Awake() {
            _viewState = new ServerBrowserViewState();
        }

        public void ChangeHideFull(bool hideFull) {
            if (_viewState.HideFull != hideFull) {
                _viewState.HideFull = hideFull;
                Render();
                Refresh();
            }
        }

        public void Join(int hostEntryIndex) {
            var hostEntry = _viewState.Hosts[hostEntryIndex];
            _viewState.JoiningHost = hostEntry;
            _viewState.IsBusy = true;
            hostEntry.IsBeingJoined = true;
            Render();

            if (RequestJoin != null) {
                RequestJoin(hostEntry);
            }
        }

        public void JoinSucceeded() {
            _viewState.IsBusy = false;
            var hostEntry = _viewState.JoiningHost;
            hostEntry.IsBeingJoined = false;
            Render();
        }

        public void JoinFailed() {
            JoinSucceeded();
        }

        public void CancelJoin(int hostEntryIndex) {
            if (RequestCancelJoin != null) {
                RequestCancelJoin();
            }

            var hostEntry = _viewState.Hosts[hostEntryIndex];
            hostEntry.IsBeingJoined = false;
            _viewState.IsBusy = false;
            Render();
        }

        public void Refresh() {
            if (!_viewState.IsBusy) {
                _viewState.IsBusy = true;
                Render();

                const int limit = 50;
                _masterServerClient.Client.ListHosts(_viewState.HideFull, limit, UpdateHostList);
            }
        }

        private void UpdateHostList(HttpStatusCode statusCode, IList<RemoteHost> hosts) {
            _viewState.IsBusy = false;

            

            if (statusCode == HttpStatusCode.OK) {
                //                foreach (var remoteHost in hosts) {
                //                    Debug.Log("Remote host " + remoteHost.Name);
                //                }

                Debug.Log("hosts are fetched " + hosts.Count);

                _viewState.Hosts.Clear();
                for (int i = 0; i < hosts.Count; i++) {
                    var host = hosts[i];
                    _viewState.Hosts.Add(new HostEntry {
                        Host = host,
                        IsBeingJoined = false
                    });
                }
                Render();
            } else {
                Debug.Log("failed to fetch remote hosts");
                // TODO Handle refresh error
            }
        }

        public void CheckMasterServerHealth() {
            _viewState.MasterServerStatus = MasterServerStatus.Unknown;
            _masterServerClient.Client.HealthCheck(UpdateMasterServerHealth);
            Render();
        }

        public ServerBrowserViewState State {
            get { return _viewState; }
        }

        private void UpdateMasterServerHealth(HttpStatusCode statusCode) {
            _viewState.MasterServerStatus = statusCode == HttpStatusCode.OK
                ? MasterServerStatus.Online
                : MasterServerStatus.Offline;
            Render();
        }

        private void Render() {
            if (StateUpdated != null) {
                StateUpdated(_viewState);
            }
        }
    }
}
