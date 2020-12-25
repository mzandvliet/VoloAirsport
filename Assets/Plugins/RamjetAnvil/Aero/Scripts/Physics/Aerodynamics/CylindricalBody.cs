using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

/*
 * Cylindrical bodies experience lift and drag.
 * 
 * The special case for cylindrical bodies is that the effects are the same no matter the roll orientation.
 */

public class CylindricalBody : MonoBehaviour, IAerodynamicSurface {
    [Dependency, SerializeField] private WindManager _wind;

    [SerializeField]
    private Rigidbody _body;
    [SerializeField]
    private AnimationCurve _liftCoeffients;
    [SerializeField]
    private float _liftCoefficientMultiplier = 1f;
    [SerializeField]
    private AnimationCurve _dragCoeffients;
    [SerializeField]
    private float _dragCoefficientMultiplier = 1f;
    [SerializeField]
    private float _referenceArea = 1f;
    [SerializeField] private Vector3 _longitudinalAxis = Vector3.up;

    public float AngleOfAttack { get; private set; }
    public Vector3 RelativeVelocity { get; private set; }
    public float AirSpeed { get; private set; }
    public Vector3 LiftForce { get; private set; }
    public Vector3 DragForce { get; private set; }
    public Vector3 MomentForce { get; private set; }

    public float Area { get { return _referenceArea; } }
    public Vector3 Center { get { return Vector3.zero; } }

    public event Action<IAerodynamicSurface> OnPreUpdate;
    public event Action<IAerodynamicSurface> OnPostUpdate;
    public event Action<IAerodynamicSurface> OnPreFixedUpdate;
    public event Action<IAerodynamicSurface> OnPostFixedUpdate;

    private void Awake() {
        _longitudinalAxis.Normalize();

        if (!_body) {
            _body = gameObject.GetComponent<Rigidbody>();
        }
    }

    public void Clear() {}

    private void Update() {
        if (OnPreUpdate != null) {
            OnPreUpdate(this);
        }

        if (OnPostUpdate != null) {
            OnPostUpdate(this);
        }
    }

    private void FixedUpdate() {
        if (_body == null || _wind == null) {
            return;
        }

        if (OnPreFixedUpdate != null) {
            OnPreFixedUpdate(this);
        }

        Vector3 centerPosition = transform.position;

        Vector3 windVelocity = _wind.GetWindVelocity(centerPosition);
        float airDensity = _wind.GetAirDensity(centerPosition);

        RelativeVelocity = _body.velocity - windVelocity;
        AirSpeed = RelativeVelocity.magnitude;

        Vector3 worldLongitudinalAxis = transform.TransformDirection(_longitudinalAxis);
        Vector3 axis = Vector3.Cross(worldLongitudinalAxis, RelativeVelocity).normalized;
        AngleOfAttack = MathUtils.AngleAroundAxis(worldLongitudinalAxis, RelativeVelocity, axis);

        float liftCoefficient = _liftCoeffients.Evaluate(AngleOfAttack) * _liftCoefficientMultiplier;
        float dragCoefficient = _dragCoeffients.Evaluate(AngleOfAttack) * _dragCoefficientMultiplier;

        float dynamicSurfacePressure = 0.5f*airDensity*_referenceArea*AirSpeed*AirSpeed;

        // Todo: can optimize by not normalizing relative velocity
        LiftForce = Vector3.Cross(RelativeVelocity, axis).normalized * (dynamicSurfacePressure*liftCoefficient);
        DragForce = -RelativeVelocity.normalized * (dynamicSurfacePressure * dragCoefficient);

        _body.AddForceAtPosition(LiftForce + DragForce, transform.position);

        if (OnPostFixedUpdate != null) {
            OnPostFixedUpdate(this);
        }
    }
}
