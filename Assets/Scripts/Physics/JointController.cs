using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;

// Todo: fixedupdate ordering between character controller and this class

/// <summary>
/// A joint modifier that makes it act more like muscles.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class JointController : MonoBehaviour, ISpawnable {
    [SerializeField]
    private float _dampeningFactor = 0f;

    private ConfigurableJoint _joint;
    private Rigidbody _jointBody;
    private Renderer _renderer;

    private float _maxXForce;
    private JointDrive _xDrive;
    private float _maxYZForce;
    private JointDrive _yZDrive;

    private Vector3 _targetPosition = Vector3.zero;
    private Quaternion _targetRotation = Quaternion.identity;

    private float _tension = 1f;
    private float _jointHealth;

    private Color _baseColor;
    private static readonly Color HurtColor = new Color(0.8f, 0.1f, 0.25f);

    public Vector3 TargetPosition {
        get { return _targetPosition; }
        set { _targetPosition = value; }
    }

    public Quaternion TargetRotation {
        get { return _targetRotation; }
        set { _targetRotation = value; }
    }

    public float Tension {
        get { return _tension; }
        set { _tension = value; }
    }

    private void Awake() {
        _joint = gameObject.GetComponent<ConfigurableJoint>();
        _jointBody = gameObject.GetComponent<Rigidbody>();
        _renderer = gameObject.GetComponent<Renderer>();

        _xDrive = _joint.angularXDrive;
        _maxXForce = _xDrive.positionSpring;
        _yZDrive = _joint.angularYZDrive;
        _maxYZForce = _yZDrive.positionSpring;

        _baseColor = _renderer.material.color;

        GetComponent<CollisionEventSource>().OnCollisionEntered += (source, collision) => {
            if (collision.gameObject.CompareTag("Parachute")) {
                return;
            }

            const float minimumDamageImpact = 0.25f;
            var collisionForce = Mathf.Pow(Vector3.Project(collision.relativeVelocity, collision.contacts[0].normal).magnitude / 25f, 1.5f);
            collisionForce = collisionForce > minimumDamageImpact ? collisionForce : 0f;

            _jointHealth -= collisionForce;
            _jointHealth = Mathf.Clamp(_jointHealth, 0.05f, 1f);
            _renderer.material.color = Color.Lerp(HurtColor, _baseColor, _jointHealth);
        };
    }

    public void OnSpawn() {
        _jointHealth = 1f;
        _renderer.material.color = _baseColor;
    }

    public void OnDespawn() {}

    private void FixedUpdate() {
        _jointBody.WakeUp();

        _joint.targetPosition = _targetPosition;
        _joint.targetRotation = _targetRotation;
        _joint.targetAngularVelocity = Vector3.zero;

        float dampening = _jointHealth * _dampeningFactor;
        float maxXForce = _maxXForce * _tension * _jointHealth;
        float maxXyForce = _maxYZForce * _tension * _jointHealth;

        _xDrive.positionSpring = maxXForce;
        _xDrive.positionDamper = dampening;
        _joint.angularXDrive = _xDrive;

        _yZDrive.positionSpring = maxXyForce;
        _yZDrive.positionDamper = dampening;
        _joint.angularYZDrive = _yZDrive;
    }
}