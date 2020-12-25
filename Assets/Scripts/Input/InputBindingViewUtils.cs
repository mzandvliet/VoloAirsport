using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using InControl;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Util;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {
    
    public static class InputBindingView {

        public static string InputSource2String(LanguageTable languageTable, InputSource inputSource, 
            Maybe<UnityInputDeviceProfile> currentControllerProfile) {

            return Interactable2String(languageTable, inputSource.SourceType, inputSource.Interactable, currentControllerProfile);
        }

        public static string Interactable2String(LanguageTable languageTable, InputSourceType sourceType, Interactable interactable, 
            Maybe<UnityInputDeviceProfile> currentControllerProfile) {

            var l = languageTable.Table;
            if (interactable is Interactable.Key) {
                var key = ((Interactable.Key) interactable).Id;
                if (key == KeyCode.UpArrow) {
                    return "↑";
                } else if (key == KeyCode.DownArrow) {
                    return "↓";
                } else if (key == KeyCode.LeftArrow) {
                    return "←";
                } else if (key == KeyCode.RightArrow) {
                    return "→";
                } else if (key == KeyCode.Return) {
                    return "Enter";
                } else {
                    return EnumUtils.ToPrettyString(key);    
                }
            } else if (interactable is Interactable.MouseButton) {
                var mouseButtonId = ((Interactable.MouseButton) interactable).Id;
                if (mouseButtonId == MouseButtonId.Left) {
                    return l["left_mouse_button"];
                } else if (mouseButtonId == MouseButtonId.Right) {
                    return l["right_mouse_button"];
                } else if (mouseButtonId == MouseButtonId.Middle) {
                    return l["middle_mouse_button"];
                } else {
                    return l["mouse_button_n"].Replace("$n", ((int) mouseButtonId).ToString());
                }
            } else if (interactable is Interactable.Button) {
                var buttonId = ((Interactable.Button) interactable).Id;
                string customMappingStr = null;
                // TODO Factor into a function
                if (currentControllerProfile.IsJust) {
                    var inputSources = UnityInputDeviceProfiles.InputSources[currentControllerProfile.Value.GetType()];
                    InputControlMapping inputControlMapping;
                    if (inputSources.TryGetValue(new InputControlSource.Button(buttonId), out inputControlMapping)) {
                        customMappingStr = inputControlMapping.Handle;
                    }
                }

                if (customMappingStr == null) {
                    return l[(sourceType.Peripheral.ToString().ToLower() + "_button_n")].Replace("$n", buttonId.ToString());
                }
                return customMappingStr;
            } else if (interactable is Interactable.JoystickAxis) {
                var axisId = ((Interactable.JoystickAxis) interactable).Id;
                string customMappingStr = null;
                // TODO Factor into a function
                if (currentControllerProfile.IsJust) {
                    var inputSources = UnityInputDeviceProfiles.InputSources[currentControllerProfile.Value.GetType()];
                    InputControlMapping inputControlMapping;
                    if (inputSources.TryGetValue(new InputControlSource.Axis(axisId), out inputControlMapping)) {
                        customMappingStr = inputControlMapping.Handle;
                    }
                }

                if (customMappingStr == null) {
                    return l["joystick_axis_n"].Replace("$n", axisId.ToString());
                }
                return customMappingStr;
            } else if (interactable is Interactable.XInputAxis) {
                var axisId = ((Interactable.XInputAxis) interactable).Id;
                return EnumUtils.ToPrettyString(axisId);
            } else if (interactable is Interactable.XInputButton) {
                var buttonId = ((Interactable.XInputButton) interactable).Id;
                return EnumUtils.ToPrettyString(buttonId);
            } else if (interactable is Interactable.MouseAxis) {
                var axisId = ((Interactable.MouseAxis) interactable).Id;
                return l["mouse_axis_n"].Replace("$n", axisId.ToString());
            } else if (interactable is Interactable.PolarizedAxis) {
                var polarizedInteractable = (Interactable.PolarizedAxis) interactable;

                // TODO Factor into a function
                if (currentControllerProfile.IsJust && polarizedInteractable.Axis is Interactable.JoystickAxis) {
                    var joystickAxisId = polarizedInteractable.Axis as Interactable.JoystickAxis;
                    var inputSources = UnityInputDeviceProfiles.InputSources[currentControllerProfile.Value.GetType()];
                    InputControlMapping inputControlMapping;
                    if (inputSources.TryGetValue(new InputControlSource.Axis(joystickAxisId.Id), out inputControlMapping)) {
                        var customMappingStr = inputControlMapping.Handle;
                        var polarity = inputControlMapping.Invert ? polarizedInteractable.Polarity.Invert() : polarizedInteractable.Polarity;

                        if (polarity == AxisPolarity.Positive) {
                            return customMappingStr
                                .ReplaceLast("Y", "Up")
                                .ReplaceLast("X", "Right");
                        }
                        return customMappingStr
                            .ReplaceLast("Y", "Down")
                            .ReplaceLast("X", "Left");
                    }
                }

                return Interactable2String(languageTable, sourceType, polarizedInteractable.Axis, currentControllerProfile) +
                    (polarizedInteractable.Polarity == AxisPolarity.Positive ? "+" : "-");
            } else {
                throw new Exception("Unknown interactable type " + interactable.GetType());
            }
        }

        public static IList<InputBindingViewModel> ToBindings<TAction>(
            LanguageTable languageTable,
            InputBindingGroup bindingGroup,
            EmptyBinding<TAction>[] mappableBindings, 
            InputSourceMapping<TAction> inputMapping,
            Maybe<UnityInputDeviceProfile> currentControllerProfile) {

            // TODO Optimize for garbage collection, use object pool
            var l = languageTable.AsFunc;

            var bindings = new List<InputBindingViewModel>();

            for (int i = 0; i < mappableBindings.Length; i++) {
                var mappableBinding = mappableBindings[i];
                var inputSources = inputMapping.Mappings.GetOrDefault(mappableBinding.Id, null) ?? ImmutableList<InputSource>.Empty;
                string binding;
                if (inputSources.Count > 0) {
                    binding = InputSource2String(languageTable, inputSources[0], currentControllerProfile);
                } else {
                    binding = null;
                }
                bindings.Add(new InputBindingViewModel(
                    new InputBindingId(bindingGroup, mappableBinding.Id), 
                    l(bindingGroup.ToString().ToUnderscoreCase()),
                    l(mappableBinding.Id.ToString().ToUnderscoreCase()),
                    l(mappableBinding.BindingType.ToUnderscoreCase()),
                    binding));
            }
            return bindings;
        }
    }

    public struct EmptyBinding<TBindingId> {
        public readonly TBindingId Id;
        public readonly string Name;
        public readonly string BindingType;

        public EmptyBinding(TBindingId id, string name, string bindingType) {
            Id = id;
            Name = name;
            BindingType = bindingType;
        }
    }
    

    /// <summary>
    /// A view class for representing an input binding in a Gui
    /// </summary>
    public struct InputBindingViewModel {
        public InputBindingId Id;
        public string Group;
        public string Name;
        public string BindingType;
        public string Binding;

        public InputBindingViewModel(InputBindingId id, string group, string name, string bindingType, string binding) {
            Group = group;
            Id = id;
            Name = name;
            BindingType = bindingType;
            Binding = binding;
        }

        public override string ToString() {
            return string.Format("{0} ({1}): {2}", Id, BindingType, Binding ?? "<Empty>");
        }
    }

    public struct InputBindingId : IEquatable<InputBindingId> {
        public readonly InputBindingGroup Group;
        public readonly object ActionId;

        public InputBindingId(InputBindingGroup @group, object actionId) {
            Group = @group;
            ActionId = actionId;
        }

        public bool Equals(InputBindingId other) {
            return Group == other.Group && Equals(ActionId, other.ActionId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InputBindingId && Equals((InputBindingId) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) Group * 397) ^ (ActionId != null ? ActionId.GetHashCode() : 0);
            }
        }

        public static bool operator ==(InputBindingId left, InputBindingId right) {
            return left.Equals(right);
        }

        public static bool operator !=(InputBindingId left, InputBindingId right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("{0}.{1}", Group, ActionId);
        }
    }

    public enum InputBindingGroup {
        Wingsuit, Menu, Parachute, Spectator
    }
}