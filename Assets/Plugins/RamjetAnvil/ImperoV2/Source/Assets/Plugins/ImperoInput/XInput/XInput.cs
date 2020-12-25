using System;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using XInputDotNetPure;
using ButtonState = RamjetAnvil.Impero.StandardInput.ButtonState;

namespace RamjetAnvil.Impero {
    public static class XInput {

        public enum Button {
            Start, Back, LeftStick, RightStick, LeftShoulder, RightShoulder, Guide, A, B, X, Y,
            DPadUp, DPadDown, DPadLeft, DPadRight
        }

        public enum Axis {
            LeftStickY, LeftStickX, RightStickY, RightStickX, LeftTrigger, RightTrigger
        }

        public static Func<GamePadState> CreateController(PlayerIndex playerIndex, GamePadDeadZone deadzone) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            //return () => new GamePadState(); // Turn off XInput all-together      
            return () => GamePad.GetState(playerIndex, deadzone);
#else
            return () => new GamePadState();       
#endif
        }

        public static PlayerIndex? AnyJoystick() {
            var playerIndices = EnumUtils.GetValues<PlayerIndex>();
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            for (int i = 0; i < playerIndices.Count; i++) {
                var playerIndex = playerIndices[i];
                var gamepadState = GamePad.GetState(playerIndex);
                var buttonStates = gamepadState.Buttons;
                var dpadState = gamepadState.DPad;
                if (buttonStates.A == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.B == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.X == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.Y == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.Back == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.Guide == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.LeftShoulder == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.LeftStick == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.RightShoulder == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.RightStick == XInputDotNetPure.ButtonState.Pressed ||
                    buttonStates.Start == XInputDotNetPure.ButtonState.Pressed ||
                    dpadState.Down == XInputDotNetPure.ButtonState.Pressed ||
                    dpadState.Up == XInputDotNetPure.ButtonState.Pressed ||
                    dpadState.Left == XInputDotNetPure.ButtonState.Pressed ||
                    dpadState.Right == XInputDotNetPure.ButtonState.Pressed) {

                    return playerIndex;
                }
            }
#endif
            return null;
        }

        public static InputMap<Button, ButtonState> Buttons(Func<GamePadState> pollState) {
            return new InputMap<Button, XInputDotNetPure.ButtonState>(new Dictionary<Button, Func<XInputDotNetPure.ButtonState>> {
                    {Button.Start, pollState.Adapt(state => state.Buttons.Start)},
                    {Button.Back, pollState.Adapt(state => state.Buttons.Back)},
                    {Button.LeftStick, pollState.Adapt(state => state.Buttons.LeftStick)},
                    {Button.RightStick, pollState.Adapt(state => state.Buttons.RightStick)},
                    {Button.LeftShoulder, pollState.Adapt(state => state.Buttons.LeftShoulder)},
                    {Button.RightShoulder, pollState.Adapt(state => state.Buttons.RightShoulder)},
                    {Button.Guide, pollState.Adapt(state => state.Buttons.Guide)},
                    {Button.A, pollState.Adapt(state => state.Buttons.A)},
                    {Button.B, pollState.Adapt(state => state.Buttons.B)},
                    {Button.X, pollState.Adapt(state => state.Buttons.X)},
                    {Button.Y, pollState.Adapt(state => state.Buttons.Y)},
                    {Button.DPadUp, pollState.Adapt(state => state.DPad.Up)},
                    {Button.DPadDown, pollState.Adapt(state => state.DPad.Down)},
                    {Button.DPadLeft, pollState.Adapt(state => state.DPad.Left)},
                    {Button.DPadRight, pollState.Adapt(state => state.DPad.Right)},
                }.ToFastImmutableEnumDictionary())
                .Adapt(XInputToImperoButtonState);
        }

        public static InputMap<Axis, float> Axes(Func<GamePadState> pollState) {
            return new InputMap<Axis, float>(new Dictionary<Axis, Func<float>> {
                {Axis.LeftStickX, pollState.Adapt(state => state.ThumbSticks.Left.X)},
                {Axis.LeftStickY, pollState.Adapt(state => state.ThumbSticks.Left.Y)},
                {Axis.RightStickX, pollState.Adapt(state => state.ThumbSticks.Right.X)},
                {Axis.RightStickY, pollState.Adapt(state => state.ThumbSticks.Right.Y)},
                {Axis.LeftTrigger, pollState.Adapt(state => state.Triggers.Left)},
                {Axis.RightTrigger, pollState.Adapt(state => state.Triggers.Right)},
            }.ToFastImmutableEnumDictionary());
        }

        private static readonly Func<XInputDotNetPure.ButtonState, ButtonState> XInputToImperoButtonState =
            b => b == XInputDotNetPure.ButtonState.Pressed ? ButtonState.Pressed : ButtonState.Released;
    }
}
