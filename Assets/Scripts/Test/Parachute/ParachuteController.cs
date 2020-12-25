using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using RamjetAnvil.Volo.Input;
using UnityExecutionOrder;

namespace RamjetAnvil.Volo {

    [Run.Before(typeof(Parachute))]
    public class ParachuteController : MonoBehaviour {
        [SerializeField, Dependency("gameClock")] private AbstractUnityClock _gameClock;
        [SerializeField] private Parachute _parachute;

        [SerializeField] private MouseSmoothingConfig _mouseSmoothingConfig = new MouseSmoothingConfig(
            gravity: 5f,
            mouseSensitivity: 0.3f,
            gravityThreshold: 0.5f,
            scalingPower: 1.5f);
        [SerializeField] private float _mouseInputSensitivityMultiplier = 3f;
        [SerializeField] private ParachuteInputConfig _inputConfig;
        private IReadonlyRef<IParachuteActionMap> _actionMap;

        private int _renderFrame;
        private ParachuteInput _input;

        public Parachute Parachute {
            get { return _parachute; }
            set { _parachute = value; }
        }

        public IReadonlyRef<IParachuteActionMap> ActionMap {
            get { return _actionMap; }
            set { _actionMap = value; }
        }

        public ParachuteInput Input {
            get { return _input; }
        }

        void Awake() {
            _renderFrame = -1;
        }

        void FixedUpdate() {
            if (_parachute != null && Time.renderedFrameCount != _renderFrame) {
                var rawInput = _actionMap.V.Input;
                ParachuteInput smoothInput;
                if (rawInput.IsMouseInput) {
                    smoothInput = SmoothMouseInput(_input, rawInput);
                } else {
                    smoothInput = SmoothInput(_input, rawInput);
                }
                _input = smoothInput;

                ApplyInput(_input, _parachute);

                _renderFrame = Time.renderedFrameCount;
            }
        }

        private ParachuteInput SmoothMouseInput(ParachuteInput prevInput, ParachuteInput input) {
            input.Brakes = SmoothMouseLineInput(_mouseSmoothingConfig, prevInput.Brakes, input.Brakes, _gameClock.DeltaTime);
            input.FrontRisers = SmoothMouseLineInput(_mouseSmoothingConfig, prevInput.FrontRisers, input.FrontRisers, _gameClock.DeltaTime);
            input.RearRisers = SmoothMouseLineInput(_mouseSmoothingConfig, prevInput.RearRisers, input.RearRisers, _gameClock.DeltaTime);
            return input;
        }

        private ParachuteInput SmoothInput(ParachuteInput prevInput, ParachuteInput input) {
            // Handle dual analog gamepad input and convert to parachute domain input (Todo: in external module)
            ScaleInputQuadratically(ref input, 1.5f);

            return ParachuteInput.SmoothInput(_inputConfig, prevInput, input, _gameClock.DeltaTime);
        }

        private void ScaleInput(ref ParachuteInput input, float scaleFactor) {
            input.Brakes.x *= scaleFactor;
            input.Brakes.y *= scaleFactor;
            input.FrontRisers.x *= scaleFactor;
            input.FrontRisers.y *= scaleFactor;
            input.RearRisers.x *= scaleFactor;
            input.RearRisers.y *= scaleFactor;
        }

        private void ScaleInputQuadratically(ref ParachuteInput input, float scaleFactor) {
            input.Brakes.x = ScaleQuadratic(input.Brakes.x, scaleFactor);
            input.Brakes.y = ScaleQuadratic(input.Brakes.y, scaleFactor);
            input.FrontRisers.x = ScaleQuadratic(input.FrontRisers.x, scaleFactor);
            input.FrontRisers.y = ScaleQuadratic(input.FrontRisers.y, scaleFactor);
            input.RearRisers.x = ScaleQuadratic(input.RearRisers.x, scaleFactor);
            input.RearRisers.y = ScaleQuadratic(input.RearRisers.y, scaleFactor);
        }

        private void ApplyInput(ParachuteInput input, Parachute parachute) {
            // Toggle input

            // Todo: this should be wrapped into the parachute interface
            for (int i = 0; i < parachute.LeftToggleSections.Count; i++) {
                parachute.LeftToggleSections[i].BrakeLine.ApplyPull(input.Brakes.x);
            }

            for (int i = 0; i < parachute.RightToggleSections.Count; i++) {
                parachute.RightToggleSections[i].BrakeLine.ApplyPull(input.Brakes.y);
            }

            // Risers input

            for (int i = 0; i < parachute.LeftRiserSections.Count; i++) {
                parachute.LeftRiserSections[i].RearLine.ApplyPull(input.RearRisers.x);
                parachute.LeftRiserSections[i].FrontLine.ApplyPull(input.FrontRisers.x);
            }


            for (int i = 0; i < parachute.RightRiserSections.Count; i++) {
                parachute.RightRiserSections[i].RearLine.ApplyPull(input.RearRisers.y);
                parachute.RightRiserSections[i].FrontLine.ApplyPull(input.FrontRisers.y);
            }

            // Weight shift input (Todo: through parachutepilot interface)

            // TODO Re-implement weight-shift
            //pilot.Body.centerOfMass = new Vector3(input.WeightShift.x, 0f, input.WeightShift.y) * config.WeightshiftMagnitude;
        }

        private static float ScaleQuadratic(float value, float pow) {
            return Mathf.Sign(value) * Mathf.Pow(value, pow);
        }

        private Vector2 SmoothMouseLineInput(MouseSmoothingConfig config, Vector2 prevInput, Vector2 input, float deltaTime) {
            var adjustedInput = input;

            // Left lines
            adjustedInput.x = adjustedInput.x * config.MouseSensitivity * deltaTime;
            float scaleX = Mathf.Pow(1f + adjustedInput.x, config.ScalingPower) - 1f;
            adjustedInput.x *= scaleX;
            adjustedInput.x += prevInput.x;

            float cancelPowerLeft;
            if (input.x > config.GravityThreshold) {
                cancelPowerLeft = 0f;
            } else {
                cancelPowerLeft = _mouseSmoothingConfig.Gravity * deltaTime;
            }
            adjustedInput.x = Mathf.Lerp(adjustedInput.x, 0, cancelPowerLeft);

            // Right lines
            adjustedInput.y = adjustedInput.y * config.MouseSensitivity * deltaTime;
            float scaleY = Mathf.Pow(1f + adjustedInput.y, config.ScalingPower) - 1f;
            adjustedInput.y *= scaleY;
            adjustedInput.y += prevInput.y;

            float cancelPowerRight;
            if (input.y > config.GravityThreshold) {
                cancelPowerRight = 0f;
            } else {
                cancelPowerRight = _mouseSmoothingConfig.Gravity * deltaTime;
            }
            // TODO Only cancel when no input is given
            adjustedInput.y = Mathf.Lerp(adjustedInput.y, 0, cancelPowerRight);

            return new Vector2(
                x: Mathf.Clamp(adjustedInput.x, 0f, 1f),
                y: Mathf.Clamp(adjustedInput.y, 0f, 1f));
        }

        [Serializable]
        public struct MouseSmoothingConfig {
            public float Gravity;
            public float GravityThreshold;
            public float MouseSensitivity;
            public float ScalingPower;

            public MouseSmoothingConfig(float gravity, float gravityThreshold, float mouseSensitivity, float scalingPower) {
                Gravity = gravity;
                GravityThreshold = gravityThreshold;
                MouseSensitivity = mouseSensitivity;
                ScalingPower = scalingPower;
            }
        }
    }
}

/* Controls two elements of the total parachute system: the parachute and its pilot */