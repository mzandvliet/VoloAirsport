/**
 * Created by Martijn Zandvliet, 2014
 * 
 * Use of the source code in this document is governed by the terms
 * and conditions described in the Ramjet Anvil End-User License Agreement.
 * 
 * A copy of the Ramjet Anvil EULA should have been provided in the purchase
 * of the license to this source code. If you do not have a copy, please
 * contact me.
 * 
 * For any inquiries, contact: martijn@ramjetanvil.com
 */

using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityExecutionOrder;

public enum AvatarType {
    Wingsuit,
    Parachute
}

[Run.After(typeof(FlightStatistics))]
public class ThirdPersonCameraController : MonoBehaviour, ICameraMount {
    [Dependency("gameClock"), SerializeField] private AbstractUnityClock _clock;
    [Dependency, SerializeField] private IReadonlyRef<PilotActionMap> _playerActionMap;
    [Dependency, SerializeField] private FlightStatistics _target;
    [Dependency, SerializeField] private UnityCoroutineScheduler _scheduler;

    [SerializeField]
    private Config _wingsuitConfig;
    [SerializeField]
    private Config _parachuteConfig;
    [SerializeField]
    private float _configTransitionDuration = 3f;

    private static readonly Vector3 CameraUp = Vector3.up;

    private bool _vrMode;
    private Transform _transform;

    private Vector2 _input;
    private bool _mouseLook;
    private Vector2 _bufferedInput;

    private Vector3 _lastPositionSwayOffset;
    private Vector3 _lastLocalPositionOffsetVelocity;
    private Vector3 _lastCameraOrbitDirection;
    private Vector3 _lastLookDirection;
    private Vector3 _nonZeroFlightVelocity;

    private Quaternion _lastOrbitRotation;
    private Quaternion _lastLookRotation;

    private Config _config;
    private double _transitionStartTime;
    private AvatarType _targetType;

    public IReadonlyRef<PilotActionMap> PlayerActionMap {
        set { _playerActionMap = value; } // todo: please, no real input handling in this class
    }

    public bool VrMode {
        get { return _vrMode; }
        set { _vrMode = value; }
    }

    private void Awake() {
        _transform = gameObject.GetComponent<Transform>();
        _config = _wingsuitConfig;

        if (_target) {
            Clear();
        }
    }

    public void OnMount(ICameraRig rig) {
        enabled = true;
    }

    public void OnDismount(ICameraRig rig) {
    }

    public void SetWingsuitTarget(FlightStatistics target) {
        _target = target;
        _targetType = AvatarType.Wingsuit;
        _transitionStartTime = _clock.CurrentTime;
    }

    public void SetParachuteTarget(FlightStatistics target, Parachute parachute) {
        _target = target;
        _targetType = AvatarType.Parachute;
        _parachuteConfig.MinOrbitDistance = UnityParachuteFactory.OrbitDistance(parachute);
        _transitionStartTime = _clock.CurrentTime;
    }

    public void RemoveTarget() {
        _target = null;
    }

    public void Clear() {
        if (!_target) {
            return;
        }

        Vector3 lookDir = Vector3.Slerp(
            _target.transform.TransformDirection(_target.RollAxis),
            Vector3.down,
            0.5f);

        InitializeState(lookDir);
        UpdateTransform();
    }

    // This resets all state used to smooth translations and rotations
    private void InitializeState(Vector3 direction) {
        _lastLookDirection = direction;
        _lastCameraOrbitDirection = direction;
        _lastPositionSwayOffset = Vector3.zero;
        _lastLocalPositionOffsetVelocity = Vector3.zero;
        _nonZeroFlightVelocity = direction;

        _lastOrbitRotation = Quaternion.LookRotation(direction, CameraUp);
        _lastLookRotation = _lastOrbitRotation;
    }

    private void Update() {
        if (_target == null || _clock == null) {
            return;
        }

        var lerp = (_clock.CurrentTime - _transitionStartTime) / _configTransitionDuration;
        var targetConfig = _targetType == AvatarType.Wingsuit ? _wingsuitConfig : _parachuteConfig;
        _config = Config.Lerp(_config, targetConfig, (float) lerp);

        UpdateInput();
        UpdateTransform();
    }

    public void UpdateTransform() {
        var c = _config;

        float vrMult = _vrMode ? 0.25f : 1.0f;

        var inputRotation = GetInputRotation();
        var inputMagnitude = GetInputMagnitude();

        /* ---- Find relevant flight information ---- */

        float targetSpeed = _target.WorldVelocity.magnitude;
        float deadZonedSpeed = Mathf.Max(targetSpeed - c.SpeedDeadzone, 0f);
        float speedFactor = Mathf.Clamp(deadZonedSpeed / c.FlightSpeedMax, 0f, 1f); // Used to avoid looking along flight direction when speed is near-zero

        var flightVelocity = GetPredictedVelocity(c.FlightDirectionPredictionTime);

        if (flightVelocity.magnitude > c.SpeedDeadzone) {
            _nonZeroFlightVelocity = Vector3.Lerp(_nonZeroFlightVelocity, flightVelocity, _clock.DeltaTime).normalized;
        }

        // Smoothly interpolate from 0-speed regime to flying regime based on speed
        var cameraOrbitDirection = Vector3.Slerp(_nonZeroFlightVelocity, flightVelocity.normalized, speedFactor);
        cameraOrbitDirection.Normalize();
        
        // Smooth basic orbit rotation over time
        cameraOrbitDirection = Vector3.Slerp(_lastCameraOrbitDirection, cameraOrbitDirection, c.FlightDirectionSmoothingSpeed * _clock.DeltaTime);
        _lastCameraOrbitDirection = cameraOrbitDirection;

        /* ---- Find orbit position ---- */

        // Handle case of looking down along world Y (avoids singularity)
        float orbitAngleToDeadzone = Mathf.Max(Vector3.Angle(cameraOrbitDirection, -CameraUp) - c.YAxisDeadzone, 0f);
        if (orbitAngleToDeadzone < c.YAxisDeadzoneFalloff) {
            float deadzoneLerp = 1f - (orbitAngleToDeadzone / c.YAxisDeadzoneFalloff);
            cameraOrbitDirection = Vector3.Slerp(cameraOrbitDirection, _nonZeroFlightVelocity.normalized, deadzoneLerp);
        }
        
        if (Math.Abs(cameraOrbitDirection.magnitude) < 0.01f) {
            cameraOrbitDirection = Vector3.forward;
        }

        // Todo: Look rotation viewing vector is zero
        Quaternion orbitLookRotation = Quaternion.LookRotation(cameraOrbitDirection, CameraUp);

        // Combine rotations to find final orbit rotation
        Quaternion baseRotationOffset = Quaternion.Euler(c.BaseRotationOffset);
        Quaternion orbitRotationOffset = baseRotationOffset;// * groundProximityRotationOffset;
        Quaternion orbitRotation = orbitLookRotation * inputRotation * orbitRotationOffset;

        orbitRotation = Quaternion.Slerp(_lastOrbitRotation, orbitRotation, c.OrbitRotationSpeed*vrMult*_clock.DeltaTime);
        _lastOrbitRotation = orbitRotation;

        float orbitDistance = c.MinOrbitDistance * c.ZoomFactor;// + 0.5f * c.MaxAccelerationMagnitude;
        Vector3 positionOrbitOffset = orbitRotation * new Vector3(0f, 0f, -orbitDistance);

        Vector3 orbitCenter = _target.transform.TransformPoint(c.BasePositionOffset);
        Vector3 cameraPosition = orbitCenter + positionOrbitOffset;

        Vector3 sway = GetSwayOffset(orbitCenter, cameraPosition, c) * speedFactor;
        cameraPosition += sway;

        _transform.position = cameraPosition;

        /* ---- Find camera rotation ---- */

        Quaternion lookRotation = GetLookRotation(c, inputMagnitude, speedFactor, cameraOrbitDirection);
        lookRotation = Quaternion.Slerp(_lastLookRotation, lookRotation, c.LookRotationSpeed*vrMult*_clock.DeltaTime);
        _lastLookRotation = lookRotation;

        _transform.rotation = lookRotation;
    }

    private Quaternion GetInputRotation() {
        Quaternion inputRotation;

        if (_mouseLook) {
            _bufferedInput += _input;
            inputRotation = Quaternion.Euler(_bufferedInput.y * 3f, _bufferedInput.x * 3f, 0f);
        }
        else {
            inputRotation = Quaternion.Euler(_input.y * 90f, _input.x * 90f, 0f);
            _bufferedInput = Vector2.zero;
        }
        return inputRotation;
    }

    private float GetInputMagnitude() {
        return _mouseLook ? 1f : Mathf.Max(Mathf.Abs(_input.x), Mathf.Abs(_input.y));
    }

    private Vector3 GetSwayOffset(Vector3 orbitCenter, Vector3 basePos, Config c) {
        Vector3 acceleration = _target.Acceleration;
        acceleration.y = Mathf.Min(0f, acceleration.y);

        // Scale it in screenspace
        acceleration = _transform.InverseTransformDirection(acceleration);
        Vector3 localSway = new Vector3(
            -acceleration.x * c.MinOrbitDistance * c.AccelerationLinearScaling.x,
            -acceleration.y * c.MinOrbitDistance * c.AccelerationLinearScaling.y,
            -acceleration.z * c.MinOrbitDistance * c.AccelerationLinearScaling.z
        );
        Vector3 sway = _transform.TransformDirection(localSway);

        // Scale it logarithmically
        float swayMagnitude = sway.magnitude;
        sway.Normalize();
        sway *= Mathf.Log(1f + swayMagnitude, c.AccelerationLogScaling);

        // Rudimentary clipping of any acceleration bringing the camera too close to the character
        const float minCamDist = 2.3f;
        Vector3 targetDelta = orbitCenter - (basePos + sway);
        float deltaMag = targetDelta.magnitude;
        if (deltaMag < minCamDist) {
            sway -= targetDelta / deltaMag * (minCamDist - deltaMag);
        }

        // Smooth it
        sway = Vector3.SmoothDamp(_lastPositionSwayOffset, sway, ref _lastLocalPositionOffsetVelocity, 1f / c.AccelerationSmoothing);
        _lastPositionSwayOffset = sway;

        return sway;
    }

    private Quaternion GetLookRotation(Config c, float inputMagnitude, float speedFactor, Vector3 cameraOrbitDirection) {
        Vector3 targetDirection = (_target.transform.position - transform.position).normalized;

        float angleToCenterScreen = Vector3.Angle(targetDirection, _transform.forward);
        float offScreenFactor = Mathf.Clamp01(angleToCenterScreen / c.FieldOfViewConstraint);

        // The more the character goes off-screen, or the stronger the user input, the more we forcibly look at the target directly
        float lookDirectionLerp = Mathf.Min(1f - offScreenFactor, 1f - inputMagnitude) * speedFactor;
        Vector3 lookDirection = Vector3.Slerp(targetDirection, cameraOrbitDirection, lookDirectionLerp);

        // Handle case of looking down along world Y (skirts around singularity)
        float lookAngleToDeadzone = Mathf.Max(Vector3.Angle(lookDirection, -CameraUp) - c.YAxisDeadzone, 0f);
        if (lookAngleToDeadzone < c.YAxisDeadzoneFalloff) {
            float deadzoneLerp = 1f - (lookAngleToDeadzone / c.YAxisDeadzoneFalloff);
            lookDirection = Vector3.Slerp(lookDirection, _lastLookDirection, deadzoneLerp);
        }
        _lastLookDirection = lookDirection;

        return Quaternion.LookRotation(lookDirection, CameraUp);
    }

    private void UpdateInput() {
        if (_playerActionMap != null) {
            Vector2 input = new Vector2(_playerActionMap.V.PollAxis(WingsuitAction.LookHorizontal),
                                        _playerActionMap.V.PollAxis(WingsuitAction.LookVertical));
            _input = InputUtilities.CircularizeInput(input);

            _mouseLook = _playerActionMap.V.PollButton(WingsuitAction.ActivateMouseLook) == ButtonState.Pressed;
        }
    }

    private Vector3 GetPredictedVelocity(float lookAheadTime) {
        // Look along trajectory. The faster we go, the more forward in time we look along the trajectory, i.e. the more we anticipate.

        //Debug.Log("target velocity " + _target.WorldVelocity + " prediction time " + _configs.FlightDirectionPredictionTime);
        float time = Mathf.Pow(Mathf.InverseLerp(0f, 50f, _target.WorldVelocity.magnitude), 2f) * lookAheadTime;
        return _target.GetInterpolatedTrajectory(time).Velocity.normalized;
    }

    [System.Serializable]
    public struct Config {
        [SerializeField]
        public float MinOrbitDistance;
        [SerializeField]
        public float ZoomFactor;
        [SerializeField]
        public Vector3 BasePositionOffset;
        [SerializeField]
        public Vector3 BaseRotationOffset;
        [SerializeField]
        public float ProximityRotationOffset;
        [SerializeField]
        public float OrbitRotationSpeed;
        [SerializeField]
        public float LookRotationSpeed;
        [SerializeField]
        public float FlightDirectionPredictionTime;
        [SerializeField]
        public float FlightDirectionSmoothingSpeed;

        [SerializeField]
        public float SpeedDeadzone;
        [SerializeField]
        public float FlightSpeedMax;

        [SerializeField]
        public float YAxisDeadzone;
        [SerializeField]
        public float YAxisDeadzoneFalloff;

        [SerializeField]
        public float AccelerationSmoothing;
        [SerializeField]
        public Vector3 AccelerationLinearScaling;
        [SerializeField]
        public float AccelerationLogScaling;

        [SerializeField]
        public float FieldOfViewConstraint;

        public static Config Lerp(Config a, Config b, float lerp) {
            lerp = Mathf.Min(lerp, 1f);

            return new Config {
                MinOrbitDistance = Mathf.Lerp(a.MinOrbitDistance, b.MinOrbitDistance, lerp),
                ZoomFactor = Mathf.Lerp(a.ZoomFactor, b.ZoomFactor, lerp),
                BasePositionOffset = Vector3.Lerp(a.BasePositionOffset, b.BasePositionOffset, lerp),
                BaseRotationOffset = Vector3.Lerp(a.BaseRotationOffset, b.BaseRotationOffset, lerp),
                ProximityRotationOffset = Mathf.Lerp(a.ProximityRotationOffset, b.ProximityRotationOffset, lerp),
                OrbitRotationSpeed = Mathf.Lerp(a.OrbitRotationSpeed, b.OrbitRotationSpeed, lerp),
                LookRotationSpeed = Mathf.Lerp(a.LookRotationSpeed, b.LookRotationSpeed, lerp),
                FlightDirectionPredictionTime = Mathf.Lerp(a.FlightDirectionPredictionTime, b.FlightDirectionPredictionTime, lerp),
                FlightDirectionSmoothingSpeed = Mathf.Lerp(a.FlightDirectionSmoothingSpeed, b.FlightDirectionSmoothingSpeed, lerp),
                SpeedDeadzone = Mathf.Lerp(a.SpeedDeadzone, b.SpeedDeadzone, lerp),
                FlightSpeedMax = Mathf.Lerp(a.FlightSpeedMax, b.FlightSpeedMax, lerp),
                YAxisDeadzone = Mathf.Lerp(a.YAxisDeadzone, b.YAxisDeadzone, lerp),
                YAxisDeadzoneFalloff = Mathf.Lerp(a.YAxisDeadzoneFalloff, b.YAxisDeadzoneFalloff, lerp),
                AccelerationSmoothing = Mathf.Lerp(a.AccelerationSmoothing, b.AccelerationSmoothing, lerp),
                AccelerationLinearScaling = Vector3.Lerp(a.AccelerationLinearScaling, b.AccelerationLinearScaling, lerp),
                AccelerationLogScaling = Mathf.Lerp(a.AccelerationLogScaling, b.AccelerationLogScaling, lerp),
                FieldOfViewConstraint = Mathf.Lerp(a.FieldOfViewConstraint, b.FieldOfViewConstraint, lerp)
            };
        }
    }
}