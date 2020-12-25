using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo {
    public class ParachutePropertyUi : MonoBehaviour {
        [SerializeField] private Text _textRenderer;

        private MutableString _text;

        void Awake() {
            if (_text == null) {
                _text = new MutableString(10);
            }
        }

        void LateUpdate() {
            _textRenderer.SetMutableString(_text);
        }

        public MutableString Text {
            get {
                Awake();
                return _text;
            }
        }
    }
}
