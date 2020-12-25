using UnityEngine;

namespace RamjetAnvil.InputModule {

    public class ColorHighlight : MonoBehaviour {
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private UnityColorRenderer _renderer;
        [SerializeField] private UnityHighlightable _highlightable;

        private Color _defaultColor;
        private bool _isHightlighted;

        void Awake() {
            if (_renderer != null) {
                Renderer = _renderer;
            }

            if (_highlightable != null) {
                Highlightable = _highlightable;
            }
        }

        private void Highlight() {
            _isHightlighted = true;
            if (_renderer != null) {
                _renderer.Color = _highlightColor;
            }
        }

        private void UnHighlight() {
            _isHightlighted = false;
            if (_renderer != null) {
                _renderer.Color = _defaultColor;
            }
        }

        public UnityHighlightable Highlightable {
            set {
                if (_highlightable != null) {
                    _highlightable.OnHighlight -= Highlight;
                    _highlightable.OnUnHighlight -= UnHighlight;
                }

                _highlightable = value;
                _highlightable.OnHighlight += Highlight;
                _highlightable.OnUnHighlight += UnHighlight;
            }
        }

        public UnityColorRenderer Renderer {
            set {
                _defaultColor = _renderer.Color;
                _renderer = value;

                if (_isHightlighted) {
                    Highlight();
                } else {
                    UnHighlight();
                }
            }
        }
    }
}
