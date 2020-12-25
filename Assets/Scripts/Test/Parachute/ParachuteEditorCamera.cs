using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo {
    public class ParachuteEditorCamera : MonoBehaviour, ICameraMount {

        [Dependency("gameClock"), SerializeField] private AbstractUnityClock _clock;
        [Dependency, SerializeField] private Parachute _target;

        [SerializeField] private float _orbitCenterSmoothing = 0.3f;
        [SerializeField] private float _rotationSmoothing = 0.1f;
        [SerializeField] private float _orbitDistanceSmoothing = 0.2f;
        [SerializeField] private float _rotationSensitivity = 15f;
        [SerializeField] private float _inputZoomSmoothing = 0.1f;
        [SerializeField] private float _zoomSensitivity = 0.25f;
        [SerializeField] private float _verticalAngleLimit = 50f;

        private Transform _transform;
        private float _smoothZoomInput;
        private Vector3 _smoothOrbitCenter;
        private Vector2 _smoothRotationAngles;
        private Vector2 _targetRotationAngles;
        private float _smoothOrbitDistance;
        private float _zoomLevel;

        private void Initialize() {
            _transform = gameObject.GetComponent<Transform>();
        }

        private void Awake() {
            if (_target) {
                Initialize();
            }
            enabled = false;
        }

        public void OnMount(ICameraRig rig) {
            enabled = true;
        }

        public void OnDismount(ICameraRig rig) {
            enabled = false;
        }

        public void SetTarget(Parachute target) {
            _target = target;
            Initialize();
        }

        public void Center() {
            if (!_target) {
                return;
            }

            _zoomLevel = 1.2f;
            _smoothZoomInput = 0f;
            _smoothOrbitDistance = GetOrbitDistance(_target, _zoomLevel);
            _smoothOrbitCenter = GetOrbitCenter();

            _targetRotationAngles = Vector2.zero;
            _smoothRotationAngles = _targetRotationAngles;

            UpdateTransform(_smoothOrbitCenter, _smoothOrbitDistance, _smoothRotationAngles);
        }

        void Update() {
            if (!_target) {
                return;
            }

            /* Warning: Right now dragging doesn't feedback with Gizmos, but only
               because when a gizmo is interacted with, this update doesn't run.
               TODO: More explicit cursor focus integration */

            // Handle rotation input
            bool isDragging = UnityEngine.Input.GetKey(KeyCode.Mouse0); 
            Vector2 rotationInput = Vector2.zero;
            if (isDragging) {
                Vector2 input = new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
                rotationInput.x = input.y*_clock.DeltaTime * _rotationSensitivity;
                rotationInput.y = input.x*_clock.DeltaTime * _rotationSensitivity;
            }
            _targetRotationAngles = new Vector2(
                Mathf.Clamp(_targetRotationAngles.x + rotationInput.x, -_verticalAngleLimit, _verticalAngleLimit),
                MathUtils.WrapAngle(_targetRotationAngles.y + rotationInput.y)
            );
            _smoothRotationAngles.x = Mathf.Lerp(_smoothRotationAngles.x, _targetRotationAngles.x, DeltaTime(_clock, _rotationSmoothing));
            _smoothRotationAngles.y = MathUtils.LerpAngle(_smoothRotationAngles.y, _targetRotationAngles.y, DeltaTime(_clock, _rotationSmoothing));

            // Handle zoom input
            float zoomInput = UnityEngine.Input.mouseScrollDelta.y * _zoomSensitivity;
            _smoothZoomInput = Mathf.Lerp(_smoothZoomInput, zoomInput, DeltaTime(_clock, _inputZoomSmoothing));
            _zoomLevel = Mathf.Clamp(_zoomLevel - _smoothZoomInput, 0.25f, 2f);

            // Apply smoothed rotation and zoom to the actual camera transform
            _smoothOrbitCenter = Vector3.Lerp(_smoothOrbitCenter, GetOrbitCenter(), DeltaTime(_clock, _orbitCenterSmoothing));
            _smoothOrbitDistance = Mathf.Lerp(_smoothOrbitDistance, GetOrbitDistance(_target, _zoomLevel), DeltaTime(_clock, _orbitDistanceSmoothing));
            UpdateTransform(_smoothOrbitCenter, _smoothOrbitDistance, _smoothRotationAngles);
        }

        private void UpdateTransform(Vector3 orbitCenter, float orbitDistance, Vector2 rotationAngles) {
            var orbitRotation = _target.Root.rotation * GetWorldSpaceOrbitRotation(rotationAngles);
            _transform.rotation = orbitRotation;
            _transform.position = orbitCenter + orbitRotation * Vector3.back * orbitDistance;
        }

        private static Quaternion GetWorldSpaceOrbitRotation(Vector2 angles) {
            return Quaternion.Euler(0f, angles.y, 0f) * Quaternion.Euler(angles.x, 0f, 0f);
        }

        private Vector3 GetOrbitCenter() {
            // Bug: watch out for Gizmos-related feedback loops

            Vector3 centroid = _target.Root.TransformPoint(ParachuteMaths.GetCanopyCentroid(_target.Config));
            centroid = Vector3.Lerp(centroid, _target.Root.position, 0.33f);
            return centroid;
        }

        private static float GetOrbitDistance(Parachute p, float zoomLevel) {
            return UnityParachuteFactory.OrbitDistance(p) * zoomLevel;
        }

        private static float DeltaTime(IClock clock, float smoothing) {
            return clock.DeltaTime * (1f/smoothing);
        }
    }
}