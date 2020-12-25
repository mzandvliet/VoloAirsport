using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Volo {

    /*
     * Todo:
     * - Of all three PID properties we only use P right now, rest is redundant
     * - Move towards more generalized visual editor and testing suite
     */

    public class PilotAnimator : MonoBehaviour {
        [Dependency("gameClock"), SerializeField]
        private AbstractUnityClock _gameClock;
        [Dependency("fixedClock"), SerializeField]
        private AbstractUnityClock _fixedClock;

        [SerializeField] private CharacterParts _characterParts;
        [SerializeField] private CharacterMuscleLimits _muscleLimits;
        [SerializeField] private PlayerPose _defaultPose;
        [SerializeField] private PlayerPose _curledPose;
        [SerializeField] private PlayerPose _parachutePose;

        [SerializeField] private PilotInputConfig _userConfig = PilotInputConfig.Default;
        [SerializeField] private WingsuitStabilizerConfig _wingsuitStabilizerConfig;

        [SerializeField] private FlightStatistics _target;

        private PilotAnimatorState _state = PilotAnimatorState.Wingsuit;
        private CharacterInput _wingsuitInput;
        private WingsuitStabilizer _wingsuitStabilizer;
        private ParachuteInput _parachuteInput;
        private PlayerPose _currentPose;
        private CameraInput _cameraInput;

        public PilotInputConfig UserConfig {
            set {
                _userConfig = value;
                _wingsuitStabilizer.UserConfig = value;
            }
        }

        private void Awake() {
            _currentPose = new PlayerPose();
            _wingsuitStabilizer = new WingsuitStabilizer(_wingsuitStabilizerConfig);
        }

        public void Clear() {
            _wingsuitInput = CharacterInput.Zero;
            _localLookRotation = _state == PilotAnimatorState.Wingsuit
                ? _target.transform.rotation * Quaternion.Euler(-45f, 0f, 0f)
                : _target.transform.rotation;
        }

        public void SetInput(CharacterInput wingsuitInput, ParachuteInput parachuteInput, CameraInput cameraInput) {
            _wingsuitInput = wingsuitInput;
            _parachuteInput = parachuteInput;
            _cameraInput = cameraInput;
        }

        public void SetState(PilotAnimatorState state) {
            _state = state;

            switch (state) {
                case PilotAnimatorState.Wingsuit:
                    SetMuscleTension(1.0f);
                    break;
                case PilotAnimatorState.Parachute:
                    SetMuscleTension(0.33f);
                    break;
            }
        }

        private void SetMuscleTension(float tension) {
            _characterParts.LegLUpper.Tension = tension;
            _characterParts.LegLLower.Tension = tension;
            _characterParts.LegRUpper.Tension = tension;
            _characterParts.LegRLower.Tension = tension;
            _characterParts.ArmLUpper.Tension = tension;
            _characterParts.ArmLLower.Tension = tension;
            _characterParts.ArmRUpper.Tension = tension;
            _characterParts.ArmRLower.Tension = tension;
            _characterParts.Torso.Tension = tension;
        }

        private Quaternion _lastWorldRotation;
        private Quaternion _localLookRotation;

        private void FixedUpdate() {
            if (!_target || !_gameClock || !_fixedClock) {
                return;
            }

            if (_state == PilotAnimatorState.Wingsuit) {
                var input = _wingsuitInput;
                input = _wingsuitStabilizer.SmoothInput(input, _fixedClock.DeltaTime);
                input = _wingsuitStabilizer.ApplyStabilization(input, _target, _fixedClock.DeltaTime);
                AnimateWingsuit(input, _muscleLimits, _characterParts);
            }
            else {
                AnimateParachute(_parachuteInput, _muscleLimits, _characterParts);
            }

            /* 
             * First person Head-Look
             * 
             * Todo:
             * - Working based on up-vectors sucks major donkey dick.
             * - Look input
             */

            Quaternion inputRotation = Quaternion.Euler(0f, _cameraInput.Horizontal * 90f, 0f) *
                                       Quaternion.Euler(_cameraInput.Vertical * 90f, 0f, 0f);

            Vector3 localUp = _state == PilotAnimatorState.Wingsuit
                ? Vector3.back 
                : Vector3.up;

            Vector3 localForward = _state == PilotAnimatorState.Wingsuit
                ? Vector3.up 
                : Vector3.forward;

            Quaternion baseHeadRotation = _state == PilotAnimatorState.Wingsuit
                ? Quaternion.Euler(-45f, 0f, 0f)
                : Quaternion.identity;

            // This really works, makes the first person view feel a lot better
            Vector3 localVelocity = _target.transform.InverseTransformVector(_target.GetInterpolatedTrajectory(3f).Velocity);
            localVelocity = Quaternion.Inverse(baseHeadRotation) * localVelocity;

            // Todo: noise cancelation
            //Quaternion worldRotationDelta = (Quaternion.Inverse(_lastWorldRotation) * _target.transform.rotation);
            //_lastWorldRotation = _target.transform.rotation;
            //Quaternion balancingRotation = Quaternion.Euler(_target.LocalAngularVelocity * Time.deltaTime * -10f)  ;

            Quaternion velocityLookRotation = Quaternion.identity;
            float dot = Vector3.Dot(localVelocity.normalized, baseHeadRotation * Vector3.forward);
            dot = Mathf.Clamp01(dot);
            velocityLookRotation = Quaternion.Slerp(velocityLookRotation, Quaternion.LookRotation(localVelocity, localUp), dot);
            
            baseHeadRotation = baseHeadRotation * velocityLookRotation * inputRotation;

            float interpolationSpeed = 3f + _target.LocalAngularVelocity.magnitude;
            _localLookRotation = Quaternion.Slerp(_localLookRotation, baseHeadRotation, interpolationSpeed * _fixedClock.DeltaTime);
            _characterParts.Head.transform.localRotation = _localLookRotation;
        }

        private void AnimateWingsuit(CharacterInput input, CharacterMuscleLimits limits, CharacterParts parts) {
            PlayerPose.Lerp(ref _currentPose, _defaultPose, _curledPose, input.Cannonball);
            
            float normalizedSpeed = Mathf.Clamp01(_target.RelativeVelocity.magnitude / _wingsuitStabilizerConfig.MaxSpeed);

            input.CloseLeftArm += Mathf.Pow(normalizedSpeed, 1.5f) * 0.25f;
            input.CloseRightArm += Mathf.Pow(normalizedSpeed, 1.5f) * 0.25f;

            float pitchUp = Mathf.Min(0f, input.Pitch);
            float pitchDown = Mathf.Min(0f, -input.Pitch);

            CharacterMuscleOutput o = new CharacterMuscleOutput();

            /* Transform user input into joint angle modifications. (still in unit values) */
            o.ShoulderLPitch = input.Pitch + input.Roll - input.CloseLeftArm;
            o.ShoulderRPitch = input.Pitch - input.Roll - input.CloseRightArm;
            o.ArmLUpperPitch = input.Roll * 0.3f + pitchDown * 0f - input.CloseLeftArm * 0.5f;
            o.ArmRUpperPitch = -input.Roll * 0.3f + pitchDown * 0f - input.CloseRightArm * 0.5f;
            o.ArmLUpperRoll = -input.Pitch;
            o.ArmRUpperRoll = input.Pitch;
            o.ArmLLowerPitch = -input.Pitch * 1f + -input.Roll * 0.5f + input.CloseLeftArm * 0.0f;
            o.ArmRLowerPitch = -input.Pitch * 1f + input.Roll * 0.5f + input.CloseRightArm * 0.0f;
            o.ArmLClose = -input.CloseLeftArm * 1f + pitchDown * 1f + input.Roll * (input.Roll < 0f ? 1f : 0f);
            o.ArmRClose = input.CloseRightArm * 1f - pitchDown * 1f + input.Roll * (input.Roll > 0f ? 1f : 0f);
            o.LegLUpperPitch = pitchUp * 1.0f + Mathf.Pow(pitchDown, 2f) * 0.5f + input.Yaw * 0.25f;
            o.LegRUpperPitch = pitchUp * 1.0f + Mathf.Pow(pitchDown, 2f) * 0.5f - input.Yaw * 0.25f;
            o.LegLUpperRoll = input.Roll;
            o.LegRUpperRoll = input.Roll;
            o.LegLLowerPitch = pitchUp * 1f + Mathf.Pow(pitchDown, 2f) + input.Yaw * 1f;
            o.LegRLowerPitch = pitchUp * 1f + Mathf.Pow(pitchDown, 2f) - input.Yaw * 1f;
            o.TorsoPitch = pitchUp * 0.5f;
            o.TorsoRoll = input.Roll * 1f + input.Yaw * 1f;
            o.TorsoYaw = input.Yaw * 0.5f;

            CharacterMuscleOutput.Clamp(ref o);
            CharacterMuscleOutput.ScaleToLimits(ref o, limits);
            CharacterMuscleOutput.ApplyPose(ref o, _currentPose);

            /* Apply a specific stability effect to just the arms (Todo: move this elsewhere, make configurable) */

            float armUpperPitchHeadingStability = 10f * Mathf.InverseLerp(15f, -5f, _target.AngleOfAttack);
            o.ArmLUpperPitch += armUpperPitchHeadingStability;
            o.ArmRUpperPitch += armUpperPitchHeadingStability;

            float armLowerPitchHeadingStability = 15f * Mathf.InverseLerp(20f, -5f, _target.AngleOfAttack);
            o.ArmLLowerPitch += armLowerPitchHeadingStability;
            o.ArmRLowerPitch += armLowerPitchHeadingStability;

            /* Apply user-configured pitch attitude offset */

            o.TorsoPitch += limits.PitchAttitudeTorsoMax * _userConfig.PitchAttitude;
            o.LegLUpperPitch += limits.PitchAttitudeLegsMax * _userConfig.PitchAttitude;
            o.LegRUpperPitch += limits.PitchAttitudeLegsMax * _userConfig.PitchAttitude;

            /* Send new targets to all the joints */

            CharacterMuscleOutput.Output(ref o, parts);
        }

        private void AnimateParachute(ParachuteInput input, CharacterMuscleLimits limits, CharacterParts parts) {
            CharacterMuscleOutput o = new CharacterMuscleOutput();

            Vector2 lineInput = Max(input.Brakes, input.FrontRisers, input.RearRisers);

            /* Todo: Lower body has to try and cancel out oscillations about harness pivot
             * I suspect that if we can get a rough version of this going it'll be enough
             */

            o.ArmLClose = -lineInput.x * 0.6f;
            o.ArmRClose =  lineInput.y * 0.6f;
            o.LegLUpperPitch = Mathf.Abs(input.WeightShift.y) * 0.66f;
            o.LegRUpperPitch = Mathf.Abs(input.WeightShift.y) * 0.66f;
            o.LegLLowerPitch = input.WeightShift.y;
            o.LegRLowerPitch = input.WeightShift.y;
            o.LegLUpperClose -= input.WeightShift.x;
            o.LegRUpperClose -= input.WeightShift.x;
            o.TorsoPitch = -input.WeightShift.y;
            o.TorsoRoll = input.WeightShift.x * 0.66f;

            CharacterMuscleOutput.Clamp(ref o);
            CharacterMuscleOutput.ScaleToLimits(ref o, limits);
            CharacterMuscleOutput.ApplyPose(ref o, _parachutePose);
            CharacterMuscleOutput.Output(ref o, parts);
        }

        // Todo: this is a bit silly
        private static Vector2 Max(Vector2 a, Vector2 b, Vector2 c) {
            Vector2 max = a;
            if (b.sqrMagnitude > max.sqrMagnitude) {
                max = b;
            }
            if (c.sqrMagnitude > max.sqrMagnitude) {
                max = c;
            }
            return max;
        }
    }

    public enum PilotAnimatorState {
        Wingsuit,
        Parachute
    }

    [Serializable]
    public class WingsuitStabilizerConfig {
        /* Stabilization */

        [SerializeField] public bool ApplyStabilization = true;
        [SerializeField] public float MaxSpeed = 75f;

        [SerializeField] public float MaxStabilityOverride = 0.7f;
        [SerializeField] public float ErrorSmoothingSpeed = 20f;
        [SerializeField] public float StabilityCorrectionSpeed = 20f;
        [SerializeField] public float InputSmoothingSpeed = 5f;

        [SerializeField] public PIDConfig PitchPIDConfig = new PIDConfig(-0.2f, 0f, 0f);
        [SerializeField] public PIDConfig RollPIDConfig = new PIDConfig(-0.2f, 0f, 0f);
        [SerializeField] public PIDConfig YawPIDConfig = new PIDConfig(0.2f, 0f, 0f);
    }

    public class WingsuitStabilizer {
        private WingsuitStabilizerConfig _c;

        private CharacterInput _smoothPlayerInput;
        private CharacterInput _lastGyroCorrection;
        private Vector3 _lastGyroError;
        private readonly PIDController _pitchPid;
        private readonly PIDController _rollPid;
        private readonly PIDController _yawPid;

        [SerializeField]
        private PilotInputConfig _userConfig = new PilotInputConfig(
            inputSpeedScaling: 0.95f,
            stallLimiterStrength: 0.5f,
            rollLimiterStrength: 0.5f,
            pitchAttitude: 0.5f);

        public WingsuitStabilizer(WingsuitStabilizerConfig c) {
            _pitchPid = new PIDController(c.PitchPIDConfig);
            _rollPid = new PIDController(c.RollPIDConfig);
            _yawPid = new PIDController(c.YawPIDConfig);
            _c = c;
        }

        public void Clear() {
            _pitchPid.Reset();
            _rollPid.Reset();
            _yawPid.Reset();
        }

        public PilotInputConfig UserConfig {
            get { return _userConfig; }
            set { _userConfig = value; }
        }

        public CharacterInput SmoothInput(CharacterInput input, float deltaTime) {
            _smoothPlayerInput = CharacterInput.Lerp(_smoothPlayerInput, input, _c.InputSmoothingSpeed * deltaTime);
            return _smoothPlayerInput;
        }

        public CharacterInput ApplyStabilization(CharacterInput input, FlightStatistics target, float deltaTime) {
            input = ApplyStallPrevention(input, target, _userConfig.StallLimiterStrength, _userConfig.RollLimiterStrength);

            var stabilityInput = CharacterInput.Zero;

            // Update gyroscopic stability
            var error = GetGyroError(deltaTime, target, _lastGyroError, _c.ErrorSmoothingSpeed);
            _lastGyroError = error;
            var pids = GetGyroPidOutputs(error);
            var gyroCorrection = PidOutputToCharacterInput(pids);
            gyroCorrection = CharacterInput.Lerp(_lastGyroCorrection, gyroCorrection, _c.StabilityCorrectionSpeed * deltaTime);
            _lastGyroCorrection = gyroCorrection;
            stabilityInput += gyroCorrection;

            // Apply stability overrides (Player input cancels out stabilization effects)
            var stabilityOverride = GetStabilityOverrides(input, _c.MaxStabilityOverride);
            stabilityInput = ApplyStabilityOverride(stabilityInput, stabilityOverride);

            input += stabilityInput;

            // Apply speed scaling to summed inputs
            float normalizedSpeed = Mathf.Clamp01(target.RelativeVelocity.magnitude / _c.MaxSpeed);

            return ApplySpeedScaling(input, normalizedSpeed, _userConfig.InputSpeedScaling, 1.5f);
        }

        private static CharacterInput ApplyStallPrevention(CharacterInput input, FlightStatistics target, float stallLimitStrength, float rollLimitStrength) {
            // Disable stall prevention if super-maneuvre inputs are active
            stallLimitStrength *= 1f - input.Cannonball;
            rollLimitStrength *= 1f - input.Cannonball;
            rollLimitStrength *= 1f - Mathf.Max(input.CloseLeftArm, input.CloseRightArm);

            // Limit both positive and negative pitch motion to keep body from stalling
            if (input.Pitch > 0f) {
                float multiplier = 1f - Mathf.InverseLerp(0f, -20f, target.AngleOfAttack) * stallLimitStrength;
                input.Pitch *= multiplier;
            }
            else {
                float multiplier = 1f - Mathf.InverseLerp(0f, 20f, target.AngleOfAttack) * stallLimitStrength;
                input.Pitch *= multiplier;
            }

            // Limit roll rate
            if (input.Roll > 0f) {
                float multiplier = 1f - Mathf.Clamp01((-target.LocalAngularVelocity.y - 1f) * 1f) * rollLimitStrength;
                input.Roll *= multiplier;
            }
            else {
                float multiplier = 1f - Mathf.Clamp01((target.LocalAngularVelocity.y - 1f) * 1f) * rollLimitStrength;
                input.Roll *= multiplier;
            }

            return input;
        }

        private static CharacterInput ApplySpeedScaling(CharacterInput input, float normalizedAirspeed, float speedScaling, float speedScalingPower) {
            float scale = Mathf.Max(0.2f, 1f - Mathf.Pow(normalizedAirspeed, speedScalingPower) * speedScaling);

            return new CharacterInput {
                Pitch = input.Pitch * scale,
                Roll = input.Roll * scale,
                Yaw = input.Yaw * scale,
                Cannonball = input.Cannonball,
                CloseLeftArm = input.CloseLeftArm,
                CloseRightArm = input.CloseRightArm
            };
        }



        private static CharacterInput PidOutputToCharacterInput(Vector3 pidOutputs) {
            return new CharacterInput {
                Pitch = Mathf.Clamp(pidOutputs.x, -1f, 1f),
                Roll = Mathf.Clamp(pidOutputs.y, -1f, 1f),
                Yaw = Mathf.Clamp(pidOutputs.z, -1f, 1f)
            };
        }

        private static StabilityOverride GetStabilityOverrides(CharacterInput playerInput, float maxOverrideFactor) {
            Func<float, float, float> overrideFactor = (input, maxOverride) => 1f - Mathf.Abs(input) * maxOverride; //Todo: Cache this func?

            return new StabilityOverride {
                Pitch = overrideFactor(playerInput.Pitch, maxOverrideFactor),
                Roll = overrideFactor(playerInput.Roll, maxOverrideFactor),
                Yaw = overrideFactor(playerInput.Yaw, maxOverrideFactor),
            };
        }

        private static CharacterInput ApplyStabilityOverride(CharacterInput input, StabilityOverride stabilityOverride) {
            input.Pitch = input.Pitch * stabilityOverride.Pitch;
            input.Roll = input.Roll * stabilityOverride.Roll;
            input.Yaw = input.Yaw * stabilityOverride.Yaw;
            return input;
        }

        private static Vector3 GetGyroError(float deltaTime, FlightStatistics target, Vector3 lastError, float smoothingSpeed) {
            Vector3 error = target.LocalAngularVelocity + target.LocalAngularAcceleration * 0.5f;
            error = Quaternion.Euler(-90f, 0f, 0f) * error;
            return Vector3.Lerp(lastError, error, deltaTime * smoothingSpeed);
        }

        private Vector3 GetGyroPidOutputs(Vector3 error) {
            return new Vector3(
                _pitchPid.Update(error.x).Sum(),
                _rollPid.Update(error.z).Sum(),
                _yawPid.Update(error.y).Sum()
            );
        }

        private struct StabilityOverride { // Todo: This struct parallels a subset of CharacterInput, might be redundant?
            public float Pitch;
            public float Roll;
            public float Yaw;
        }
    }

    public struct CharacterInput {
        public float Pitch;
        public float Roll;
        public float Yaw;
        public float Cannonball;
        public float CloseLeftArm;
        public float CloseRightArm;

        public static CharacterInput operator +(CharacterInput c1, CharacterInput c2) {
            return new CharacterInput {
                Pitch = c1.Pitch + c2.Pitch,
                Roll = c1.Roll + c2.Roll,
                Yaw = c1.Yaw + c2.Yaw,
                Cannonball = c1.Cannonball + c2.Cannonball,
                CloseLeftArm = c1.CloseLeftArm + c2.CloseLeftArm,
                CloseRightArm = c1.CloseRightArm + c2.CloseRightArm
            };
        }

        public static CharacterInput Lerp(CharacterInput c1, CharacterInput c2, float lerp) {
            return new CharacterInput() {
                Pitch = Mathf.Lerp(c1.Pitch, c2.Pitch, lerp),
                Roll = Mathf.Lerp(c1.Roll, c2.Roll, lerp),
                Yaw = Mathf.Lerp(c1.Yaw, c2.Yaw, lerp),
                Cannonball = Mathf.Lerp(c1.Cannonball, c2.Cannonball, lerp),
                CloseLeftArm = Mathf.Lerp(c1.CloseLeftArm, c2.CloseLeftArm, lerp),
                CloseRightArm = Mathf.Lerp(c1.CloseRightArm, c2.CloseRightArm, lerp),
            };
        }

        public CharacterInput Merge(CharacterInput c) {
            return new CharacterInput {
                Pitch = Adapters.MergeAxes(Pitch, c.Pitch),
                Roll = Adapters.MergeAxes(Roll, c.Roll),
                Yaw = Adapters.MergeAxes(Yaw, c.Yaw),
                Cannonball = Adapters.MergeAxes(Cannonball, c.Cannonball),
                CloseLeftArm = Adapters.MergeAxes(CloseLeftArm, c.CloseLeftArm),
                CloseRightArm = Adapters.MergeAxes(CloseRightArm, c.CloseRightArm),
            };
        }

        public static readonly CharacterInput Zero = new CharacterInput();
    }

    [Serializable]
    public class PlayerPose {
        [SerializeField]
        public float LegUpperPitch = -4f;
        [SerializeField]
        public float LegUpperRoll = -24f;
        [SerializeField]
        public float LegUpperSpread = 42f;
        [SerializeField]
        public float LegLowerPitch = -5f;
        [SerializeField]
        public float ArmUpperPitch = -25f;
        [SerializeField]
        public float ArmUpperRoll = 10f;
        [SerializeField]
        public float ArmClose = 38f;
        [SerializeField]
        public float ArmLowerPitch = 8f;
        [SerializeField]
        public float ShoulderPitch = 0.008f;
        [SerializeField]
        public float TorsoPitch = -8f;

        public static void Lerp(ref PlayerPose result, PlayerPose lhs, PlayerPose rhs, float lerp) {
            result.LegUpperPitch = Mathf.Lerp(lhs.LegUpperPitch, rhs.LegUpperPitch, lerp);
            result.LegUpperRoll = Mathf.Lerp(lhs.LegUpperRoll, rhs.LegUpperRoll, lerp);
            result.LegUpperSpread = Mathf.Lerp(lhs.LegUpperSpread, rhs.LegUpperSpread, lerp);
            result.LegLowerPitch = Mathf.Lerp(lhs.LegLowerPitch, rhs.LegLowerPitch, lerp);
            result.ArmUpperPitch = Mathf.Lerp(lhs.ArmUpperPitch, rhs.ArmUpperPitch, lerp);
            result.ArmUpperRoll = Mathf.Lerp(lhs.ArmUpperRoll, rhs.ArmUpperRoll, lerp);
            result.ArmClose = Mathf.Lerp(lhs.ArmClose, rhs.ArmClose, lerp);
            result.ArmLowerPitch = Mathf.Lerp(lhs.ArmLowerPitch, rhs.ArmLowerPitch, lerp);
            result.ShoulderPitch = Mathf.Lerp(lhs.ShoulderPitch, rhs.ShoulderPitch, lerp);
            result.TorsoPitch = Mathf.Lerp(lhs.TorsoPitch, rhs.TorsoPitch, lerp);
        }
    }

    public struct CharacterMuscleOutput {
        public float ShoulderLPitch;
        public float ShoulderRPitch;
        public float ArmLUpperPitch;
        public float ArmRUpperPitch;
        public float ArmLUpperRoll;
        public float ArmRUpperRoll;
        public float ArmLLowerPitch;
        public float ArmRLowerPitch;
        public float ArmLClose;
        public float ArmRClose;
        public float LegLUpperPitch;
        public float LegRUpperPitch;
        public float LegLUpperRoll;
        public float LegRUpperRoll;
        public float LegLUpperClose;
        public float LegRUpperClose;
        public float LegLLowerPitch;
        public float LegRLowerPitch;
        public float TorsoPitch;
        public float TorsoRoll;
        public float TorsoYaw;

        public static void Clamp(ref CharacterMuscleOutput o) {
            o.ShoulderLPitch = Mathf.Clamp(o.ShoulderLPitch, -1f, 1f);
            o.ShoulderRPitch = Mathf.Clamp(o.ShoulderRPitch, -1f, 1f);
            o.ArmLUpperPitch = Mathf.Clamp(o.ArmLUpperPitch, -1f, 1f);
            o.ArmRUpperPitch = Mathf.Clamp(o.ArmRUpperPitch, -1f, 1f);
            o.ArmLUpperRoll = Mathf.Clamp(o.ArmLUpperRoll, -1f, 1f);
            o.ArmRUpperRoll = Mathf.Clamp(o.ArmRUpperRoll, -1f, 1f);
            o.ArmLClose = Mathf.Clamp(o.ArmLClose, -1f, 1f);
            o.ArmRClose = Mathf.Clamp(o.ArmRClose, -1f, 1f);
            o.LegLUpperPitch = Mathf.Clamp(o.LegLUpperPitch, -1f, 1f);
            o.LegRUpperPitch = Mathf.Clamp(o.LegRUpperPitch, -1f, 1f);
            o.LegLUpperRoll = Mathf.Clamp(o.LegLUpperRoll, -1f, 1f);
            o.LegRUpperRoll = Mathf.Clamp(o.LegRUpperRoll, -1f, 1f);
            o.LegLUpperClose = Mathf.Clamp(o.LegLUpperClose, -1f, 1f);
            o.LegRUpperClose = Mathf.Clamp(o.LegRUpperClose, -1f, 1f);
            o.TorsoPitch = Mathf.Clamp(o.TorsoPitch, -1f, 1f);
            o.TorsoRoll = Mathf.Clamp(o.TorsoRoll, -1f, 1f);
            o.TorsoYaw = Mathf.Clamp(o.TorsoYaw, -1f, 1f);
        }

        public static void ScaleToLimits(ref CharacterMuscleOutput o, CharacterMuscleLimits l) {
            // linear motion, meters
            o.ShoulderLPitch *= l.ShoulderPitch;
            o.ShoulderRPitch *= l.ShoulderPitch;

            // angular motion, degrees
            o.ArmLUpperPitch *= l.ArmUpperPitch;
            o.ArmRUpperPitch *= l.ArmUpperPitch;
            o.ArmLUpperRoll *= l.ArmUpperRoll;
            o.ArmRUpperRoll *= l.ArmUpperRoll;
            o.ArmLLowerPitch *= l.ArmLowerPitch;
            o.ArmRLowerPitch *= l.ArmLowerPitch;
            o.ArmLClose *= l.ArmClose;
            o.ArmRClose *= l.ArmClose;
            o.LegLUpperPitch *= l.LegUpperPitch;
            o.LegRUpperPitch *= l.LegUpperPitch;
            o.LegLUpperRoll *= l.LegUpperRoll;
            o.LegRUpperRoll *= l.LegUpperRoll;
            o.LegLUpperClose *= l.LegUpperClose;
            o.LegRUpperClose *= l.LegUpperClose;
            o.LegLLowerPitch *= l.LegLowerPitch;
            o.LegRLowerPitch *= l.LegLowerPitch;
            o.TorsoPitch *= l.TorsoPitch;
            o.TorsoRoll *= l.TorsoRoll;
            o.TorsoYaw *= l.TorsoYaw;
        }

        public static void ApplyPose(ref CharacterMuscleOutput o, PlayerPose p) {
            o.ShoulderLPitch += p.ShoulderPitch;
            o.ShoulderRPitch += p.ShoulderPitch;

            o.ArmLUpperPitch += p.ArmUpperPitch;
            o.ArmRUpperPitch += p.ArmUpperPitch;
            o.ArmLUpperRoll -= p.ArmUpperRoll;
            o.ArmRUpperRoll += p.ArmUpperRoll;
            o.ArmLLowerPitch += p.ArmLowerPitch;
            o.ArmRLowerPitch += p.ArmLowerPitch;
            o.ArmLClose -= p.ArmClose;
            o.ArmRClose += p.ArmClose;
            o.LegLLowerPitch += p.LegLowerPitch;
            o.LegRLowerPitch += p.LegLowerPitch;
            o.LegLUpperPitch += p.LegUpperPitch;
            o.LegRUpperPitch += p.LegUpperPitch;
            o.LegLUpperRoll -= p.LegUpperRoll;
            o.LegRUpperRoll += p.LegUpperRoll;
            o.LegLUpperClose += p.LegUpperSpread;
            o.LegRUpperClose -= p.LegUpperSpread;
            o.TorsoPitch += p.TorsoPitch;
        }

        public static void Output(ref CharacterMuscleOutput o, CharacterParts p) {
            p.LegLUpper.TargetRotation = Quaternion.Euler(o.LegLUpperPitch, o.LegLUpperRoll, o.LegLUpperClose);
            p.LegRUpper.TargetRotation = Quaternion.Euler(o.LegRUpperPitch, o.LegRUpperRoll, o.LegRUpperClose);
            p.LegLLower.TargetRotation = Quaternion.Euler(o.LegLLowerPitch, 0f, 0f);
            p.LegRLower.TargetRotation = Quaternion.Euler(o.LegRLowerPitch, 0f, 0f);
            p.ArmLUpper.TargetRotation = Quaternion.Euler(o.ArmLUpperPitch, o.ArmLUpperRoll, o.ArmLClose);
            p.ArmRUpper.TargetRotation = Quaternion.Euler(o.ArmRUpperPitch, o.ArmRUpperRoll, o.ArmRClose);
            p.ArmLLower.TargetRotation = Quaternion.Euler(o.ArmLLowerPitch, 0f, 0f);
            p.ArmRLower.TargetRotation = Quaternion.Euler(o.ArmRLowerPitch, 0f, 0f);
            p.Torso.TargetRotation = Quaternion.Euler(o.TorsoPitch, o.TorsoRoll, o.TorsoYaw);
            p.ArmLUpper.TargetPosition = new Vector3(0f, 0f, o.ShoulderLPitch);
            p.ArmRUpper.TargetPosition = new Vector3(0f, 0f, o.ShoulderRPitch);
        }
    }

    [Serializable]
    public class CharacterMuscleLimits {
        // Linear motion (in meters)
        [SerializeField]
        public float ShoulderPitch = 0.05f;

        // Angular motion (in degrees)
        [SerializeField]
        public float ArmUpperPitch = 50f;
        [SerializeField]
        public float ArmUpperRoll = 30f;
        [SerializeField]
        public float ArmClose = 90f;
        [SerializeField]
        public float ArmLowerPitch = 60f;
        [SerializeField]
        public float LegUpperPitch = 90f;
        [SerializeField]
        public float LegUpperRoll = 30f;
        [SerializeField]
        public float LegLowerPitch = 90f;
        [SerializeField]
        public float LegUpperClose = 42f;
        [SerializeField]
        public float TorsoPitch = 40f;
        [SerializeField]
        public float TorsoRoll = 50f;
        [SerializeField]
        public float TorsoYaw = 40f;

        [SerializeField]
        public float PitchAttitudeTorsoMax = -1.5f;
        [SerializeField]
        public float PitchAttitudeLegsMax = 3f;
    }

    [Serializable]
    public class CharacterParts {
        [SerializeField]
        public JointController LegLUpper;
        [SerializeField]
        public JointController LegLLower;
        [SerializeField]
        public JointController LegRUpper;
        [SerializeField]
        public JointController LegRLower;
        [SerializeField]
        public JointController ArmLUpper;
        [SerializeField]
        public JointController ArmLLower;
        [SerializeField]
        public JointController ArmRUpper;
        [SerializeField]
        public JointController ArmRLower;
        [SerializeField]
        public JointController Torso;
        [SerializeField]
        public Transform Neck;
        [SerializeField]
        public Transform Head;
    }
}
