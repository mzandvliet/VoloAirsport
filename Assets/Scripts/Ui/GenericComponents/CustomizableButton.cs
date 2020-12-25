using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class CustomizableButton : MonoBehaviour {
        [SerializeField] private Button _button;
        [SerializeField] private Text _buttonText;

        public UnityEvent OnSubmit {
            get { return _button.onClick; }
        }

        public string text {
            set { _buttonText.text = value; }
        }

        public Text ButtonText {
            get { return _buttonText; }
        }

        public void InvalidateTextCache() {
            _buttonText.cachedTextGenerator.Invalidate();
        }

        public Button Button {
            get { return _button; }
        }
    }
}
