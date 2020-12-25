using UnityEngine;

namespace RamjetAnvil.InputModule {

    public class TransparencyHighlight : MonoBehaviour {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private UnityHighlightable _highlightable;
        [SerializeField] private float unHighlightTransparency = 0.5f;

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
                var color = _renderer.material.color;
                color.a = 1f;
                _renderer.material.color = color;
            }
        }

        private void UnHighlight() {
            _isHightlighted = false;
            if (_renderer != null) {
                var color = _renderer.material.color;
                color.a = unHighlightTransparency;
                _renderer.material.color = color;
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

        public Renderer Renderer {
            set {
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
