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

            // Gamepad only has two continuous inputs (triggers) to spend on six line-pull
            // channels (front/rear/brake x left/right). Matching the original v3.7 Steam
            // build exactly: the triggers are reused for whichever line pair is currently
            // "held" - brakes by default, front risers while West/X is held, rear risers
            // while South/A is held. A pilot only has two hands; the game never lets you
            // pull front-left and rear-right simultaneously, same as the reference build.
            // PullLeftLines/PullRightLines here are only the raw trigger source feeding that
            // redirect (see the Input getter below) - never read directly by ApplyInput.
            var rawLeftTrigger = AddAxis(ParachuteAction.PullLeftLines, "RawLeftTrigger");
            rawLeftTrigger.AddBinding("<Gamepad>/leftTrigger");

            var rawRightTrigger = AddAxis(ParachuteAction.PullRightLines, "RawRightTrigger");
            rawRightTrigger.AddBinding("<Gamepad>/rightTrigger");

            var holdFrontLines = AddButton(ParachuteAction.HoldFrontLines, "HoldFrontLines");
            holdFrontLines.AddBinding("<Gamepad>/buttonWest");

            var holdRearLines = AddButton(ParachuteAction.HoldRearLines, "HoldRearLines");
            holdRearLines.AddBinding("<Gamepad>/buttonSouth");

            // Keyboard has plenty of distinct keys, so each line pair gets its own dedicated
            // key rather than needing the gamepad's hold-to-redirect trick.
            var brakeLeft = AddAxis(ParachuteAction.PullBrakeLineLeft, "PullBrakeLineLeft");
            brakeLeft.AddBinding("<Keyboard>/a");

            var brakeRight = AddAxis(ParachuteAction.PullBrakeLineRight, "PullBrakeLineRight");
            brakeRight.AddBinding("<Keyboard>/d");

            var frontLeft = AddAxis(ParachuteAction.PullFrontLineLeft, "PullFrontLineLeft");
            frontLeft.AddBinding("<Keyboard>/q");

            var frontRight = AddAxis(ParachuteAction.PullFrontLineRight, "PullFrontLineRight");
            frontRight.AddBinding("<Keyboard>/e");

            var rearLeft = AddAxis(ParachuteAction.PullRearLineLeft, "PullRearLineLeft");
            rearLeft.AddBinding("<Keyboard>/z");

            var rearRight = AddAxis(ParachuteAction.PullRearLineRight, "PullRearLineRight");
            rearRight.AddBinding("<Keyboard>/c");

            var configToggle = AddButton(ParachuteAction.ParachuteConfigToggle, "ParachuteConfigToggle");
            configToggle.AddBinding("<Keyboard>/tab");
            configToggle.AddBinding("<Gamepad>/select");
        }

        public ParachuteInput Input {
            get {
                // Gate the raw gamepad trigger value to whichever line pair is currently
                // "held" (see SetupBindings) - mutually exclusive, matching the reference
                // build's two-hands-only design. Additively combined with keyboard's own
                // directly-bound per-line keys, which don't need gating.
                var rawLeftTrigger = PollAxis(ParachuteAction.PullLeftLines);
                var rawRightTrigger = PollAxis(ParachuteAction.PullRightLines);
                var holdFront = PollButton(ParachuteAction.HoldFrontLines) == ButtonState.Pressed;
                var holdRear = PollButton(ParachuteAction.HoldRearLines) == ButtonState.Pressed;
                var holdNeither = !holdFront && !holdRear;

                return new ParachuteInput {
                    Brakes = new Vector2(
                        PollAxis(ParachuteAction.PullBrakeLineLeft) + (holdNeither ? rawLeftTrigger : 0f),
                        PollAxis(ParachuteAction.PullBrakeLineRight) + (holdNeither ? rawRightTrigger : 0f)),
                    FrontRisers = new Vector2(
                        PollAxis(ParachuteAction.PullFrontLineLeft) + (holdFront ? rawLeftTrigger : 0f),
                        PollAxis(ParachuteAction.PullFrontLineRight) + (holdFront ? rawRightTrigger : 0f)),
                    RearRisers = new Vector2(
                        PollAxis(ParachuteAction.PullRearLineLeft) + (holdRear ? rawLeftTrigger : 0f),
                        PollAxis(ParachuteAction.PullRearLineRight) + (holdRear ? rawRightTrigger : 0f)),
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
