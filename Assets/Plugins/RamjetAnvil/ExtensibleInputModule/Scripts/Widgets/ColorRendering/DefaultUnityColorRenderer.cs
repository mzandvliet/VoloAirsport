using UnityEngine;

namespace RamjetAnvil.InputModule {
    public class DefaultUnityColorRenderer : UnityColorRenderer {
        [SerializeField] private Renderer[] _renderers;

        public override Color Color {
            get { return _renderers[0].material.color; }
            set {
                for (int i = 0; i < _renderers.Length; i++) {
                    _renderers[i].material.color = value;
                }
            }
        }
    }
}
