using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    // Todo: minimal - a plain Image + slider-driven RGB color swatch, not a full picker UI.
    // Was a stripped commercial/custom asset with no source left in this repo; this is a
    // small self-contained replacement, not a stub (unlike the input rebinding surface).
    [RequireComponent(typeof(Image))]
    public class ColorPicker : MonoBehaviour {
        public class ColorChangedEvent : UnityEvent<Color> { }

        public ColorChangedEvent onValueChanged = new ColorChangedEvent();

        [SerializeField] private Color _currentColor = Color.white;

        private Image _swatch;

        private void Awake() {
            _swatch = GetComponent<Image>();
            _swatch.color = _currentColor;
        }

        public Color CurrentColor {
            get { return _currentColor; }
            set {
                _currentColor = value;
                if (_swatch == null) {
                    _swatch = GetComponent<Image>();
                }
                _swatch.color = value;
                onValueChanged.Invoke(value);
            }
        }
    }
}
