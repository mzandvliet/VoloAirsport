using RamjetAnvil.Unity.Utility;
using UnityEngine;

/*
 * Todo:
 * - Express compression as a suspension length.
 * - Camber, because currently this assumes the wheel is always perfectly aligned to ground surface
 * 
 * Bugs:
 * - Moving down a slope introduces stuttering, why?
 */
public class Suspension : MonoBehaviour
{
    [SerializeField] Rigidbody _body;
    [SerializeField] float _springLength = 1f;
    [SerializeField] float _springRate = 1f;
    [SerializeField] float _dampeningFactor = 1f;
    //[SerializeField] private float _brakeFactor = 1f;
    //[SerializeField] private AnimationCurve _slipCurve;

    const string VehicleLayerName = "Vehicle";
    int _vehicleLayerMask;
    bool _initialized;
    bool _isGrounded;
    RaycastHit _hitInfo;
    Vector3 _springForce;
    Vector3 _dampeningForce;
    Vector3 _pointALastPosition;
    Vector3 _pointBLastPosition;
    //private float _brakeInput;

    public float SpringLength
    {
        get { return _springLength; }
        set { _springLength = value; }
    }

    public float SpringRate
    {
        get { return _springRate; }
        set { _springRate = value; }
    }

    public bool IsGrounded
    {
        get { return _isGrounded; }
    }

    public float DampeningFactor
    {
        get { return _dampeningFactor; }
        set { _dampeningFactor = value; }
    }

    public RaycastHit HitInfo
    {
        get { return _hitInfo; }
    }

    public void Brake(float brakeInput)
    {
        //_brakeInput = brakeInput;
    }

    void Awake()
    {
        Initialize();
        _vehicleLayerMask = ~(1 << LayerMask.NameToLayer(VehicleLayerName));

        _pointALastPosition = transform.position;
        _pointBLastPosition = transform.position + -transform.up * _springLength;
    }

    void Initialize()
    {
        // If no body was manually assigned, see if the parents have one
        if (!_body)
        {
            _body = PhysicsUtils.GetRigidBodyInParents(transform);
	        if (!_body)
	        {
	            _initialized = false;
	            Debug.LogError(string.Format("Suspension '{0}' failed to initialize, no rigidbody assigned or found in hierarchy", name));
	        }
		}

        if (_body)
            _initialized = true;
    }

    void FixedUpdate()
    {
        if (!_initialized)
            return;

        _springForce = Vector3.zero;
        _dampeningForce = Vector3.zero;

        Vector3 pointAPosition = transform.position;
        Vector3 pointBPosition = transform.position + -transform.up * _springLength;

        _isGrounded = Physics.Raycast(pointAPosition, -transform.up, out _hitInfo, _springLength, _vehicleLayerMask);
        if (_isGrounded)
        {
            pointBPosition = _hitInfo.point;

            Vector3 pointAVelocity = (pointAPosition - _pointALastPosition) / Time.fixedDeltaTime;
            _pointALastPosition = pointAPosition;

            Vector3 pointBVelocity = (pointBPosition - _pointBLastPosition) / Time.fixedDeltaTime;
            _pointBLastPosition = pointBPosition;

            /* Spring force */
            float compression = (_springLength - _hitInfo.distance) / _springLength;
            _springForce = transform.up * compression * _springRate;

            Vector3 localRelativePointVelocity = transform.InverseTransformDirection(pointBVelocity - pointAVelocity);
            float dampening = Mathf.Max(localRelativePointVelocity.y*_dampeningFactor, 0f);
            _dampeningForce = Vector3.up * dampening;

            Vector3 force = _springForce + _dampeningForce;

            _body.AddForceAtPosition(force, pointAPosition);

            /* Wheel */

            //Vector3 wheelHorizontalVelocity = pointBVelocity - Vector3.Project(pointBVelocity, _hitInfo.normal);
            //_body.AddForceAtPosition(-wheelHorizontalVelocity * _body.mass * _brakeFactor * _brakeInput, pointBPosition);

            //Vector3 wheelForward = MathUtils.ProjectOnPlane(transform.forward, _hitInfo.normal);
            //Vector3 wheelRight = MathUtils.ProjectOnPlane(transform.right, _hitInfo.normal);

            //float slipAngle = MathUtils.AngleAroundAxis(wheelForward, wheelHorizontalVelocity, _hitInfo.normal);
            //float slipMagnitude = Vector3.Project(wheelHorizontalVelocity, wheelRight).magnitude;
            
            //Vector3 slipForce = wheelRight * slipMagnitude * _body.mass * _slipCurve.Evaluate(slipAngle);
            //_body.AddForceAtPosition(slipForce, pointBPosition);

            //Debug.DrawRay(pointBPosition, slipForce / _body.mass * PhysicsUtils.DebugForceScale, Color.red);
        }
        else
        {
            _pointALastPosition = transform.position;
            _pointBLastPosition = transform.position + -transform.up * _springLength;
        }
    }

    void OnDrawGizmos()
    {
        // Debug visualization
        Gizmos.color = _isGrounded ? Color.yellow : Color.white;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawRay(transform.position, -transform.up * _springLength);

        if (!_body) {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _springForce / _body.mass * PhysicsUtils.DebugForceScale);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _dampeningForce / _body.mass * PhysicsUtils.DebugForceScale);
    }
}
