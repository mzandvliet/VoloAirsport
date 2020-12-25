using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using RamjetAnvil.Unity.Aero;
#if UNITY_EDITOR
using UnityEditor;
#endif


[AddComponentMenu("Aerodynamics/Airfoil1D")]
public class Airfoil1D : MonoBehaviour, IAerodynamicSurface
{
    [Dependency, SerializeField] private WindManager _wind;

    [SerializeField] private Rigidbody _body;
    [SerializeField] private Vector3 _pitchAxis = Vector3.right;
    [SerializeField] private Vector3 _rollAxis = Vector3.forward;

    [SerializeField] private float _surfaceArea = 1f;
    [SerializeField] private Vector3 _chordPressurePoint = Vector3.zero;

    [SerializeField] private AirfoilAxisResponse _liftResponse;
    [SerializeField] private AirfoilAxisResponse _dragResponse;
    [SerializeField] private AirfoilAxisResponse _momentResponse;

    [SerializeField] private bool _debug;

    private Transform _transform;
    private float _efficiency;
    private Vector3 _lastPosition;
    
    public event Action<IAerodynamicSurface> OnPreUpdate;
    public event Action<IAerodynamicSurface> OnPostUpdate;
    public event Action<IAerodynamicSurface> OnPreFixedUpdate;
    public event Action<IAerodynamicSurface> OnPostFixedUpdate;

	public WindManager WindManager {
		get { return _wind; }
		set { _wind = value; }
	}

    public Vector3 Center { get { return _chordPressurePoint; } }
	public float Area { get { return _surfaceArea; } set { _surfaceArea = value; } }
    public Vector3 RelativeVelocity { get; private set; }
    public float AirSpeed { get; private set; }
    public Vector3 LiftForce { get; private set; }
    public Vector3 DragForce { get; private set; }
    public Vector3 MomentForce { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float Efficiency {
        get { return _efficiency; }
        set { _efficiency = Mathf.Clamp01(value); }
    }

	public AirfoilAxisResponse LiftResponse {
		get { return _liftResponse; }
		set { _liftResponse = value; }
	}

	public AirfoilAxisResponse DragResponse {
		get { return _dragResponse; }
		set { _dragResponse = value; }
	}

	public AirfoilAxisResponse MomentRepsonse {
		get { return _momentResponse; }
		set { _momentResponse = value; }
	}

    
    private void Awake() {
        _transform = gameObject.GetComponent<Transform>();
        _efficiency = 1f;
        
        Clear();
    }

    public void Clear() {
		if (_body == null) {
			_body = gameObject.GetComponent<Rigidbody>();
		}

        _lastPosition = transform.position;
    }

    private void Update() {
        if (OnPreUpdate != null) {
            OnPreUpdate(this);
        }

        if (OnPostUpdate != null) {
            OnPostUpdate(this);
        }
    }

    private void FixedUpdate()
    {
        if (_wind == null) {
            return;
        }

        if (OnPreFixedUpdate != null) {
            OnPreFixedUpdate(this);
        }

        EvaluateForces();

        if (OnPostFixedUpdate != null) {
            OnPostFixedUpdate(this);
        }
    }

    private void EvaluateForces()
    {
        Vector3 position = _transform.position;

        Vector3 windVelocity = _wind.GetWindVelocity(position);
        float airDensity = _wind.GetAirDensity(position);

        Vector3 velocity = (position - _lastPosition) / Time.deltaTime;
        _lastPosition = position;
        RelativeVelocity = velocity - windVelocity;
        AirSpeed = RelativeVelocity.magnitude;

        Vector3 relativeDirection = RelativeVelocity.normalized;

        float dynamicSurfacePressure = 0.5f * airDensity * _surfaceArea * AirSpeed * AirSpeed;

        // Bug: This method of determining angle of attack only works for the default axis configuration!!
        // Bug: if lift coeff goes negative, we get NaNs really quickly!

        Vector3 localRelativeVelocity = _transform.InverseTransformDirection(RelativeVelocity);
        Vector2 pitchPlaneVelocity = new Vector2(localRelativeVelocity.z, localRelativeVelocity.y);
        AngleOfAttack = AngleSigned(Vector2.right, pitchPlaneVelocity);

        if (float.IsNaN(AngleOfAttack)) {
            Debug.LogError("Angle of Attack is NaN " + RelativeVelocity);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            return;
        }

        Vector3 worldAxis = _transform.TransformDirection(_pitchAxis);

        Vector3 linearForce = Vector3.zero;

        if (_liftResponse.Enabled) {
            float liftCoefficient = _liftResponse.Coefficients.Evaluate(AngleOfAttack) * _liftResponse.Multiplier;
            float liftMagnitude = dynamicSurfacePressure * liftCoefficient * Efficiency;
            Vector3 liftDirection = Vector3.Cross(relativeDirection, worldAxis);
            LiftForce = liftDirection * liftMagnitude;
            linearForce += LiftForce;
        }

        if (_dragResponse.Enabled) {
            float dragCoefficient = _dragResponse.Coefficients.Evaluate(AngleOfAttack) * _dragResponse.Multiplier;
            float dragMagnitude = dynamicSurfacePressure * dragCoefficient * Efficiency;
            DragForce = relativeDirection * -dragMagnitude;
            linearForce += DragForce;
        }

        if (_momentResponse.Enabled) {
            float momentCoefficient = _momentResponse.Coefficients.Evaluate(AngleOfAttack) * _momentResponse.Multiplier;
            float momentMagnitude = dynamicSurfacePressure * momentCoefficient;
            MomentForce = worldAxis * momentMagnitude;
        }

        Vector3 pressurePoint = _transform.TransformPoint(_chordPressurePoint); // Todo: only needed if applying force from within this script

        if (_body) {
            _body.AddForceAtPosition(linearForce, pressurePoint);
            _body.AddTorqueAtPosition(MomentForce, _rollAxis, pressurePoint, ForceMode.Force);
        }
    }

    private static float AngleSigned(Vector2 a, Vector2 b) {
        return (Mathf.Atan2(a.y, a.x) - Mathf.Atan2(b.y, b.x)) * Mathf.Rad2Deg;
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }

        const float scale = 0.01f;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_transform.position, LiftForce * scale);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_transform.position, DragForce * scale);
    }
}