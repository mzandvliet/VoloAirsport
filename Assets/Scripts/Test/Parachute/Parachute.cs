using System;
using UnityEngine;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using UnityNoise;

/*
 * Todo:
 * 
 * Link airfoil definition by some custom id, from parachute config. Naive Unity ref breaks easily.
 * 
 * What we have lost in the refactor:
 * editor-time parachute tweaking with instant results
 * we need to bring this back runtime, with serialization options
 * or, for iteration 1, hack it back into the editor. Whatever works fastest.
 * 
 * Need to make all important pilot mass properties tweakable: mass, drag profile, etc.
 * 
 * 
 * - Only allow uneven NumCells, and cap between a reasonable min/max
 * - automatic mass distribution based on parameters and a kg/m3 constant
 * - Create and skin mesh (try signed distance fields! Make a small hybrid rendering thing.)
 * - Line drag
 * - Inlet design / pressure system
 *
 * =========================================
 *
 * Goal: Make this editor fully available in-game, both the physics (with constraints) and graphics styling
 * See this as a learning experience for building a parametric wingsuit designer
 * 
 * Notes: line control is faked a little. Should be entirely pilot-induced-force-based, but takes some
 * shortcuts due to physics engine limitations, complexity avoidance, and it being Good Enough.
 */

namespace RamjetAnvil.Volo {
    
    [ExecuteInEditMode]
    public class Parachute : MonoBehaviour {
        public static int Brakes = 1;
        public static int RearRisers = 2;
        public static int FrontRisers = 3;

        [SerializeField] private WindManager _wind;
        [SerializeField] private Wingsuit _pilot;
        [SerializeField] private ParachuteConfig _config;
        [SerializeField] private FixedUnityCoroutineScheduler _scheduler;
        [SerializeField] private ParachuteLineRenderer _lineRenderer;
        [SerializeField] private Renderer _canopyMesh;

        // Todo: instead of control groups as actual lists, just do clever iteration? This is messy.
        [SerializeField] private List<Section> _sections;
        [SerializeField] private List<Section> _leftToggleSections;
        [SerializeField] private List<Section> _rightToggleSections;
        [SerializeField] private List<Section> _leftRiserSections;
        [SerializeField] private List<Section> _rightRiserSections;
        private IList<Renderer> _renderers;

        [SerializeField]
        private Transform _root;

        private Transform[] _bones;
        private Perlin _p;

        // Derived properties (pre-calculated)
        private Bounds _canopyBounds;

        public Renderer CanopyMesh {
            get { return _canopyMesh; }
            set { _canopyMesh = value; }
        }

        public List<Section> Sections {
            get { return _sections; }
        }

        public List<Section> LeftToggleSections {
            get { return _leftToggleSections; }
        }

        public List<Section> RightToggleSections {
            get { return _rightToggleSections; }
        }

        public List<Section> LeftRiserSections {
            get { return _leftRiserSections; }
        }

        public List<Section> RightRiserSections {
            get { return _rightRiserSections; }
        }

        public IList<Renderer> Renderers {
            get { return _renderers; }
        }

        public ParachuteLineRenderer LineRenderer {
            get { return _lineRenderer; }
        }

        public Transform Root {
            get { return _root; }
        }

        public Wingsuit Pilot {
            get { return _pilot; }
        }

        public ParachuteConfig Config {
            get { return _config; }
        }

        public Transform[] Bones {
            get { return _bones; }
            set { _bones = value; }
        }

        public Bounds CanopyBounds {
            get { return _canopyBounds; }
        }

        public void Init(ParachuteConfig config) {
            _config = config;

            _root = gameObject.GetComponent<Transform>();

            _renderers = GetComponentsInChildren<Renderer>();

            if (_sections == null || _leftRiserSections == null || _rightRiserSections == null) {
                _sections = new List<Section>();
                _leftToggleSections = new List<Section>();
                _rightToggleSections = new List<Section>();
                _leftRiserSections = new List<Section>();
                _rightRiserSections = new List<Section>();
            }

            _sections.Clear();
            _leftToggleSections.Clear();
            _rightToggleSections.Clear();
            _leftRiserSections.Clear();
            _rightRiserSections.Clear();

            _p = new Perlin(1234);

        }

        public void Activate() {
            Root.gameObject.SetActive(true);
        }

        public void Deactivate() {
            Root.gameObject.SetActive(false);
        }

        public static readonly Quaternion DefaultUnfoldOrientation = Quaternion.identity;
        public static readonly Quaternion InflightUnfoldOrientation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));

        public void CalculateCanopyBounds() {
            _canopyBounds = UnityParachuteFactory.CanopyBounds(this);
        }

        public void AttachToPilot(Wingsuit pilot, Quaternion unfoldOrientation, GameSettingsProvider gameSettingsProvider) {
            _pilot = pilot;

            UnityParachuteFactory.SetKinematic(this);

            Root.position = _pilot.Torso.position;
            Root.rotation = _pilot.Torso.rotation;
            Root.rotation *= unfoldOrientation;

            // Layout canopy to initial pre-opened deployment state
            UnityParachuteFactory.LayoutCells(this);
            UnityParachuteFactory.Relax(this);

            for (int i = 0; i < Sections.Count; i++) {
                var section = Sections[i];
                if (section.FrontLine == null || section.RearLine == null) {
                    continue;
                }
                section.BrakeLine.Joint.connectedBody = pilot.Torso;
                section.FrontLine.Joint.connectedBody = pilot.Torso;
                section.RearLine.Joint.connectedBody = pilot.Torso;
            }

            UnityParachuteFactory.SetPhysical(this);

            if (_lineRenderer == null) {
                _lineRenderer = gameObject.AddComponent<ParachuteLineRenderer>();
                _lineRenderer.Initialize(this, gameSettingsProvider);
            }
            
            _lineRenderer.enabled = true;
        }

        public void DetachPilot() {
            if (_pilot != null) {
                _pilot = null;

                for (int i = 0; i < Sections.Count; i++) {
                    var section = Sections[i];
                    if (section.BrakeLine == null || section.FrontLine == null || section.RearLine == null) {
                        continue;
                    }
                    section.BrakeLine.SetKinematic();
                    section.FrontLine.SetKinematic();
                    section.RearLine.SetKinematic();

                    section.BrakeLine.Joint.connectedBody = null;
                    section.FrontLine.Joint.connectedBody = null;
                    section.RearLine.Joint.connectedBody = null;
                }

                _lineRenderer.enabled = false;
            }
        }

        private IAwaitable _deployFibre;

        public void Deploy() {
            if (_deployFibre != null) {
                _deployFibre.Dispose();
                _deployFibre = null;
            }

            _deployFibre = _scheduler.Run(DeployAsync());
        }

        private IEnumerator<WaitCommand> DeployAsync() {
            const float start = 0.1f;
            const float dur = 3f;

            float t = 0f;
            while (t < dur) {
                SetSlider(start + t / dur * (1f - start));

                for (int i = 0; i < LeftToggleSections.Count; i++) {
                    LeftToggleSections[i].BrakeLine.ApplyPull(.5f);
                }

                for (int i = 0; i < RightToggleSections.Count; i++) {
                    RightToggleSections[i].BrakeLine.ApplyPull(.5f);
                }

                t += _scheduler.Clock.DeltaTime;
                yield return WaitCommand.WaitForNextFrame;
            }

            for (int i = 0; i < LeftToggleSections.Count; i++) {
                LeftToggleSections[i].BrakeLine.ApplyPull(0f);
            }

            for (int i = 0; i < RightToggleSections.Count; i++) {
                RightToggleSections[i].BrakeLine.ApplyPull(0f);
            }
        }

        public void Inject(WindManager wind, AbstractUnityClock fixedClock, FixedUnityCoroutineScheduler scheduler) {
            _scheduler = scheduler;
            for (int i = 0; i < Sections.Count; i++) {
                var section = Sections[i];
                section.Cell.Airfoil.WindManager = wind;
                section.Cell.Airfoil.FixedClock = fixedClock;
            }
        }

        private void SetSlider(float slider) {
            for (int i = 0; i < _sections.Count; i++) {
                var section = _sections[i];
                section.Cell.Airfoil.SliderFactor = slider;
            }
        }

        void LateUpdate() {
            // Mesh animation
            for (int i = 0; i < Sections.Count; i++) {
                _bones[i].position = Sections[i].Cell.transform.position;
                _bones[i].rotation = Sections[i].Cell.transform.rotation;

                float judder = 1f + _p.Noise(Time.time * 7f, i * 2.3f) * 0.05f;
                _bones[i].localScale = Vector3.one * judder;
            }
        }

        void FixedUpdate() {
            Simulate();
        }

        void Simulate() {
            for (int i = 0; i < _sections.Count-1; i++) {
                var cellL = _sections[i].Cell;
                var cellR = _sections[i+1].Cell;
                
                float pressureL = GetPressureForCell(cellL);
                float pressureR = GetPressureForCell(cellR);
                float normalizedPressureL = GetNormalizedPressureForCell(cellL);
                float normalizedPressureR = GetNormalizedPressureForCell(cellR);
                cellL.SetPressure(
                    (pressureL+pressureR) * 0.5f * _config.PressureMultiplier,
                    (normalizedPressureL+normalizedPressureR) * 0.5f);
            }
        }

        public void Destroy(TimeSpan after) {
            GameObject.Destroy(gameObject, (float) after.TotalSeconds);
        }

        // unscientific, stupid, but entirely stateless pressure based on airspeed^2 and AoA
        private static float GetPressureForCell(Cell cell) {
            return 0.33f + 0.66f *
                Mathf.Pow(cell.Airfoil.AirSpeed, 2f) *
                Mathf.Max(0f, Mathf.Pow(Mathf.Abs(Vector3.Dot(cell.Airfoil.RelativeVelocity.normalized, cell.Transform.forward)), 0.025f));
        }

        private static float GetNormalizedPressureForCell(Cell cell) {
            return Mathf.Pow(Mathf.Abs(Vector3.Dot(cell.Airfoil.RelativeVelocity.normalized, cell.transform.forward)), 0.025f) * Mathf.Max(cell.Airfoil.AirSpeed / 20f, 1f);
        }
    }

    

    [System.Serializable]
    public class Section {
        [SerializeField] private Cell _cell;
        [SerializeField] private Line _brakeLine;
        [SerializeField] private Line _rearLine;
        [SerializeField] private Line _frontLine;

        public void SetKinematic() {
            _cell.SetKinematic();

    //      if (_brakeLine != null) { // Phantom line, ignore its state for now
    //          _brakeLine.SetKinematic();    
    //      }
            if (_rearLine != null) {
                _rearLine.SetKinematic();
            }
            if (_frontLine != null) {
                _frontLine.SetKinematic();
            }
        }

        public void SetPhysical() {
            _cell.SetPhysical();

    //      if (_brakeLine != null) { // Phantom line, ignore its state for now
    //          _brakeLine.SetPhysical();    
    //      }
            if (_rearLine != null) {
                _rearLine.SetPhysical();
            }
            if (_frontLine != null) {
                _frontLine.SetPhysical();
            }
        }

        public Cell Cell {
            get { return _cell; }
            set { _cell = value; }
        }

        public Line BrakeLine {
            get { return _brakeLine; }
            set { _brakeLine = value; }
        }

        public Line RearLine {
            get { return _rearLine; }
            set { _rearLine = value; }
        }

        public Line FrontLine {
            get { return _frontLine; }
            set { _frontLine = value; }
        }
    }

    [System.Serializable]
    public class Line {
        private readonly ConfigurableJoint _joint;
        private readonly float _length;
        private readonly ParachuteAirfoil _airfoil;
        private readonly int _lineType;
        private readonly float _pullLength;
        private SoftJointLimit _limit;

        public float Pull { get; private set; }

        public ConfigurableJoint Joint {
            get { return _joint; }
        }

        public float Length {
            get { return _length; }
        }

        public void SetPhysical() {
            // Make sure we don't accidentally set this on a destroyed object
            if (_joint != null) { 
                _joint.xMotion = ConfigurableJointMotion.Limited;
                _joint.yMotion = ConfigurableJointMotion.Limited;
                _joint.zMotion = ConfigurableJointMotion.Limited;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
            }
        }

        public void SetKinematic() {
            // Make sure we don't accidentally set this on a destroyed object
            if (_joint != null) {
                _joint.xMotion = ConfigurableJointMotion.Free;
                _joint.yMotion = ConfigurableJointMotion.Free;
                _joint.zMotion = ConfigurableJointMotion.Free;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
            }
        }

        public Line(ConfigurableJoint joint, ParachuteAirfoil airfoil, int lineType) {
            if (joint != null) {
                _joint = joint;
                _limit = joint.linearLimit;
                _length = _limit.limit;
            }

            _airfoil = airfoil;
            _lineType = lineType;

            switch (lineType) {
                case 1:
                    _pullLength = 0.15f;
                    break;
                case 2:
                    _pullLength = 0.3f;
                    break;
                case 3:
                    _pullLength = 0.3f;
                    break;
            }
        }

        public void ApplyPull(float input) {
            Pull = input;

            if (_airfoil) {
                _airfoil.DeflectionInputs[_lineType] = input;
            }

            if (_joint) {
                _limit.limit = _length - input * _pullLength;
                _joint.linearLimit = _limit;
            }
        }
    }
}
