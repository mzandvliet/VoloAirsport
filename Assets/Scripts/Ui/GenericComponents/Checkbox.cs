using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class Checkbox : MonoBehaviour {

        public event Action OnValueChanged;

        [SerializeField] private CustomizableButton _checkBox;

        private bool _isEnabled;

        void Awake() {
            _isEnabled = true;
            _checkBox.OnSubmit.AddListener(() => {
                if (OnValueChanged != null && _isEnabled) {
                    OnValueChanged();
                }
            });
        }

        public void SetEnabled(bool isEnabled) {
            _isEnabled = isEnabled;
        }

        public void SetChecked(bool isChecked) {
            if (isChecked) {
                Check();
            } else {
                Uncheck();
            }
        }

        public void Check() {
            _checkBox.text = "✓";    
        }

        public void Uncheck() {
            _checkBox.text = " ";    
        }

        public Selectable NavigationElement {
            get { return _checkBox.Button; }
        }
    }
}
