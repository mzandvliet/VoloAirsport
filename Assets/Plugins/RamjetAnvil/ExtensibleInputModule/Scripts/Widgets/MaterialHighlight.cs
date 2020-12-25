using UnityEngine;

namespace RamjetAnvil.InputModule {

    public class MaterialHighlight : MonoBehaviour {
        [SerializeField] private Material _highlightMaterial;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private UnityHighlightable _highlightable;

        private bool _isHightlighted;
        private Material _defaultMaterial;

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
                _renderer.material = _highlightMaterial;    
            }
        }

        private void UnHighlight() {
            _isHightlighted = false;
            if (_renderer != null) {
                _renderer.material = _defaultMaterial;    
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
                _defaultMaterial = _renderer.material;

                if (_isHightlighted) {
                    Highlight();
                }
            }
        }
    }
}
