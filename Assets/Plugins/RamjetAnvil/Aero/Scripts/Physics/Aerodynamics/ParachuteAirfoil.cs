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
    
        // Diagnostics for the parachute impulse-cascade bug, see EvaluateForces
        private const float ExtremeForce = 1e6f;
        private int _badStepCount;
        private bool _isReportingExtremeForce;
    
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
    
            var previousPosition = _lastPosition;
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
    
            // This used to hard-quit the game on a NaN force, which left nothing to debug the
            // parachute "explosion" with. Now: log the full state once, skip applying this step's
            // force, and keep running. Whether the simulation recovers is itself useful to know.
            if (!IsFinite(linearForce) || !IsFinite(MomentForce)) {
                _badStepCount++;
                if (_badStepCount == 1) {
                    Debug.LogError("[ParachuteAirfoil] Non-finite force, skipping force application. " +
                                   DescribeState(previousPosition, liftPressurePoint, windVelocity, airDensity, linearForce));
                    Debug.Break(); // Pauses play mode in the Editor, no-op in builds
                }
                LiftForce = Vector3.zero;
                DragForce = Vector3.zero;
                MomentForce = Vector3.zero;
                return;
            }
            if (_badStepCount > 0) {
                Debug.LogWarning(string.Format("[ParachuteAirfoil] '{0}' recovered after {1} non-finite step(s)",
                    name, _badStepCount));
                _badStepCount = 0;
            }
    
            // Diagnostic only, does not change behaviour: a force this large on a single canopy
            // cell means the impulse cascade is already underway, a few steps before any NaN.
            var isForceExtreme = linearForce.sqrMagnitude > ExtremeForce*ExtremeForce;
            if (isForceExtreme && !_isReportingExtremeForce) {
                Debug.LogWarning("[ParachuteAirfoil] Extreme force (> " + ExtremeForce + " N). " +
                                 DescribeState(previousPosition, liftPressurePoint, windVelocity, airDensity, linearForce));
            }
            _isReportingExtremeForce = isForceExtreme;
    
            if (_body) {
                _body.AddForceAtPosition(linearForce, liftPressurePoint);
                _body.AddTorqueAtPosition(MomentForce, dragPressurePoint, ForceMode.Force);
            }
        }
    
        private string DescribeState(Vector3 previousPosition, Vector3 pressurePoint, Vector3 windVelocity,
            float airDensity, Vector3 linearForce) {
            return string.Format(
                "airfoil='{0}' fixedFrame={1} dt={2}\n" +
                "  pressurePoint={3} previousPressurePoint={4} (a jump here means a teleport without Clear())\n" +
                "  body: position={5} velocity={6} angularVelocity={7}\n" +
                "  relativeVelocity={8} wind={9} airDensity={10} airSpeed={11} aoa={12}\n" +
                "  slider={13} optimalPressure={14} deflection=[{15}]\n" +
                "  lift={16} drag={17} moment={18} linearForce={19}",
                GetPath(transform), _fixedClock.FrameCount, _fixedClock.DeltaTime.ToString("R"),
                pressurePoint.ToString("F3"), previousPosition.ToString("F3"),
                _body ? _body.position.ToString("F3") : "<none>",
                _body ? _body.linearVelocity.ToString("F3") : "<none>",
                _body ? _body.angularVelocity.ToString("F3") : "<none>",
                RelativeVelocity.ToString("F3"), windVelocity.ToString("F3"), airDensity, AirSpeed, AngleOfAttack,
                _sliderFactor, _optimalPressureFactor,
                DeflectionInputs == null ? "" : string.Join(", ", Array.ConvertAll(DeflectionInputs, d => d.ToString("F2"))),
                LiftForce.ToString("F1"), DragForce.ToString("F1"), MomentForce.ToString("F1"), linearForce.ToString("F1"));
        }
    
        private static string GetPath(Transform t) {
            var path = t.name;
            while (t.parent != null) {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    
        private static bool IsFinite(Vector3 v) {
            return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
                   !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
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
