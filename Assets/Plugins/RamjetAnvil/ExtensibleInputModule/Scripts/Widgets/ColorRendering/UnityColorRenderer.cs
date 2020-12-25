using UnityEngine;

namespace RamjetAnvil.InputModule {
    public abstract class UnityColorRenderer : MonoBehaviour, IColorRenderer {
        public abstract Color Color { get; set; }
    }
}
