using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    public class WrappedRigidbody : AbstractRigidbody {

        [SerializeField] private Rigidbody _rigidbody;

        public override Vector3 Velocity {
            get { return _rigidbody.velocity; }
        }

        public override Vector3 Position {
            get { return _rigidbody.position; }
        }

        public override ImmutableTransform Transform {
            get { return _rigidbody.ImmutableTransform(); }
        }
    }
}
