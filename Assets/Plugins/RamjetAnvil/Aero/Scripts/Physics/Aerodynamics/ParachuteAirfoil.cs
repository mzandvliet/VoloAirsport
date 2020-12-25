using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

/* 
 * Share state with rest of the parachute, there's lots of duplication.
 * 
 * Data oriented approaches are possible.
 */
namespace RamjetAnvil.Volo {

    [AddComponentMenu("Aerodynamics/ParachuteAirfoil")]
    public class ParachuteAirfoil : MonoBehaviour, IAerodynamicSurface {
        [Dependency, SerializeField] private WindManager _wind;
        [Dependency("fixedClock"), SerializeField] private AbstractUnityClock _fixedClock;
    
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Vector3 _chordPressurePoint = Vector3.zero;
        [SerializeField] private AirfoilDefinition _definition;
        [SerializeField] private float _surfaceArea = 1f;
        [SerializeField] private float _totalAspectRatio = 16f;
    
        private Matrix4x4 _bodyMatrix;
        private float _sliderFactor = 1f;
        private float _optimalPressureFactor = 1f;
        private Vector3 _lastPosition;
    
        public AbstractUnityClock FixedClock {
            set { _fixedClock = value; }
        }
    
        public WindManager WindManager {
            set { _wind = value; }
        }
    
        public float AngleOfAttack { get; private set; }
    
        public float SliderFactor {
            get { return _sliderFactor; }
            set { _sliderFactor = value; }
        }
    
        public float OptimalPressureFactor {
            get { return _optimalPressureFactor; }
            set { _optimalPressureFactor = value; }
        }
    
        public float[] DeflectionInputs { get; set; }
    
        public Vector3 ChordPressurePoint {
            get { return _chordPressurePoint; }
            set { _chordPressurePoint = value; }
        }
    
        public Vector3 Center {
            get { return _chordPressurePoint; }
        }
    
        public float Area {
            get { return _surfaceArea; }
            set { _surfaceArea = value; }
        }
    
        public AirfoilDefinition Definition {
            get { return _definition; }
            set {
                _definition = value;
                DeflectionInputs = new float[_definition.Profiles.Length];
            }
        }
    
        public Vector3 RelativeVelocity { get; private set; }
        public float AirSpeed { get; private set; }
        public Vector3 LiftForce { get; private set; }
        public Vector3 DragForce { get; private set; }
        public Vector3 MomentForce { get; private set; }
    
        public float TotalAspectRatio {
            get { return _totalAspectRatio; }
            set { _totalAspectRatio = value; }
        }
    
        public Matrix4x4 BodyMatrix {
            get { return _bodyMatrix; }
        }
    
        public event Action<IAerodynamicSurface> OnPreUpdate;
        public event Action<IAerodynamicSurface> OnPostUpdate;
        public event Action<IAerodynamicSurface> OnPreFixedUpdate;
        public event Action<IAerodynamicSurface> OnPostFixedUpdate;
    
        public void Clear() { // Note: call *AFTER* teleporting, not before
            if (_body == null) {
                _body = gameObject.GetComponent<Rigidbody>();
            }
    
            UpdateBodyMatrix();
            _lastPosition = _bodyMatrix.MultiplyPoint(_chordPressurePoint);
        }
    
        private void Start() {
            Clear();
        }
    
        private void Update() {
            if (OnPreUpdate != null) {
                OnPreUpdate(this);
            }
    
            if (OnPostUpdate != null) {
                OnPostUpdate(this);
            }
    
            const float scale = 0.033f;
            var pressurePoint = _bodyMatrix.MultiplyPoint(_chordPressurePoint);
            Debug.DrawRay(pressurePoint, LiftForce*scale, Color.green);
            Debug.DrawRay(pressurePoint, DragForce*scale, Color.red);
        }
    
        private void FixedUpdate() {
            if (_wind == null) {
                return;
            }
    
            Debug.Assert(_definition != null && _definition.Profiles.Length > 1, "Airfoil failed to initialize, Profiles not set!");
    
            if (OnPreFixedUpdate != null) {
                OnPreFixedUpdate(this);
            }
    
            EvaluateForces();
    
            if (OnPostFixedUpdate != null) {
                OnPostFixedUpdate(this);
            }
        }
    
        private void EvaluateForces() {
            UpdateBodyMatrix();
    
            var liftPressurePoint = _bodyMatrix.MultiplyPoint(_chordPressurePoint);
            var dragPressurePoint = _bodyMatrix.MultiplyPoint(_chordPressurePoint);
            var windVelocity = _wind.GetWindVelocity(liftPressurePoint);
            var airDensity = _wind.GetAirDensity(liftPressurePoint);
    
            var velocity = (liftPressurePoint - _lastPosition) / _fixedClock.DeltaTime;
            _lastPosition = liftPressurePoint;
            RelativeVelocity = velocity - windVelocity;
            var relativeDirection = RelativeVelocity.normalized;
    
            var localRelativeVelocity = _bodyMatrix.inverse.MultiplyVector(RelativeVelocity);
            var pitchPlaneVelocity = new Vector2(localRelativeVelocity.z, localRelativeVelocity.y);
            AngleOfAttack = AngleSigned(Vector2.right, pitchPlaneVelocity.normalized);
    
            AirSpeed = pitchPlaneVelocity.magnitude;
            var dynamicSurfacePressure = 0.5f*airDensity*_surfaceArea*AirSpeed*AirSpeed;
    
            var worldAxis = _bodyMatrix.MultiplyVector(Vector3.right);
            var linearForce = Vector3.zero;
    
            DeflectionInputs[0] = 1f - Mathf.Min(1f, DeflectionInputs[1] + DeflectionInputs[2] + DeflectionInputs[3]);
    
            var c = _definition.GetInterpolated(AngleOfAttack, DeflectionInputs);
    
            var liftMagnitude = dynamicSurfacePressure * c.Lift * Mathf.Pow(_sliderFactor, 3f) * _optimalPressureFactor * _optimalPressureFactor;
            float aspectFactor = 1f - 1f / Mathf.Pow(_totalAspectRatio, 0.5f); // Todo: Stupid simple way to make aspect ratio matter, improve
            liftMagnitude *= aspectFactor;
    
            var liftDirection = Vector3.Cross(relativeDirection, worldAxis);
            LiftForce = liftDirection*liftMagnitude;
            linearForce += LiftForce;
    
            var dragMagnitude = dynamicSurfacePressure * c.Drag * SliderFactor * _optimalPressureFactor;
            DragForce = relativeDirection*-dragMagnitude;
            linearForce += DragForce;
    
            var momentMagnitude = dynamicSurfacePressure*c.Moment;
            MomentForce = worldAxis*momentMagnitude;
    
            if (float.IsNaN(linearForce.magnitude) || float.IsNaN(linearForce.magnitude)) {
                Debug.LogError("Airfoil Force is NaN " + RelativeVelocity + ", forcing quit...");
    #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
    #else
                Application.Quit();
    #endif
                return;
            }
    
            if (_body) {
                _body.AddForceAtPosition(linearForce, liftPressurePoint);
                _body.AddTorqueAtPosition(MomentForce, dragPressurePoint, ForceMode.Force);
            }
        }
    
        private static float AngleSigned(Vector2 a, Vector2 b) {
            var sign = -Mathf.Sign(a.x*b.y - a.y*b.x);
            return Vector2.Angle(a, b)*sign;
        }
    
        private void UpdateBodyMatrix() {
            _bodyMatrix = Matrix4x4.TRS(_body.position, _body.rotation, Vector3.one);
        }
    
    #if UNITY_EDITOR
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void DrawHandle(ParachuteAirfoil foil, GizmoType gizmosType) {
            if (!Application.isPlaying) {
                return;
            }
    
            var text = string.Format("AoA: {0:0.0}deg\nSpd: {1:0.0}m/s\nLift: {2:0}N\nDrag: {3:0}N\n" +
                                     "Defl: {4:0.0},{5:0.0},{6:0.0},{7:0.0}",
                foil.AngleOfAttack,
                foil.AirSpeed,
                foil.LiftForce.magnitude,
                foil.DragForce.magnitude,
                foil.DeflectionInputs[0],
                foil.DeflectionInputs[1],
                foil.DeflectionInputs[2],
                foil.DeflectionInputs[3]);
            Handles.Label(foil.transform.position, text, GUI.skin.box);
        }
    #endif
    }
}
