using System;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class EditableText : MonoBehaviour {
        [SerializeField] private InputField _inputField;

        private string _currentText;
        public event Action<string> TextChanged;

        void Awake() {
            IsEditable = false;

            _inputField.onValueChanged.AddListener(newText => {
                if (TextChanged != null && _currentText != newText) {
                    _currentText = newText;
                    TextChanged(newText);
                }
            });
        }

        public bool IsEditable {
            set {
                _inputField.gameObject.SetActive(value);    
            }
        }

        public string Text {
            set {
                _currentText = value;
                if (_inputField.text != value) {
                    _inputField.text = value;    
                }
            }
        }

        public int Limit {
            set { _inputField.characterLimit = value; }
        }
    }
}
