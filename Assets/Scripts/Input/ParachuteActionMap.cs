using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RamjetAnvil.Volo.Input {

    // Todo: field shape is a first guess pending real parachute-control retuning.
    [System.Serializable]
    public struct ParachuteInput {
        public float WeightShiftHorizontal;
        public float WeightShiftVertical;
        public float BrakeLeft;
        public float BrakeRight;

        public static readonly ParachuteInput Zero = new ParachuteInput();
    }

    public class ParachuteActionMap : ActionMap<ParachuteAction> {
        public ParachuteActionMap() : base(new InputActionMap("Parachute")) {
            SetupBindings();
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

            var pullLeft = AddAxis(ParachuteAction.PullLeftLines, "PullLeftLines");
            pullLeft.AddBinding("<Gamepad>/leftTrigger");
            pullLeft.AddBinding("<Keyboard>/q");

            var pullRight = AddAxis(ParachuteAction.PullRightLines, "PullRightLines");
            pullRight.AddBinding("<Gamepad>/rightTrigger");
            pullRight.AddBinding("<Keyboard>/e");
        }

        public ParachuteInput Input {
            get {
                return new ParachuteInput {
                    WeightShiftHorizontal = PollAxis(ParachuteAction.WeightShiftRight) - PollAxis(ParachuteAction.WeightShiftLeft),
                    WeightShiftVertical = PollAxis(ParachuteAction.WeightShiftFront) - PollAxis(ParachuteAction.WeightShiftBack),
                    BrakeLeft = PollAxis(ParachuteAction.PullLeftLines),
                    BrakeRight = PollAxis(ParachuteAction.PullRightLines)
                };
            }
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

        private void OnDestroy() {
            if (_actionMap != null) {
                _actionMap.Dispose();
            }
        }
    }
}
