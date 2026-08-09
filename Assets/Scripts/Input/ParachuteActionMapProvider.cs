using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RamjetAnvil.Volo.Input {

    [System.Serializable]
    public struct ParachuteInput {
        public Vector2 Brakes;
        public Vector2 FrontRisers;
        public Vector2 RearRisers;
        public Vector2 WeightShift;
        public bool IsMouseInput;
        public ParachuteLine? SelectedLine;
        public Vector2 SelectedLinePull;

        public static readonly ParachuteInput Zero = new ParachuteInput();

        public static ParachuteInput SmoothInput(ParachuteInputConfig config, ParachuteInput prev, ParachuteInput input, float deltaTime) {
            return new ParachuteInput {
                Brakes = Vector2.Lerp(prev.Brakes, input.Brakes, config.BrakeSmoothingSpeed * deltaTime),
                FrontRisers = Vector2.Lerp(prev.FrontRisers, input.FrontRisers, config.FrontRisersSmoothingSpeed * deltaTime),
                RearRisers = Vector2.Lerp(prev.RearRisers, input.RearRisers, config.RearRisersSmoothingSpeed * deltaTime),
                WeightShift = Vector2.Lerp(prev.WeightShift, input.WeightShift, config.WeightShiftSmoothingSpeed * deltaTime),
                IsMouseInput = input.IsMouseInput,
                SelectedLine = input.SelectedLine,
                SelectedLinePull = Vector2.Lerp(prev.SelectedLinePull, input.SelectedLinePull, config.BrakeSmoothingSpeed * deltaTime)
            };
        }
    }

    // Todo: placeholder bindings, needs real tuning once parachute flight is retuned.
    // Hold-line mode (select a line pair with PullLeftLines/PullRightLines/PullBothLines,
    // then pull it) is not wired up yet - only direct-line mode (Front/Rear/Brake pairs
    // bound straight to gamepad triggers) is implemented.
    public class ParachuteActionMap : ActionMap<ParachuteAction>, IParachuteActionMap {
        public ParachuteActionMap() : base(new InputActionMap("Parachute")) {
            SetupBindings();
            EnableActions();
        }

        private void SetupBindings() {
            var shiftLeft = AddAxis(ParachuteAction.WeightShiftLeft, "WeightShiftLeft");
            shiftLeft.AddBinding("<Gamepad>/leftStick/left");
            shiftLeft.AddBinding("<Keyboard>/a");

            var shiftRight = AddAxis(ParachuteAction.WeightShiftRight, "WeightShiftRight");
            shiftRight.AddBinding("<Gamepad>/leftStick/right");
            shiftRight.AddBinding("<Keyboard>/d");

            var shiftFront = AddAxis(ParachuteAction.WeightShiftFront, "WeightShiftFront");
            shiftFront.AddBinding("<Gamepad>/leftStick/up");
            shiftFront.AddBinding("<Keyboard>/w");

            var shiftBack = AddAxis(ParachuteAction.WeightShiftBack, "WeightShiftBack");
            shiftBack.AddBinding("<Gamepad>/leftStick/down");
            shiftBack.AddBinding("<Keyboard>/s");

            var brakeLeft = AddAxis(ParachuteAction.PullBrakeLineLeft, "PullBrakeLineLeft");
            brakeLeft.AddBinding("<Gamepad>/leftTrigger");
            brakeLeft.AddBinding("<Keyboard>/q");

            var brakeRight = AddAxis(ParachuteAction.PullBrakeLineRight, "PullBrakeLineRight");
            brakeRight.AddBinding("<Gamepad>/rightTrigger");
            brakeRight.AddBinding("<Keyboard>/e");

            var frontLeft = AddAxis(ParachuteAction.PullFrontLineLeft, "PullFrontLineLeft");
            var frontRight = AddAxis(ParachuteAction.PullFrontLineRight, "PullFrontLineRight");
            var rearLeft = AddAxis(ParachuteAction.PullRearLineLeft, "PullRearLineLeft");
            var rearRight = AddAxis(ParachuteAction.PullRearLineRight, "PullRearLineRight");

            var configToggle = AddButton(ParachuteAction.ParachuteConfigToggle, "ParachuteConfigToggle");
            configToggle.AddBinding("<Keyboard>/tab");
            configToggle.AddBinding("<Gamepad>/select");
        }

        public ParachuteInput Input {
            get {
                return new ParachuteInput {
                    Brakes = new Vector2(PollAxis(ParachuteAction.PullBrakeLineLeft), PollAxis(ParachuteAction.PullBrakeLineRight)),
                    FrontRisers = new Vector2(PollAxis(ParachuteAction.PullFrontLineLeft), PollAxis(ParachuteAction.PullFrontLineRight)),
                    RearRisers = new Vector2(PollAxis(ParachuteAction.PullRearLineLeft), PollAxis(ParachuteAction.PullRearLineRight)),
                    WeightShift = new Vector2(
                        PollAxis(ParachuteAction.WeightShiftRight) - PollAxis(ParachuteAction.WeightShiftLeft),
                        PollAxis(ParachuteAction.WeightShiftFront) - PollAxis(ParachuteAction.WeightShiftBack)),
                    IsMouseInput = false,
                    SelectedLine = null,
                    SelectedLinePull = Vector2.zero
                };
            }
        }

        public ButtonEvent ParachuteConfigToggle {
            get { return PollButtonEvent(ParachuteAction.ParachuteConfigToggle); }
        }
    }

    public class ParachuteActionMapProvider : MonoBehaviour, IReadonlyRef<ParachuteActionMap> {
        private ParachuteActionMap _actionMap;

        private void Awake() {
            _actionMap = new ParachuteActionMap();
        }

        public ParachuteActionMap V {
            get { return _actionMap; }
        }

        // Todo: not wired up yet - see PilotActionMapProvider.SetInputMappingSource.
        public void SetInputMappingSource(System.IObservable<ActionMapConfig<ParachuteAction>> mappingChanges) {
        }

        private void OnDestroy() {
            if (_actionMap != null) {
                _actionMap.Dispose();
            }
        }
    }
}
