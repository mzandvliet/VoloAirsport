using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    public abstract class PositionAndVelocity : MonoBehaviour {
        public abstract Vector3 Velocity { get; }
        public abstract Vector3 Position { get; }
    }
}
