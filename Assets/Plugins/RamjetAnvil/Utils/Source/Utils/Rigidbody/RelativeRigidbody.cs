using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {

    public class RelativeRigidbody : AbstractRigidbody {
        [SerializeField] private Rigidbody _rigidbody;

        private Transform _transform;

        void Awake() {
            _transform = GetComponent<Transform>();
        }

        public override Vector3 Velocity {
            get { return _rigidbody.GetPointVelocity(_transform.position); }
        }

        public override ImmutableTransform Transform {
            get { return _transform.MakeImmutable(); }
        }

        public override Vector3 Position {
            get { return _transform.position; }
        }
    }
}
