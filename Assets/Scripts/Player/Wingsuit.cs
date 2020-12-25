using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class Wingsuit : MonoBehaviour, ISpawnable {

        [SerializeField]
        private FlightStatistics _flightStatistics;
        [SerializeField]
        private CollisionEventSource _collisionEventSource;
        [SerializeField]
        private TrajectoryVisualizer _trajectoryVisualizer;
        [SerializeField]
        private AerodynamicsVisualizationManager _aerodynamicsVisualizationManager;
        [SerializeField]
        private PlayerController _playerController;
        [SerializeField]
        private PilotAnimator _pilotAnimator;
        [SerializeField]
        private Rigidbody _torso;
        [SerializeField]
        private AlphaManager _alphaManager;
        [SerializeField]
        private CameraMount _headCameraMount;

        [SerializeField] private Airfoil1D[] _wingsuitWings;

        private IList<Rigidbody> _rigidbodies;
        private IList<Transform> _transforms;
        private IList<IAerodynamicSurface> _aerodynamicSurfaces;

        private IDictionary<Transform, ImmutableTransform> _initialTransforms;
        private IDictionary<Rigidbody, bool> _kinematicState;

        private IList<AdaptiveTrailRenderer> _adaptiveTrailRenderers;
        private IList<JointController> _jointControllers;

        private bool _isInitialized;

        void Awake() {
            Initialize();
        }

        private void Initialize() {
            if (!_isInitialized) {
                _isInitialized = true;

                _rigidbodies = GetComponentsInChildren<Rigidbody>(true);
                _transforms = GetComponentsInChildren<Transform>(true);
                _playerController = GetComponent<PlayerController>();
                _aerodynamicSurfaces = GetComponentsInChildren<IAerodynamicSurface>();
                _adaptiveTrailRenderers = GetComponentsInChildren<AdaptiveTrailRenderer>(includeInactive: true);
                _jointControllers = GetComponentsInChildren<JointController>(includeInactive: true);

                _initialTransforms = new Dictionary<Transform, ImmutableTransform>();
                for (int i = 0; i < _transforms.Count; i++) {
                    var t = _transforms[i];
                    _initialTransforms.Add(t, t.MakeLocalImmutable());
                }

                _kinematicState = new Dictionary<Rigidbody, bool>();
                for (int i = 0; i < _rigidbodies.Count; i++) {
                    var r = _rigidbodies[i];
                    _kinematicState.Add(r, r.isKinematic);
                }
            }
        }

        public void SetKinematic() {
            for (int i = 0; i < Rigidbodies.Count; i++) {
                var r = Rigidbodies[i];
                r.isKinematic = true;
            }
        }

        public void SetPhysical() {
            for (int i = 0; i < Rigidbodies.Count; i++) {
                var r = Rigidbodies[i];
                r.isKinematic = _kinematicState[r];
            }
        }

        public void DisableWings() {
            for (int i = 0; i < _wingsuitWings.Length; i++) {
                _wingsuitWings[i].gameObject.SetActive(false);
            }
        }

        public void EnableWings()
        {
            for (int i = 0; i < _wingsuitWings.Length; i++) {
                // Todo: better aligner management
                _wingsuitWings[i].gameObject.SetActive(true);
                _wingsuitWings[i].gameObject.GetComponent<Aligner>().Align(_wingsuitWings[i]);
                _wingsuitWings[i].Clear();
            }
        }

        public void OnSpawn() {
            Initialize();

            for (int i = 0; i < Rigidbodies.Count; i++) {
                var r = Rigidbodies[i];
                r.isKinematic = _kinematicState[r];
                if (!r.isKinematic) {
                    r.velocity = Vector3.zero;
                    r.angularVelocity = Vector3.zero;
                }
            }

            for (int i = 0; i < AerodynamicSurfaces.Count; i++) {
                AerodynamicSurfaces[i].Clear();
            }

            FlightStatistics.OnSpawn();
            Controller.Clear();
            for (int i = 0; i < _adaptiveTrailRenderers.Count; i++) {
                var adaptiveTrailRenderer = _adaptiveTrailRenderers[i];
                adaptiveTrailRenderer.OnSpawn();
            }
            for (int i = 0; i < _jointControllers.Count; i++) {
                var jointController = _jointControllers[i];
                jointController.OnSpawn();
            }
        }

        public void OnDespawn() {
            SetKinematic();

            for (int i = 0; i < Transforms.Count; i++) {
                var t = Transforms[i];
                var initialTransform = _initialTransforms[t];
                t.SetLocal(initialTransform);
            }
        }

        public Rigidbody Torso {
            get {
                return _torso;
            }
        }

        public CameraMount HeadCameraMount {
            get { return _headCameraMount; }
        }

        public IList<Rigidbody> Rigidbodies {
            get {
                Initialize();
                return _rigidbodies;
            }
        }

        public IList<Transform> Transforms {
            get {
                Initialize();
                return _transforms;
            }
        }

        public IList<IAerodynamicSurface> AerodynamicSurfaces {
            get {
                Initialize();
                return _aerodynamicSurfaces;
            }
        }

        public FlightStatistics FlightStatistics {
            get { return _flightStatistics; }
        }

        public CollisionEventSource CollisionEventSource {
            get { return _collisionEventSource; }
        }

        public TrajectoryVisualizer TrajectoryVisualizer {
            get { return _trajectoryVisualizer; }
        }

        public AerodynamicsVisualizationManager AerodynamicsVisualizationManager {
            get { return _aerodynamicsVisualizationManager; }
        }

        public AlphaManager AlphaManager {
            get { return _alphaManager; }
        }

        public PlayerController Controller {
            get { return _playerController; }
        }

        public PilotAnimator PilotAnimator {
            get { return _pilotAnimator; }
        }
    }
}
