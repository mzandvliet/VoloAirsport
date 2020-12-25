using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class ServerBrowserView : MonoBehaviour {
        [SerializeField] private int _maxHostEntries = 100;
        [SerializeField] private GameObject _hostEntryViewPrefab;
        [SerializeField] private GameObject _hostList;
        [SerializeField] private Text _masterServerStatus;
        [SerializeField] private CustomizableButton _refreshButton;
        [SerializeField] private Checkbox _hideFull;

        [SerializeField, Dependency] private ServerBrowserViewModel _viewModel;

        private MutableString _masterServerStatusStr;
        private IList<HostEntryView> _hostEntryViews; 

        void Awake() {
            _masterServerStatusStr = new MutableString(100);
            _hostEntryViews = new List<HostEntryView>(_maxHostEntries);
            for (int i = 0; i < _maxHostEntries; i++) {
                var entryIndex = i;
                var hostEntryView = Instantiate(_hostEntryViewPrefab).GetComponent<HostEntryView>();
                hostEntryView.OnJoinClicked += () => _viewModel.Join(entryIndex);
                hostEntryView.OnCancelClicked += () => _viewModel.CancelJoin(entryIndex);

                hostEntryView.gameObject.SetParent(_hostList);
                hostEntryView.gameObject.SetActive(false);
                _hostEntryViews.Add(hostEntryView);
            }
            _hideFull.OnValueChanged += () => _viewModel.ChangeHideFull(!_viewModel.State.HideFull);
            _refreshButton.OnSubmit.AddListener(() => _viewModel.Refresh());
            _viewModel.StateUpdated += SetState;
        }

        void OnEnable() {
        }

        // Is responsible for rendering the available servers given
        // Limits the output to n servers
        // Displays state such as: currently joining game
        // Allows players to cancel the join
        private void SetState(ServerBrowserViewState state) {
            for (int i = 0; i < _hostEntryViews.Count; i++) {
                _hostEntryViews[i].gameObject.SetActive(false);
            }

            var isJoinAllowed = !state.IsBusy;
            Debug.Log("host count " + state.Hosts.Count);
            for (int i = 0; i < state.Hosts.Count; i++) {
                var hostEntry = state.Hosts[i];
                var hostEntryView = _hostEntryViews[i];
                hostEntryView.gameObject.SetActive(true);
                hostEntryView.SetState(hostEntry, isJoinAllowed);

                var joinButton = hostEntryView.JoinButton.Button;

                Selectable prev;
                var hasPreviousHost = i > 0;
                if (hasPreviousHost) {
                    prev = _hostEntryViews[i - 1].JoinButton.Button;
                } else {
                    prev = _refreshButton.Button;
                }

                Selectable next;
                var hasNextHost = i + 1 < state.Hosts.Count;
                if (hasNextHost) {
                    next = _hostEntryViews[i + 1].JoinButton.Button;
                } else {
                    next = _refreshButton.Button;
                }

                joinButton.navigation = new Navigation {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = prev,
                    selectOnDown = next,
                };
            }

            Debug.Log("MS status "  + state.MasterServerStatus);
            _masterServerStatusStr.Clear()
                .Append("Master server status: <b>")
                .Append(state.MasterServerStatus == MasterServerStatus.Online ? "Online" : "Offline")
                .Append("</b>");

            // TODO Factor this into a single re-usable method
            _masterServerStatus.SetMutableString(_masterServerStatusStr);

            var firstSelectableJoinButton = state.Hosts.Count > 0 ? _hostEntryViews[0].JoinButton.Button : null;
            var lastSelectableJoinButton = state.Hosts.Count > 0 ? _hostEntryViews[state.Hosts.Count - 1].JoinButton.Button : null;

            _hideFull.SetEnabled(isJoinAllowed);
            _hideFull.SetChecked(state.HideFull);
            _hideFull.NavigationElement.navigation = new Navigation {
                mode = Navigation.Mode.Explicit,
                selectOnUp = lastSelectableJoinButton ?? _refreshButton.Button,
                selectOnDown = _refreshButton.Button,
            };

            _refreshButton.Button.interactable = !state.IsBusy;
            _refreshButton.Button.navigation = new Navigation {
                mode = Navigation.Mode.Explicit,
                selectOnUp = _hideFull.NavigationElement,
                selectOnDown = firstSelectableJoinButton ?? _hideFull.NavigationElement,
            };
        }
    }
}
