using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    public abstract class AbstractRigidbody : PositionAndVelocity {
        public abstract ImmutableTransform Transform { get; }
    }
}
