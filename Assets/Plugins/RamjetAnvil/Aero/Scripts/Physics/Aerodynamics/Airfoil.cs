using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Util;
using UnityEngine;
using RamjetAnvil.Unity.Aero;
using RamjetAnvil.Unity.Utility;

/*
 * A configurable, multi-dimensional aerodynamic surface. Define lift, drag or moment
 * effects around any local axis.
 * 
 * Todo:
 * 
 * Specify local pitch, roll and yaw axes in inspector.
 * 
 * - Get rid of the switch statement, use delegates
 * - Visualize wing & control surface configuration in edit mode
 * 
 * - Chord length per section?
 * - Interface for control surfaces, so they can affect performance of sections.
 *   - Control surface has reference to Airfoil, selects sections to modify
 *   
 * - Stat query methods for chosen axis
 * 
 * Performance:
 * - Performance might suffer because of branching
 */

[AddComponentMenu("Aerodynamics/Airfoil")]
public class Airfoil : MonoBehaviour, IAerodynamicSurface
{
    [Dependency, SerializeField] private WindManager _wind;
    [SerializeField]
    private Rigidbody _body;
    [SerializeField] 
    private Vector3 _pitchAxis = Vector3.right;
    [SerializeField] 
    private Vector3 _rollAxis = Vector3.forward;

    [SerializeField]
    private float _cordLengthA = 0.5f;
    [SerializeField]
    private float _cordLengthB = 0.5f;
    [SerializeField]
    private float _span = 2f;
    [SerializeField]
    private float _cordPressurePoint = 0.25f;

    [SerializeField]
    private AirfoilAxisResponse[] _components;
    [SerializeField]
    private int _numSections = 1;

    private float _efficiency = 1f;
    private SectionState[] _sectionStates;
    private Vector3 _lastPosition;
    private int _layerMask;

    private AirfoilAxis[] _axes;

    public SectionState[] SectionStates {
        get { return _sectionStates; }
    }
    
    public Vector3 Center { get { return _rollAxis * _cordPressurePoint; } }
    public float Area {
        get {
            return _span * (_cordLengthA + _cordLengthB) * 0.5f;
        }
    }

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

    public event Action<IAerodynamicSurface> OnPreUpdate;
    public event Action<IAerodynamicSurface> OnPostUpdate;
    public event Action<IAerodynamicSurface> OnPreFixedUpdate;
    public event Action<IAerodynamicSurface> OnPostFixedUpdate;

    //private static readonly Action<SectionForceInput>[] ForceApplicators;

    //static Airfoil()  {
    //    ForceApplicators = new Action<SectionForceInput>[3];
    //    ForceApplicators[(int) ForceType.Lift] = ApplyLift;
    //    ForceApplicators[(int)ForceType.Drag] = ApplyDrag;
    //    ForceApplicators[(int)ForceType.Moment] = ApplyMoment;
    //}

    private void Awake()
    {
        ConfigureAxes();
        RecalculateSectionProperties();
        _layerMask = LayerMaskUtil.CreateLayerMask("Player", "Head", "UI", "Interactive").Invert();

        Clear();
    }

    public void Clear() {
        _lastPosition = transform.position;

        // Todo: The less state we keep for sections, the less we have to do this
        for (int i = 0; i < SectionStates.Length; i++) {
            var section = SectionStates[i];
            section.AirSpeed = 0f;
            section.AngleOfAttack = 0f;
            section.Drag = Vector3.zero;
            section.LastSectionWorldPosition = transform.TransformPoint(section.LocalPosition);
            section.Lift = Vector3.zero;
            section.Moment = Vector3.zero;
            section.RelativeVelocity = Vector3.zero;
        }
    }

    private void ConfigureAxes() {
        Vector3 yawAxis = Vector3.Cross(_pitchAxis, _rollAxis);

        _axes = new AirfoilAxis[3];
        _axes[0] = new AirfoilAxis(_pitchAxis, _rollAxis); // Pitch
        _axes[1] = new AirfoilAxis(_rollAxis, yawAxis); // Yaw
        _axes[2] = new AirfoilAxis(yawAxis, _rollAxis); // Roll
    }

    private void RecalculateSectionProperties() {
        _sectionStates = new SectionState[_numSections];
        
        for (int i = 0; i < _numSections; i++) {
            var section = new SectionState(); 

            // Find surface area for each section (uses property of trapezoids as shortcut)
            float sectionSpan = _span / _numSections;
            float averageCordLength = Mathf.Lerp(_cordLengthA, _cordLengthB, 1f / _numSections / 2f + i / (float)_numSections);
            section.SurfaceArea = sectionSpan * averageCordLength;

            // Spread sections evenly over the width of the wing
            float spanPosition = -0.5f * _span + sectionSpan * i + sectionSpan * 0.5f;

            // Choose point along cord line
            float cordPosition = averageCordLength * _cordPressurePoint;

            section.LocalPosition = new Vector3(spanPosition, 0f, cordPosition);
            section.LastSectionWorldPosition = transform.TransformPoint(section.LocalPosition);

            _sectionStates[i] = section;
        }   
    }

    private void Update() {
        if (OnPreUpdate != null) {
            OnPreUpdate(this);
        }

        if (OnPostUpdate != null) {
            OnPostUpdate(this);
        }
    }

    private void FixedUpdate() {
        if (OnPreFixedUpdate != null) {
            OnPreFixedUpdate(this);
        }

        EvaluateForces();

        if (OnPostFixedUpdate != null) {
            OnPostFixedUpdate(this);
        }
    }

    private void EvaluateForces() {
        Transform wingTransform = transform;
        Vector3 wingPosition = wingTransform.position;

        Vector3 windVelocity = _wind.GetWindVelocity(wingPosition);
        float airDensity = _wind.GetAirDensity(wingPosition);

        Vector3 velocity = (wingPosition - _lastPosition) / Time.deltaTime;
        _lastPosition = wingPosition;
        RelativeVelocity = velocity - windVelocity;
        AirSpeed = RelativeVelocity.magnitude;

        const float groundEffectHeight = 32f;
        const float groundEffectPow = 2f;
        float groundEffectMultiplier = 1f;
        RaycastHit hit;
        if (Physics.Raycast(_body.transform.position, Vector3.down, out hit, groundEffectHeight, _layerMask)) {
            groundEffectMultiplier += Mathf.Pow(Mathf.InverseLerp(groundEffectHeight, 0f, hit.distance), groundEffectPow);
        }
        
        UpdateStats(wingTransform);

        // Calculate forces per section of the wing
        for (int i = 0; i < _numSections; i++) {
            var section = _sectionStates[i];

            Vector3 sectionWorldPosition = wingTransform.TransformPoint(section.LocalPosition);
            Vector3 sectionWorldVelocity = (sectionWorldPosition - section.LastSectionWorldPosition) / Time.fixedDeltaTime;
            section.LastSectionWorldPosition = sectionWorldPosition;
            section.RelativeVelocity = sectionWorldVelocity - windVelocity;

            section.AirSpeed = section.RelativeVelocity.magnitude;
            
            float dynamicPressure = 0.5f * airDensity * section.AirSpeed * section.AirSpeed * groundEffectMultiplier;

            section.Lift = Vector3.zero;
            section.Drag = Vector3.zero;
            section.Moment = Vector3.zero;

            //Vector3 localRelativeVelocity = wingTransform.InverseTransformDirection(section.RelativeVelocity);

            // Calculate the effects around each axis
            for (int j= 0; j < _components.Length; j++) {
                AirfoilAxisResponse coefficient = _components[j];
                AirfoilAxis airfoilAxis = _axes[(int)coefficient.AxisType];

                Vector3 worldAxis = wingTransform.TransformDirection(airfoilAxis.Axis);
                Vector3 worldAxisBase = wingTransform.TransformDirection(airfoilAxis.AxisZero);

                // Todo: Collapse this all into 2D calculations around the coefficient axis

                Vector3 projectedVelocity = section.RelativeVelocity - Vector3.Project(section.RelativeVelocity, worldAxis);
                section.AngleOfAttack = MathUtils.AngleAroundAxis(worldAxisBase, projectedVelocity, worldAxis);
                Vector3 sectionRelativeDirection = projectedVelocity.normalized;

                float forceCoefficient = coefficient.Coefficients.Evaluate(section.AngleOfAttack + section.Offset) * coefficient.Multiplier;
                float forceMagnitude = dynamicPressure * section.SurfaceArea * forceCoefficient;

                var input = new SectionForceInput() {
                    body = _body,
                    force = forceMagnitude,
                    efficiency = _efficiency,
                    section = section,
                    sectionWorldPosition = sectionWorldPosition,
                    sectionRelativeDirection = sectionRelativeDirection,
                    worldAxis = worldAxis,
                    worldAxisZero = worldAxisBase
                };

                //ForceApplicators[(int) coefficient.Type](input);
                switch (coefficient.Type) {
                    case ForceType.Lift:
                        ApplyLift(input);
                        break;
                    case ForceType.Drag:
                        ApplyDrag(input);
                        break;
                    case ForceType.Moment:
                        ApplyMoment(input);
                        break;
                }
            }
        }
    }

    private void UpdateStats(Transform wingTransform) {
        Vector3 localRelativeVelocity = wingTransform.InverseTransformDirection(RelativeVelocity);
        Vector2 pitchPlaneVelocity = new Vector2(localRelativeVelocity.z, localRelativeVelocity.y);
        AngleOfAttack = AngleSigned(pitchPlaneVelocity, Vector2.right);
    }

    private static float AngleSigned(Vector2 a, Vector2 b) {
        return (Mathf.Atan2(a.y, a.x) - Mathf.Atan2(b.y, b.x)) * Mathf.Rad2Deg;
    }

    private struct SectionForceInput {
        public Rigidbody body;
        public SectionState section;
        public float force;
        public float efficiency;
        public Vector3 sectionWorldPosition;
        public Vector3 sectionRelativeDirection;
        public Vector3 worldAxis;
        public Vector3 worldAxisZero;
    }

    // Todo: We might be able to skip a normalize and a scale operation if we use RelativeVelocity instead of RelativeDirection+Magnitude breakup

    private static void ApplyLift(SectionForceInput input) {
        Vector3 liftDirection = Vector3.Cross(input.sectionRelativeDirection, input.worldAxis);
        float liftMagnitude = input.force * Mathf.Pow(input.efficiency, 2f);
        Vector3 lift = liftDirection*liftMagnitude;
        input.section.Lift += lift;

        if (!input.body) {
            return;
        }
        input.body.AddForceAtPosition(lift, input.sectionWorldPosition);
    }

    private static void ApplyDrag(SectionForceInput input) {
        Vector3 drag = input.sectionRelativeDirection * -input.force;
        input.section.Drag += drag;

        if (!input.body) {
            return;
        }
        input.body.AddForceAtPosition(drag, input.sectionWorldPosition);
    }

    private static void ApplyMoment(SectionForceInput input) {
        Vector3 moment = input.worldAxis * input.force;
        input.section.Moment += moment;

        if (!input.body) {
            return;
        }
        input.body.AddTorqueAtPosition(moment, input.worldAxisZero, input.sectionWorldPosition, ForceMode.Force);
    }

    // Todo: Make external visualization component using Vectrocity
    //private void OnDrawGizmos() {
    //    if (!_showDebug || !Application.isPlaying || !_body)
    //        return;

    //    const float scale = PhysicsUtils.DebugForceScale;
    //    float mass = _body.mass;
    //    Transform wingTransform = transform;

    //    for (int i = 0; i < _numSections; i++) {
    //        SectionState sectionState = _sectionStates[i];
    //        Vector3 position = wingTransform.TransformPoint(sectionState.LocalPosition);

    //        Gizmos.color = Color.white;
    //        Gizmos.DrawSphere(position, 0.1f);
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawRay(position, sectionState.Lift / mass * scale);
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawRay(position, sectionState.Drag / mass * scale);
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawRay(position, sectionState.Moment / mass * scale); // Todo: visualize differently, this is torque
    //    }
    //}
}

namespace RamjetAnvil.Unity.Aero
{
    public struct AirfoilAxis
    {
        public Vector3 Axis;
        public Vector3 AxisZero;

        public AirfoilAxis(Vector3 axis, Vector3 axisZero)
        {
            Axis = axis;
            AxisZero = axisZero;
        }
    }

    [Serializable]
    public class AirfoilAxisResponse
    {
        public AxisType AxisType;
        public ForceType Type;
        public AnimationCurve Coefficients;
        public float Multiplier = 1;
        public bool Enabled = true;
    }

    public class SectionState
    {
        public float SurfaceArea;
        public Vector3 LocalPosition;
        public Vector3 LastSectionWorldPosition;

        public Vector3 RelativeVelocity;
        public float AirSpeed;
        public float AngleOfAttack;

        public Vector3 Lift;
        public Vector3 Drag;
        public Vector3 Moment;

        public float Offset;
    }

    public enum AxisType
    {
        Pitch = 0,
        Roll = 1,
        Yaw = 2
    }

    public enum ForceType
    {
        Lift,
        Drag,
        Moment
    }
}