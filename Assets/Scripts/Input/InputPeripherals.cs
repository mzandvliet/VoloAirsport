using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using XInputDotNetPure;
using ButtonState = RamjetAnvil.Impero.StandardInput.ButtonState;

namespace RamjetAnvil.Volo.Input {

    public class InputSourceMapping<TTargetId> : InputMapping<InputSource, TTargetId> {
        public const int CurrentFormatVersion = 4;

        public int FormatVersion { get; private set; }

        public static readonly InputSourceMapping<TTargetId> Empty = new InputSourceMapping<TTargetId>(
            ImmutableDictionary<TTargetId, IImmutableList<InputSource>>.Empty, CurrentFormatVersion);

        public InputSourceMapping(IImmutableDictionary<TTargetId, IImmutableList<InputSource>> mappings,
            int formatVersion) : base(mappings) {
            FormatVersion = formatVersion;
        }
    }

    public static class InputSourceExtensions {

        public static readonly JsonSerializer Serializer;

        static InputSourceExtensions()  {
            Serializer = new JsonSerializer {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            Serializer.Converters.Add(new StringEnumConverter());
        }

        public static InputSourceMapping<TActionId> DeserializeMapping<TActionId>(string configUri) {
            using (var reader = new StreamReader(configUri, Encoding.UTF8)) {
                return DeserializeMapping<TActionId>(reader);
            }
        }

        public static InputSourceMapping<T> DeserializeMapping<T>(TextReader reader) {
            using (JsonReader jsonReader = new JsonTextReader(reader)) {
                var mapping = Serializer.Deserialize<InputSourceMapping<T>>(jsonReader);
                if (mapping == null) {
                    throw new Exception("Unable to deserialize input mapping for " + typeof(T));
                }
                if (mapping.FormatVersion != InputSourceMapping<T>.CurrentFormatVersion) {
                    throw new Exception("Cannot read input bindings of format version " + mapping.FormatVersion + " because" +
                                        " it is different from the current format version " + InputSourceMapping<T>.CurrentFormatVersion);
                }
                return mapping;
            }
        }

        public static string Serialize<TTargetId>(this InputSourceMapping<TTargetId> inputMapping) {
            return Serializer.Serialize2String(inputMapping.Mappings);
        }

        public static void Serialize2Disk<TTargetId>(this InputSourceMapping<TTargetId> inputMapping, string path) {
            using (var fileStream = new FileStream(path, FileMode.Create)) 
            using (var textWriter = new StreamWriter(fileStream)) {
                Serializer.Serialize(textWriter, inputMapping);
            }
        }

        public static InputSourceMapping<TTargetId> ToInputSourceMapping<TTargetId>(
            this IImmutableDictionary<TTargetId, IImmutableList<InputSource>> mappings) {
            return new InputSourceMapping<TTargetId>(mappings, formatVersion: InputSourceMapping<TTargetId>.CurrentFormatVersion);
        }
    }

    public static class Peripherals {
        public static readonly InputMap<InputSource, ButtonState> Keyboard =
            UnityInputMaps.KeyInput.ChangeId(keyCode => new InputSource(Peripheral.Keyboard, InputType.Button, new Interactable.Key(keyCode)));

        public static class Mouse {
            public static readonly InputMap<InputSource, ButtonState> Buttons =
                UnityInputMaps.MouseButtonInput.ChangeId(buttonId => new InputSource(Peripheral.Mouse, InputType.Button, new Interactable.MouseButton(buttonId)));

            public static readonly InputMap<InputSource, float> Axes = UnityInputMaps.MouseAxesInput
                .ChangeId(mouseAxisId => new InputSource(Peripheral.Mouse, InputType.Axis, new Interactable.MouseAxis(mouseAxisId)));

            public static readonly InputMap<InputSource, float> PolarizedAxes = UnityInputMaps.MouseAxesInput
                .PolarizeAxes()
                .ChangeId(mouseAxis =>
                    new InputSource(
                        Peripheral.Mouse, 
                        InputType.Axis,
                        new Interactable.PolarizedAxis(mouseAxis.Polarity, new Interactable.MouseAxis(mouseAxis.AxisId))));
        }

        public static class Controller {
            public static readonly Func<int, InputMap<InputSource, ButtonState>> Buttons =
                Memoization.Memoize<int, InputMap<InputSource, ButtonState>>(joystickId => {
                    return UnityInputMaps.JoystickButtonInput(joystickId)
                        .ChangeId(buttonIndex => new InputSource(Peripheral.Joystick, InputType.Button, new Interactable.Button(buttonIndex)));
                });

            public static readonly Func<int, InputMap<InputSource, float>> PolarizedAxes = Memoization.Memoize<int, InputMap<InputSource, float>>(
                joystickId => {
                    return UnityInputMaps.JoystickAxisInput(joystickId)
                        .PolarizeAxes()
                        .ChangeId(polarizedAxis => new InputSource(
                            Peripheral.Joystick, InputType.Axis,
                            new Interactable.PolarizedAxis(polarizedAxis.Polarity, new Interactable.JoystickAxis(polarizedAxis.AxisId))));
                });

            public static readonly Func<PlayerIndex, InputMap<InputSource, ButtonState>> XInputButtons =
                Memoization.Memoize<PlayerIndex, InputMap<InputSource, ButtonState>>(playerId => {
                    return XInput.Buttons(XInput.CreateController(playerId, GamePadDeadZone.None).Cache(() => Time.frameCount))
                        .ChangeId(buttonIndex => new InputSource(Peripheral.XInputGamepad, InputType.Button, new Interactable.XInputButton(buttonIndex)));
                });

            public static readonly Func<PlayerIndex, InputMap<InputSource, float>> PolarizedXInputAxes = Memoization.Memoize<PlayerIndex, InputMap<InputSource, float>>(
                playerId => {
                    return XInput.Axes(XInput.CreateController(playerId, GamePadDeadZone.Circular).Cache(() => Time.frameCount))
                        .PolarizeAxes()
                        .ChangeId(polarizedAxis => new InputSource(
                            Peripheral.XInputGamepad, InputType.Axis,
                            new Interactable.PolarizedAxis(polarizedAxis.Polarity, new Interactable.XInputAxis(polarizedAxis.AxisId))));
                });

            public static ControllerPeripheral GetPeripheral(ControllerId id, InputSettings inputSettings) {
                InputMap<InputSource, ButtonState> buttons;
                InputMap<InputSource, float> axes;
                if (id is ControllerId.Unity) {
                    var controllerId = id as ControllerId.Unity;
                    buttons = Buttons(controllerId.Id);
                    axes = PolarizedAxes(controllerId.Id)
                        .Adapt(Adapters.Deadzone(inputSettings.JoystickDeadzone));
                } else if (id is ControllerId.XInput) {
                    var controllerId = id as ControllerId.XInput;
                    buttons = XInputButtons(controllerId.Id);
                    axes = PolarizedXInputAxes(controllerId.Id);
                } else {
                    buttons = InputMap<InputSource, ButtonState>.Empty;
                    axes = InputMap<InputSource, float>.Empty;
                }
                return new ControllerPeripheral(buttons, axes);
            }
        }

        public class ControllerPeripheral {
            public readonly InputMap<InputSource, ButtonState> Buttons;
            public readonly InputMap<InputSource, float> Axes;

            public ControllerPeripheral(InputMap<InputSource, ButtonState> buttons, InputMap<InputSource, float> axes) {
                Buttons = buttons;
                Axes = axes;
            }
        }
    }

    // The following classes represent these algebriac data types:
    //
    // InputSource = InputSourceType InteractableId;
    //
    // InputSourceType = Peripheral InputType;
    // Peripheral = Mouse | Keyboard | Joystick | XInputGamepad;
    // InputType = Axis | Button
    //
    // InteractableId = ButtonId int | AxisId int | MouseAxis MouseAxisId | Polarized InteractableId |
    //                  XInputButton XInputButtonId | XInputAxis XInputAxisId
    // MouseAxisId = X | Y
    // 
    // Apparently in C# these couple lines of Haskell code translate to 200 lines. Sorry about that.
    
    public struct InputSource : IEquatable<InputSource> {
        public InputSourceType SourceType;
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public Interactable Interactable;

        public InputSource(Peripheral peripheral, InputType inputType, Interactable interactable) {
            SourceType = new InputSourceType(peripheral, inputType);
            Interactable = interactable;
        }

        public InputSource(InputSourceType sourceType, Interactable interactable) {
            SourceType = sourceType;
            Interactable = interactable;
        }

        public bool Equals(InputSource other) {
            return SourceType.Equals(other.SourceType) && Equals(Interactable, other.Interactable);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InputSource && Equals((InputSource) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (SourceType.GetHashCode() * 397) ^ (Interactable != null ? Interactable.GetHashCode() : 0);
            }
        }

        public static bool operator ==(InputSource left, InputSource right) {
            return left.Equals(right);
        }

        public static bool operator !=(InputSource left, InputSource right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("{0} {1}", SourceType, Interactable);
        }

        public static InputSource PolarizedJoystickAxis(AxisPolarity polarity, int axisId) {
            return new InputSource(Peripheral.Joystick, InputType.Axis, new Interactable.PolarizedAxis(polarity, new Interactable.JoystickAxis(axisId)));
        }

        public static InputSource JoystickButton(int id) {
            return new InputSource(Peripheral.Joystick, InputType.Button, new Interactable.Button(id));
        }

        public static InputSource Key(KeyCode keyCode) {
            return new InputSource(Peripheral.Keyboard, InputType.Button, new Interactable.Key(keyCode));
        }

        public static InputSource PolarizedMouseAxis(AxisPolarity polarity, MouseAxisId axisId) {
            return new InputSource(Peripheral.Mouse, InputType.Axis, new Interactable.PolarizedAxis(polarity, new Interactable.MouseAxis(axisId)));
        }

        public static readonly InputSource LeftMouseButton = InputSource.MouseButton(MouseButtonId.Left);
        public static InputSource MouseButton(MouseButtonId mouseButton) {
            return new InputSource(Peripheral.Mouse, InputType.Button, new Interactable.MouseButton(mouseButton));
        }

        public static InputSource XInputAxis(XInput.Axis axisId) {
            return new InputSource(Peripheral.XInputGamepad, InputType.Axis, new Interactable.XInputAxis(axisId));
        }

        public static InputSource PolarizedXInputAxis(AxisPolarity polarity, XInput.Axis axisId) {
            return new InputSource(Peripheral.XInputGamepad, InputType.Axis, new Interactable.PolarizedAxis(polarity, new Interactable.XInputAxis(axisId)));
        }

        public static InputSource XInputButton(XInput.Button id) {
            return new InputSource(Peripheral.XInputGamepad, InputType.Button, new Interactable.XInputButton(id));
        }
    }

    public enum Peripheral { Joystick, Mouse, Keyboard, XInputGamepad }

    public enum InputType { Axis, Button }

    public struct InputSourceType : IEquatable<InputSourceType> {

        public Peripheral Peripheral;
        public InputType InputType;

        public InputSourceType(Peripheral peripheral, InputType inputType) {
            Peripheral = peripheral;
            InputType = inputType;
        }

        public bool Equals(InputSourceType other) {
            return Peripheral == other.Peripheral && InputType == other.InputType;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InputSourceType && Equals((InputSourceType) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) Peripheral * 397) ^ (int) InputType;
            }
        }

        public static bool operator ==(InputSourceType left, InputSourceType right) {
            return left.Equals(right);
        }

        public static bool operator !=(InputSourceType left, InputSourceType right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("{0} {1}", Peripheral, InputType);
        }
    }

    public abstract class Axis : Interactable { }


    public abstract class Interactable {
        protected Interactable() { }

        public class JoystickAxis : Axis, IEquatable<JoystickAxis> {
            private readonly int _id;

            public JoystickAxis(int id) {
                _id = id;
            }

            public int Id {
                get { return _id; }
            }

            public bool Equals(JoystickAxis other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((JoystickAxis) obj);
            }

            public override int GetHashCode() {
                return _id;
            }

            public static bool operator ==(JoystickAxis left, JoystickAxis right) {
                return Equals(left, right);
            }

            public static bool operator !=(JoystickAxis left, JoystickAxis right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }

        public class MouseButton : Interactable, IEquatable<MouseButton> {
            private readonly MouseButtonId _id;

            public MouseButton(MouseButtonId id) {
                _id = id;
            }

            public MouseButtonId Id {
                get { return _id; }
            }

            public bool Equals(MouseButton other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MouseButton) obj);
            }

            public override int GetHashCode() {
                return (int) _id;
            }

            public static bool operator ==(MouseButton left, MouseButton right) {
                return Equals(left, right);
            }

            public static bool operator !=(MouseButton left, MouseButton right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }

        public class Key : Interactable, IEquatable<Key> {
            private readonly KeyCode _id;

            public Key(KeyCode id) {
                _id = id;
            }

            public KeyCode Id {
                get { return _id; }
            }

            public bool Equals(Key other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Key) obj);
            }

            public override int GetHashCode() {
                return (int) _id;
            }

            public static bool operator ==(Key left, Key right) {
                return Equals(left, right);
            }

            public static bool operator !=(Key left, Key right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }

        public class MouseAxis : Axis, IEquatable<MouseAxis> {
            private readonly MouseAxisId _id;

            public MouseAxis(MouseAxisId id) {
                _id = id;
            }

            public MouseAxisId Id {
                get { return _id; }
            }

            public bool Equals(MouseAxis other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MouseAxis) obj);
            }

            public override int GetHashCode() {
                return (int) _id;
            }

            public static bool operator ==(MouseAxis left, MouseAxis right) {
                return Equals(left, right);
            }

            public static bool operator !=(MouseAxis left, MouseAxis right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }

        public class Button : Interactable, IEquatable<Button> {
            private readonly int _id;

            public Button(int id) {
                _id = id;
            }

            public int Id {
                get { return _id; }
            }

            public bool Equals(Button other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Button) obj);
            }

            public override int GetHashCode() {
                return _id;
            }

            public static bool operator ==(Button left, Button right) {
                return Equals(left, right);
            }

            public static bool operator !=(Button left, Button right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }

        public class PolarizedAxis : Interactable, IEquatable<PolarizedAxis> {
            private readonly AxisPolarity _polarity;
            private readonly Axis _axis;

            public PolarizedAxis(AxisPolarity polarity, Axis axis) {
                _polarity = polarity;
                _axis = axis;
            }

            public AxisPolarity Polarity {
                get { return _polarity; }
            }

            public Axis Axis {
                get { return _axis; }
            }

            public bool Equals(PolarizedAxis other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _polarity == other._polarity && Equals(_axis, other._axis);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PolarizedAxis) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((int) _polarity * 397) ^ (_axis != null ? _axis.GetHashCode() : 0);
                }
            }

            public static bool operator ==(PolarizedAxis left, PolarizedAxis right) {
                return Equals(left, right);
            }

            public static bool operator !=(PolarizedAxis left, PolarizedAxis right) {
                return !Equals(left, right);
            }


            public override string ToString() {
                return string.Format("{0} {1}", _polarity, _axis);
            }
        }

        public class XInputAxis : Axis, IEquatable<XInputAxis> {
            private readonly XInput.Axis _id;

            public XInputAxis(XInput.Axis id) {
                _id = id;
            }

            public XInput.Axis Id {
                get { return _id; }
            }

            public bool Equals(XInputAxis other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((XInputAxis) obj);
            }

            public override int GetHashCode() {
                return (int) _id;
            }

            public static bool operator ==(XInputAxis left, XInputAxis right) {
                return Equals(left, right);
            }

            public static bool operator !=(XInputAxis left, XInputAxis right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }

        public class XInputButton : Interactable, IEquatable<XInputButton> {
            private readonly XInput.Button _id;

            public XInputButton(XInput.Button id) {
                _id = id;
            }

            public XInput.Button Id {
                get { return _id; }
            }

            public bool Equals(XInputButton other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((XInputButton) obj);
            }

            public override int GetHashCode() {
                return (int) _id;
            }

            public static bool operator ==(XInputButton left, XInputButton right) {
                return Equals(left, right);
            }

            public static bool operator !=(XInputButton left, XInputButton right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return _id.ToString();
            }
        }
    }
}
