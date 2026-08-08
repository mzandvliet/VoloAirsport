using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace RamjetAnvil.Volo.Input {

    public enum ButtonState { Released, Pressed }
    public enum ButtonEvent { Nothing, Down, Up }

    /// <summary>
    /// Generic enum-keyed wrapper around the Input System. Replaces the old Impero + InControl
    /// combo; concrete subclasses (PilotActionMap, MenuActionMap, ParachuteActionMap) set up the
    /// actual bindings, this just provides the polling surface the rest of the game expects.
    /// </summary>
    public class ActionMap<TAction> : IDisposable where TAction : struct, Enum {
        private readonly InputActionMap _inputActionMap;
        private readonly Dictionary<TAction, InputAction> _axisActions = new Dictionary<TAction, InputAction>();
        private readonly Dictionary<TAction, InputAction> _mouseAxisActions = new Dictionary<TAction, InputAction>();
        private readonly Dictionary<TAction, InputAction> _buttonActions = new Dictionary<TAction, InputAction>();

        protected ActionMap(InputActionMap inputActionMap) {
            _inputActionMap = inputActionMap;
            _inputActionMap.Enable();
        }

        protected InputAction AddAxis(TAction action, string name) {
            var inputAction = _inputActionMap.AddAction(name, InputActionType.Value);
            _axisActions[action] = inputAction;
            return inputAction;
        }

        protected InputAction AddMouseAxis(TAction action, string name) {
            var inputAction = _inputActionMap.AddAction(name, InputActionType.Value);
            _mouseAxisActions[action] = inputAction;
            return inputAction;
        }

        protected InputAction AddButton(TAction action, string name) {
            var inputAction = _inputActionMap.AddAction(name, InputActionType.Button);
            _buttonActions[action] = inputAction;
            return inputAction;
        }

        public float PollAxis(TAction action) {
            InputAction inputAction;
            return _axisActions.TryGetValue(action, out inputAction) ? inputAction.ReadValue<float>() : 0f;
        }

        public float PollMouseAxis(TAction action) {
            InputAction inputAction;
            return _mouseAxisActions.TryGetValue(action, out inputAction) ? inputAction.ReadValue<float>() : 0f;
        }

        public ButtonState PollButton(TAction action) {
            InputAction inputAction;
            if (_buttonActions.TryGetValue(action, out inputAction)) {
                return inputAction.IsPressed() ? ButtonState.Pressed : ButtonState.Released;
            }
            return ButtonState.Released;
        }

        public ButtonEvent PollButtonEvent(TAction action) {
            InputAction inputAction;
            if (!_buttonActions.TryGetValue(action, out inputAction)) {
                return ButtonEvent.Nothing;
            }
            if (inputAction.WasPressedThisFrame()) {
                return ButtonEvent.Down;
            }
            if (inputAction.WasReleasedThisFrame()) {
                return ButtonEvent.Up;
            }
            return ButtonEvent.Nothing;
        }

        public void Dispose() {
            _inputActionMap.Disable();
        }
    }
}
