using UnityEngine;
using UnityEngine.InputSystem;

namespace RamjetAnvil.Volo.Input {

    public enum SpectatorAction {
        MoveHorizontal, MoveVertical, MoveUpDown, LookHorizontal, LookVertical, SpeedUp
    }

    // Todo: placeholder bindings, needs real tuning.
    public class SpectatorActionMap : ActionMap<SpectatorAction> {
        public SpectatorActionMap() : base(new InputActionMap("Spectator")) {
            SetupBindings();
        }

        private void SetupBindings() {
            var moveHorizontal = AddAxis(SpectatorAction.MoveHorizontal, "MoveHorizontal");
            moveHorizontal.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            moveHorizontal.AddBinding("<Gamepad>/leftStick/x");

            var moveVertical = AddAxis(SpectatorAction.MoveVertical, "MoveVertical");
            moveVertical.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/s")
                .With("Positive", "<Keyboard>/w");
            moveVertical.AddBinding("<Gamepad>/leftStick/y");

            var moveUpDown = AddAxis(SpectatorAction.MoveUpDown, "MoveUpDown");
            moveUpDown.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftCtrl")
                .With("Positive", "<Keyboard>/space");
            moveUpDown.AddCompositeBinding("1DAxis")
                .With("Negative", "<Gamepad>/leftShoulder")
                .With("Positive", "<Gamepad>/rightShoulder");

            var lookHorizontal = AddAxis(SpectatorAction.LookHorizontal, "LookHorizontal");
            lookHorizontal.AddBinding("<Mouse>/delta/x");
            lookHorizontal.AddBinding("<Gamepad>/rightStick/x");

            var lookVertical = AddAxis(SpectatorAction.LookVertical, "LookVertical");
            lookVertical.AddBinding("<Mouse>/delta/y");
            lookVertical.AddBinding("<Gamepad>/rightStick/y");

            var speedUp = AddAxis(SpectatorAction.SpeedUp, "SpeedUp");
            speedUp.AddBinding("<Keyboard>/leftShift");
            speedUp.AddBinding("<Gamepad>/leftTrigger");
        }

        public Vector3 Movement() {
            return new Vector3(PollAxis(SpectatorAction.MoveHorizontal), PollAxis(SpectatorAction.MoveUpDown), PollAxis(SpectatorAction.MoveVertical));
        }

        public Vector2 LookDirection() {
            return new Vector2(PollAxis(SpectatorAction.LookHorizontal), PollAxis(SpectatorAction.LookVertical));
        }

        public float SpeedUp() {
            return PollAxis(SpectatorAction.SpeedUp);
        }
    }
}
