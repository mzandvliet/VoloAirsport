using UnityEngine;

/*
 * Todo:
 * - this should't be a component, but a datatype in Parachute class
 * - maintain internal pressure state? Analytic is nice because no state goes over network, but has limits
 * Try without pressure state first.
 */

namespace RamjetAnvil.Volo {
    
    public class Cell : MonoBehaviour {
        [SerializeField]
        private Rigidbody _body;
        [SerializeField]
        private ParachuteAirfoil _airfoil;
        [SerializeField]
        private ConfigurableJoint _joint;
        [SerializeField]
        private BoxCollider _collider;

        private Transform _transform;
        private JointDrive _linearDrive;
        private JointDrive _angularDrive;

        public Transform Transform {
            get { return _transform; }
        }

        public BoxCollider Collider {
            get {
                if (_collider == null) {
                    _collider = GetComponent<BoxCollider>();
                }
                return _collider;
            }
        }

        public ParachuteAirfoil Airfoil {
            get { return _airfoil; }
            set { _airfoil = value; }
        }

        public Rigidbody Body {
            get { return _body; }
            set { _body = value; }
        }

        public ConfigurableJoint Joint {
            get { return _joint; }
            set { _joint = value; }
        }

        private void Awake() {
            _transform = gameObject.GetComponent<Transform>();
            _collider = _collider ?? GetComponent<BoxCollider>();
            _linearDrive = new JointDrive {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = 0f
            };
            _angularDrive = new JointDrive {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = 0f
            };
        }

        public void SetKinematic() {
            _body.isKinematic = true;

            if (_joint != null) {
                _joint.xMotion = ConfigurableJointMotion.Free;
                _joint.yMotion = ConfigurableJointMotion.Free;
                _joint.zMotion = ConfigurableJointMotion.Free;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularYMotion = ConfigurableJointMotion.Free;
                _joint.angularZMotion = ConfigurableJointMotion.Free;
            }
        }

        public void SetPhysical() {
            _body.isKinematic = false;

            _airfoil.Clear();

            if (_joint != null) {
                _joint.xMotion = ConfigurableJointMotion.Limited;
                _joint.yMotion = ConfigurableJointMotion.Limited;
                _joint.zMotion = ConfigurableJointMotion.Limited;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularYMotion = ConfigurableJointMotion.Free;
                _joint.angularZMotion = ConfigurableJointMotion.Free;
            }
        }

        public void ResetVelocity() {
            _body.velocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
        }

        public void SetPressure(float pressure, float normalizedPressure) {
            if (!_joint) {
                return;
            }

            _airfoil.OptimalPressureFactor = normalizedPressure;

            _linearDrive.positionSpring = pressure * 1f;
            _linearDrive.positionDamper = pressure * 0.1f;
            _linearDrive.maximumForce = pressure * 1f;

            _angularDrive.positionSpring = pressure * 1.0f;
            _angularDrive.positionDamper = pressure * 0.1f;
            _angularDrive.maximumForce = pressure * 1f;

            _joint.xDrive = _linearDrive;
            _joint.yDrive = _linearDrive;
            _joint.zDrive = _linearDrive;

            _joint.angularXDrive = _angularDrive;
            _joint.angularYZDrive = _angularDrive;
        }
    }
}
