using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class EnumList : Selectable {

        public event Action OnNext;
        public event Action OnPrev;

        [SerializeField] private Button _prev;
        [SerializeField] private Button _next;
        [SerializeField] private Text _text;

        private bool _isOnNextEnabled;
        private bool _isOnPrevEnabled;
        private bool _isEnabled;

        protected override void Awake() {
            base.Awake();
            _isOnNextEnabled = true;
            _isOnPrevEnabled = true;
            _isEnabled = true;

            _prev.onClick.AddListener(SelectPrevious);
            _next.onClick.AddListener(SelectNext);
        }

        public void SetEnabled(bool isEnabled) {
            _isEnabled = isEnabled;
            UpdateState();
        }

        public void SetText(string text) {
            _text.text = text;
        }

        public void EnableOnNext() {
            _isOnNextEnabled = true;
            UpdateState();
        }

        public void DisableOnNext() {
            _isOnNextEnabled = false;
            UpdateState();
        }

        public void EnableOnPrev() {
            _isOnPrevEnabled = true;
            UpdateState();
        }

        public void DisableOnPrev() {
            _isOnPrevEnabled = false;
            UpdateState();
        }

        protected void UpdateState() {
            _next.interactable = _isOnNextEnabled && _isEnabled;
            _prev.interactable = _isOnPrevEnabled && _isEnabled;
        }

        public override void OnMove(AxisEventData eventData) {
            switch (eventData.moveDir) {
                case MoveDirection.Left:
                  SelectPrevious();
                  break;
                case MoveDirection.Right:
                  SelectNext();
                  break;
                default:
                    base.OnMove(eventData);
                    break;
            }
        }

        protected void SelectPrevious() {
            if (OnPrev != null && _isEnabled) {
                OnPrev();
            }
        }

        protected void SelectNext() {
            if (OnNext != null && _isEnabled) {
                OnNext();
            }
        }
    }
}
