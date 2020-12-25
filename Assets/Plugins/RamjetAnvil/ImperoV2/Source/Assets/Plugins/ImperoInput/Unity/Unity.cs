using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Util;
using UnityEngine;

namespace RamjetAnvil.Impero.Unity {
    public static class Clock {
        public static readonly Func<float> DeltaTime = () => Time.deltaTime;
        public static readonly Func<float> CurrentTime = () => Time.time;
        public static readonly Func<float> FixedDeltaTime = () => Time.fixedDeltaTime;
        public static readonly Func<float> CurrentFixedTime = () => Time.fixedTime;
        public static readonly Func<int> FrameCounter = () => Time.frameCount;
    }
    
    public static class UnityInputIds {
        public static readonly IEnumerable<KeyCode> Keys = KeyCodeRange(KeyCode.Backspace, KeyCode.Break);
        public static readonly IEnumerable<int> MouseButtons = Enumerable.Range(0, 6);
        public static readonly IEnumerable<string> MouseAxes = new[] {"Mouse X", "Mouse Y"};
        public static readonly IEnumerable<int> ControllerAxes = Enumerable.Range(0, 10);
        public static readonly IEnumerable<int> ControllerButtons = Enumerable.Range(0, 20);
        public static readonly IEnumerable<int> ControllerIds = Enumerable.Range(0, 11);

        public static readonly IEnumerable<CombinedAxisId<int>> Xbox360Axes =
            new[] {
                new CombinedAxisId<int>(0, 1),
                new CombinedAxisId<int>(3, 4),
                new CombinedAxisId<int>(5, 6)
            };

        public static readonly IDictionary<CombinedAxisId<int>, float> Xbox360Deadzones =
            new Dictionary<CombinedAxisId<int>, float> {
                {new CombinedAxisId<int>(0, 1), 0.17f},
                {new CombinedAxisId<int>(3, 4), 0.17f},
                {new CombinedAxisId<int>(5, 6), 0f}
            };

        private static IEnumerable<KeyCode> KeyCodeRange(KeyCode first, KeyCode last) {
            return KeyCodeRange((int) first, (int) last);
        }

        private static IEnumerable<KeyCode> KeyCodeRange(int first, int last) {
            return Enumerable.Range(first, last - first).Select(keyCode => (KeyCode) keyCode);
        }
    }

    public static class UnityInputMaps {
        public static readonly InputMap<KeyCode, ButtonState> KeyInput = ImperoCore
            .ToInputMap<KeyCode, bool>(UnityInputIds.Keys, Input.GetKey)
            .Adapt<KeyCode, bool, ButtonState>(Adapters.Bool2ButtonState);

        public static readonly InputMap<MouseButtonId, ButtonState> MouseButtonInput =
            ImmutableDictionary<MouseButtonId, Func<bool>>.Empty
                .Add(MouseButtonId.Left, () => Input.GetMouseButton(0))
                .Add(MouseButtonId.Right, () => Input.GetMouseButton(1))
                .Add(MouseButtonId.Middle, () => Input.GetMouseButton(2))
                .Add(MouseButtonId.Fourth, () => Input.GetMouseButton(3))
                .Add(MouseButtonId.Fifth, () => Input.GetMouseButton(4))
                .Add(MouseButtonId.Sixth, () => Input.GetMouseButton(5))
                .Add(MouseButtonId.Seventh, () => Input.GetMouseButton(6))
                .ToInputMap()
                .Adapt<MouseButtonId, bool, ButtonState>(Adapters.Bool2ButtonState);

        public static readonly InputMap<MouseAxisId, float> MouseAxesInput =
            ImmutableDictionary<MouseAxisId, Func<float>>.Empty
                .Add(MouseAxisId.X, () => Input.GetAxisRaw("Mouse X"))
                .Add(MouseAxisId.Y, () => Input.GetAxisRaw("Mouse Y"))
                .ToInputMap();

        public static InputMap<int, ButtonState> JoystickButtonInput(int joystickId) {
            var buttonInput = new Dictionary<int, Func<ButtonState>>();
            foreach (var buttonIndex in UnityInputIds.ControllerButtons) {
                var unityButtonId = (KeyCode)((int) KeyCode.Joystick1Button0 + (joystickId * UnityInputIds.ControllerButtons.Count()) + buttonIndex);
                buttonInput[buttonIndex] = () => Adapters.Bool2ButtonState(Input.GetKey(unityButtonId));
            }
            return buttonInput.ToInputMap();
        }
        
        public static InputMap<int, float> JoystickAxisInput(int joystickId) {
            var axisInput = new Dictionary<int, Func<float>>();
            foreach (var axisIndex in UnityInputIds.ControllerAxes) {
                var unityAxisId = "joy_" + joystickId + "_" + axisIndex;
                axisInput[axisIndex] = () => Input.GetAxisRaw(unityAxisId);
            }
            return axisInput.ToInputMap();
        }

        public static InputMap<CombinedAxisId<TId>, Vector2> CombineAxes<TId>(this InputMap<TId, float> sourceMap,
            IEnumerable<CombinedAxisId<TId>> axisIds) {
            return axisIds.ToDictionary(
                axisId => axisId,
                axisId => CombineAxes(sourceMap.Source[axisId.X], sourceMap.Source[axisId.Y]))
                .ToInputMap();
        }

        private static Func<Vector2> CombineAxes(Func<float> x, Func<float> y) {
            return () => new Vector2(x(), y());
        }

        public static InputMap<TId, float> SplitAxes<TId>(this InputMap<CombinedAxisId<TId>, Vector2> inputMap) {
            return inputMap.Source.SelectMany(sourceEntry => {
                Func<Vector2> pollAxisVector = sourceEntry.Value;
                return new[] {
                    new KeyValuePair<TId, Func<float>>(sourceEntry.Key.X, () => pollAxisVector().x),
                    new KeyValuePair<TId, Func<float>>(sourceEntry.Key.Y, () => pollAxisVector().y)
                };
            }).ToInputMap();
        }

        public static InputMap<PolarizedAxisId<TId>, float> PolarizeAxes<TId>(this InputMap<TId, float> inputMap) {
            return inputMap.Source.SelectMany(sourceEntry => {
                TId axisId = sourceEntry.Key;
                Func<float> pollAxis = sourceEntry.Value;
                return new[] {
                    new KeyValuePair<PolarizedAxisId<TId>, Func<float>>(
                        new PolarizedAxisId<TId>(axisId, AxisPolarity.Positive),
                        pollAxis.Adapt<float, float>(Adapters.FilterPositiveInput)),
                    new KeyValuePair<PolarizedAxisId<TId>, Func<float>>(
                        new PolarizedAxisId<TId>(axisId, AxisPolarity.Negative),
                        pollAxis.Adapt<float, float>(Adapters.FilterNegativeInput)
                                .Adapt<float, float>(Adapters.Abs))
                };
            }).ToInputMap();
        }

        public static InputMap<CombinedAxisId<TId>, Vector2> ApplyDeadzone<TId>(
            this InputMap<CombinedAxisId<TId>, Vector2> inputMap,
            Func<CombinedAxisId<TId>, float> deadzones) {
            return inputMap.Adapt((axisId, pollFn) => {
                float deadzone = deadzones(axisId);
                if (deadzone > 0.0f) {
                    return pollFn.Adapt(Adapters.StickDeadzone(deadzone));
                }
                return pollFn;
            });
        }

        public static int? AnyJoystick() {
            // Note: JoystickButton0 is button 0 for all joysticks, don't use it
            const int startIndex = (int)KeyCode.Joystick1Button0;
            const int numJoysticks = 8;
            const int numButtonsPerJoystick = 20;

            for (int joystickId = 0; joystickId < numJoysticks; joystickId++) {
                int initialJoystickIndex = numButtonsPerJoystick * joystickId + startIndex;
                for (int i = 0; i < numButtonsPerJoystick; i++) {
                    var buttonIndex = (KeyCode)(initialJoystickIndex + i);
                    if (Input.GetKeyDown(buttonIndex)) {
                        return joystickId;
                    }
                }
            }
            return null;
        }
    }

    public enum MouseButtonId { Left = 1, Right = 2, Middle = 3, Fourth = 4, Fifth = 5, Sixth = 6, Seventh = 7 }

    public enum MouseAxisId { X, Y }

    public enum AxisPolarity {
        Positive,
        Negative
    }

    public static class AxisPolarityExtensions {
        public static AxisPolarity Invert(this AxisPolarity polarity) {
            return polarity == AxisPolarity.Positive ? AxisPolarity.Negative : AxisPolarity.Positive;
        }
    }

    public class PolarizedAxisId<T> {
        public readonly T AxisId;
        public readonly AxisPolarity Polarity;

        public PolarizedAxisId(T axisId, AxisPolarity polarity) {
            AxisId = axisId;
            Polarity = polarity;
        }

        protected bool Equals(PolarizedAxisId<T> other) {
            return EqualityComparer<T>.Default.Equals(AxisId, other.AxisId) && Polarity == other.Polarity;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != GetType()) {
                return false;
            }
            return Equals((PolarizedAxisId<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(AxisId)*397) ^ (int) Polarity;
            }
        }

        public override string ToString() {
            return AxisId + " " + (Polarity == AxisPolarity.Positive ? "+" : "-");
        }
    }

    public class CombinedAxisId<T> {
        public readonly T X;
        public readonly T Y;

        public CombinedAxisId(T x, T y) {
            X = x;
            Y = y;
        }

        public bool Equals(CombinedAxisId<T> other) {
            return EqualityComparer<T>.Default.Equals(X, other.X) && EqualityComparer<T>.Default.Equals(Y, other.Y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is CombinedAxisId<T> && Equals((CombinedAxisId<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(X)*397) ^ EqualityComparer<T>.Default.GetHashCode(Y);
            }
        }
    }
}