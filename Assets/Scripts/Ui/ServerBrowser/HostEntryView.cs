using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class HostEntryView : MonoBehaviour {

        public event Action OnCancelClicked;
        public event Action OnJoinClicked;

        [SerializeField] private Text _name;
        [SerializeField] private Text _hostedBy;
        [SerializeField] private Text _players;
        [SerializeField] private Text _distance;
        [SerializeField] private CustomizableButton _join;

        private bool _isBeingJoined;

        void Awake() {
            _join.OnSubmit.AddListener(() => {
                if (_isBeingJoined) {
                    if (OnCancelClicked != null) {
                        OnCancelClicked();
                    }
                } else {
                    if (OnJoinClicked != null) {
                        OnJoinClicked();
                    }
                }
            });
        }

        public void SetState(HostEntry hostEntry, bool isJoinAllowed) {
            _isBeingJoined = hostEntry.IsBeingJoined;

            _name.text = hostEntry.Host.Name;
            // TODO Use mutable strings
            _hostedBy.supportRichText = hostEntry.Host.HostedBy == null;
            _hostedBy.text = hostEntry.Host.HostedBy ?? "<i>Anonymous player</i>";
            _players.text = hostEntry.Host.PlayerCount + "/" + hostEntry.Host.MaxPlayers;
            _distance.text = hostEntry.Host.DistanceInKm + "km";
            if (_isBeingJoined) {
                _join.text = "joining... (press to cancel)";
                _join.Button.interactable = true;
            } else {
                _join.text = "Join";
                _join.Button.interactable = isJoinAllowed;
            }
        }

        public CustomizableButton JoinButton
        {
            get { return _join; }
        }
    }
}
