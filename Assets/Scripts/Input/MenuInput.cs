using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using InControl;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public enum MenuAction {
        Left, Right, Up, Down,
        MoveCursor,
        Confirm, Back, Pause,
        RecenterVrHeadset,
        TakeScreenshot
    }

    public class MenuActionMap {
        private readonly Func<Vector2> _pollDiscreteCursor;
        private readonly Func<Vector2> _pollCursor;
        private readonly InputMap<MenuAction, ButtonState> _buttons;
        private readonly InputMap<MenuAction, ButtonEvent> _buttonEvents;

        public MenuActionMap(Func<Vector2> pollCursor, Func<Vector2> pollDiscreteCursor, InputMap<MenuAction, ButtonState> buttons) {
            _pollCursor = pollCursor;
            _pollDiscreteCursor = pollDiscreteCursor;
            _buttons = buttons.Optimize<MenuAction, ButtonState>();

            _buttonEvents = _buttons
                .Adapt<MenuAction, ButtonState, ButtonEvent>(() => Adapters.ButtonEvents(Clock.FrameCounter))
                .Optimize<MenuAction, ButtonEvent>();
        }


        public Vector2 PollCursor() {
            return _pollCursor();
        }

        public Vector2 PollDiscreteCursor() {
            return _pollDiscreteCursor();
        }

        public ButtonState PollButton(MenuAction id) {
            return _buttons.Poll(id);
        }

        public ButtonEvent PollButtonEvent(MenuAction id) {
            return _buttonEvents.Poll(id);
        }
    }

    public static class MenuInput {

        public static class MenuActionDetails {
            public static readonly MenuAction[] MenuActions = EnumUtils.GetValues<MenuAction>().ToArray();

            public static readonly MenuAction[] MenuActionButtons = new[] {
                MenuAction.Left, MenuAction.Right, MenuAction.Up, MenuAction.Down, MenuAction.Confirm, MenuAction.Back,
                MenuAction.Pause, MenuAction.RecenterVrHeadset, MenuAction.TakeScreenshot
            };

//            public static readonly MenuAction[] MenuActionButtons = new[] {
//                MenuAction.Left, MenuAction.Right, MenuAction.Up, MenuAction.Down, MenuAction.Confirm, MenuAction.Back,
//                MenuAction.Pause
//            };

            public static readonly MenuAction[] MappableActions = new[] {
                MenuAction.Pause, MenuAction.Confirm, MenuAction.Back,
                MenuAction.Left, MenuAction.Right, MenuAction.Up, MenuAction.Down,
                MenuAction.RecenterVrHeadset, MenuAction.TakeScreenshot
            };
        }

        public static class Bindings {

            public static Lazy<EmptyBinding<MenuAction>[]> EmptyBindings = new Lazy<EmptyBinding<MenuAction>[]>(
                () => {
                    return MenuInput.MenuActionDetails.MappableActions
                        .Select(action => {
                            string actionStr = EnumUtils.ToPrettyString(action);
                            return new EmptyBinding<MenuAction>(action, actionStr, InputType.Button.ToString());
                        })
                        .ToArray();
                });

            public static IEnumerable<InputBindingViewModel> ToBindings(LanguageTable languageTable, 
                InputSourceMapping<MenuAction> inputMapping,
                Maybe<UnityInputDeviceProfile> currentControllerProfile) {

                return InputBindingView.ToBindings(languageTable, InputBindingGroup.Menu, EmptyBindings.Value, inputMapping, 
                    currentControllerProfile);
            }

            public static readonly Lazy<string> CustomInputMappingFilePath = new Lazy<string>(() => {
                return Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "MenuInputConfig.json");
            });

            private static readonly IObjectPool<InputMappingBuilder<InputSource, MenuAction>> InputMappingBuilderPool = ObjectPool
                .FromSpawnable(
                    factory: () => new InputMappingBuilder<InputSource, MenuAction>(),
                    growthStep: 1);

            public static readonly Lazy<InputSourceMapping<MenuAction>> DefaultMapping =  new Lazy<InputSourceMapping<MenuAction>>(
                () => {
                    using (var inputMappingBuilder = InputMappingBuilderPool.Take()) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.Key(KeyCode.UpArrow), MenuAction.Up)
                            .Map(InputSource.Key(KeyCode.DownArrow), MenuAction.Down)
                            .Map(InputSource.Key(KeyCode.LeftArrow), MenuAction.Left)
                            .Map(InputSource.Key(KeyCode.RightArrow), MenuAction.Right)

                            .Map(InputSource.Key(KeyCode.Escape), MenuAction.Pause)
                            .Map(InputSource.Key(KeyCode.Escape), MenuAction.Back)
                            .Map(InputSource.Key(KeyCode.Return), MenuAction.Confirm)

                            .Map(InputSource.Key(KeyCode.F5), MenuAction.RecenterVrHeadset)
                            .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                            .Build()
                            .ToInputSourceMapping();
                    }
                });

            public static readonly Lazy<InputSourceMapping<MenuAction>> DefaultXInputMapping = new Lazy<InputSourceMapping<MenuAction>>(
                () => {
                    using (var inputMappingBuilder = InputMappingBuilderPool.Take()) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickY), MenuAction.Up)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickY), MenuAction.Down)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickX), MenuAction.Left)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickX), MenuAction.Right)

                            .Map(InputSource.XInputButton(XInput.Button.A), MenuAction.Confirm)
                            .Map(InputSource.XInputButton(XInput.Button.Start), MenuAction.Pause)
                            .Map(InputSource.XInputButton(XInput.Button.B), MenuAction.Back)

                            .Map(InputSource.XInputButton(XInput.Button.DPadUp), MenuAction.RecenterVrHeadset)
                            .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                            .Build()
                            .ToInputSourceMapping();
                    }
                });

            public static readonly Lazy<InputSourceMapping<MenuAction>> DefaultXbox360Mapping = new Lazy<InputSourceMapping<MenuAction>>(
                () => {
                    using (var inputMappingBuilder = InputMappingBuilderPool.Take()) {
                        if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.JoystickButton(17), MenuAction.Back)
                                .Map(InputSource.JoystickButton(9), MenuAction.Pause)
                                .Map(InputSource.JoystickButton(16), MenuAction.Confirm)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), MenuAction.Down)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), MenuAction.Up)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), MenuAction.Right)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), MenuAction.Left)

                                .Map(InputSource.JoystickButton(6), MenuAction.RecenterVrHeadset)
                                .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                                .Build()
                                .ToInputSourceMapping();
                        } else {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.JoystickButton(1), MenuAction.Back)
                                .Map(InputSource.JoystickButton(7), MenuAction.Pause)
                                .Map(InputSource.JoystickButton(0), MenuAction.Confirm)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), MenuAction.Down)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), MenuAction.Up)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), MenuAction.Right)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), MenuAction.Left)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 6), MenuAction.RecenterVrHeadset)
                                .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                                .Build()
                                .ToInputSourceMapping();
                        }
                    }
                });

            public static readonly Lazy<InputSourceMapping<MenuAction>> DefaultXboxOneMapping = new Lazy<InputSourceMapping<MenuAction>>(
                () => {
                    using (var inputMappingBuilder = InputMappingBuilderPool.Take()) {
                        if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.JoystickButton(17), MenuAction.Back)
                                .Map(InputSource.JoystickButton(9), MenuAction.Pause)
                                .Map(InputSource.JoystickButton(16), MenuAction.Confirm)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), MenuAction.Down)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), MenuAction.Up)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), MenuAction.Right)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), MenuAction.Left)

                                .Map(InputSource.JoystickButton(6), MenuAction.RecenterVrHeadset)
                                .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                                .Build()
                                .ToInputSourceMapping();
                        } else if(PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.Linux) {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.JoystickButton(1), MenuAction.Back)
                                .Map(InputSource.JoystickButton(7), MenuAction.Pause)
                                .Map(InputSource.JoystickButton(0), MenuAction.Confirm)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), MenuAction.Down)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), MenuAction.Up)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), MenuAction.Right)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), MenuAction.Left)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 7), MenuAction.RecenterVrHeadset)
                                .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                                .Build()
                                .ToInputSourceMapping();
                        } else {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.JoystickButton(1), MenuAction.Back)
                                .Map(InputSource.JoystickButton(7), MenuAction.Pause)
                                .Map(InputSource.JoystickButton(0), MenuAction.Confirm)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), MenuAction.Down)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), MenuAction.Up)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), MenuAction.Right)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), MenuAction.Left)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 6), MenuAction.RecenterVrHeadset)
                                .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                                .Build()
                                .ToInputSourceMapping();
                        }
                    }
                });

            public static readonly Lazy<InputSourceMapping<MenuAction>> DefaultPlaystation4Mapping = new Lazy<InputSourceMapping<MenuAction>>(
                () => {
                    using (var inputMappingBuilder = InputMappingBuilderPool.Take()) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.JoystickButton(2), MenuAction.Back)
                            .Map(InputSource.JoystickButton(9), MenuAction.Pause)
                            .Map(InputSource.JoystickButton(1), MenuAction.Confirm)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), MenuAction.Down)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), MenuAction.Up)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), MenuAction.Right)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), MenuAction.Left)

                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 7), MenuAction.RecenterVrHeadset)
                            .Map(InputSource.Key(KeyCode.F9), MenuAction.TakeScreenshot)

                            .Build()
                            .ToInputSourceMapping();
                    }
                });

            public static Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<MenuAction>>> DefaultControllerMappings = new Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<MenuAction>>>(
                () => new Dictionary<InputDefaults, InputSourceMapping<MenuAction>> {
                    {InputDefaults.KeyboardAndMouse, DefaultMapping.Value},
                    {InputDefaults.KeyboardOnly, DefaultMapping.Value},
                    {InputDefaults.XInput, DefaultXInputMapping.Value},
                    {InputDefaults.Xbox360, DefaultXbox360Mapping.Value},
                    {InputDefaults.XboxOne, DefaultXboxOneMapping.Value},
                    {InputDefaults.SteamController, DefaultXbox360Mapping.Value},
                    {InputDefaults.Playstation4, DefaultPlaystation4Mapping.Value},
                }.ToFastImmutableEnumDictionary());

            public static InputSourceMapping<MenuAction> InitialMapping() {
                if (File.Exists(CustomInputMappingFilePath.Value)) {
                    try {
                        return InputSourceExtensions.DeserializeMapping<MenuAction>(CustomInputMappingFilePath.Value);
                    } catch (Exception e) {
                        Debug.Log(new Exception("Failed to load input mapping due to: " + CustomInputMappingFilePath.Value, e));
                    }
                }
                return DefaultMapping.Value;
            }
        }

        public static class ActionMap {

            private static readonly InputSourceMapping<MenuAction> DefaultMenuMapping =  new InputMappingBuilder<InputSource, MenuAction>()
                .Map(InputSource.Key(KeyCode.UpArrow), MenuAction.Up)
                .Map(InputSource.Key(KeyCode.DownArrow), MenuAction.Down)
                .Map(InputSource.Key(KeyCode.LeftArrow), MenuAction.Left)
                .Map(InputSource.Key(KeyCode.RightArrow), MenuAction.Right)

                .Map(InputSource.Key(KeyCode.Escape), MenuAction.Pause)
                .Map(InputSource.Key(KeyCode.Escape), MenuAction.Back)
                .Map(InputSource.Key(KeyCode.Return), MenuAction.Confirm)
                .Build()
                .ToInputSourceMapping();

            public static MenuActionMap Create(ActionMapConfig<MenuAction> actionMapConfig, float axisButtonThreshold = 0.5f) {
                var inputSettings = actionMapConfig.InputSettings;
                var combinedInputMapping = actionMapConfig.InputMapping.Merge(DefaultMenuMapping);

                var controllerPeripheral = Peripherals.Controller.GetPeripheral(actionMapConfig.ControllerId,
                    inputSettings);

                var buttonInputSource = ImperoCore.MergeAll(
                    Adapters.MergeButtons,
                    Peripherals.Keyboard,
                    Peripherals.Mouse.Buttons,
                    controllerPeripheral.Buttons);

                var axisInputSource = ImperoCore.MergeAll(
                    Adapters.MergeAxes,
                    Peripherals.Mouse.PolarizedAxes,
                    controllerPeripheral.Axes);

                var inputWithAxisSource = combinedInputMapping.ApplyMapping(axisInputSource, Adapters.MergeAxes);
                var inputWithButtonSource = combinedInputMapping.ApplyMapping(buttonInputSource, Adapters.MergeButtons);

                // Finally merge all input maps
                var axisInput = ImperoCore.MergeAll(
                    Adapters.MergeAxes,
                    inputWithAxisSource,
                    inputWithButtonSource.Adapt<MenuAction, ButtonState, float>(Adapters.Button2Axis));
                var buttonInput = ImperoCore.MergeAll(
                    Adapters.MergeButtons,
                    inputWithButtonSource,
                    inputWithAxisSource.Adapt<MenuAction, float, ButtonState>(Adapters.Axis2Button(axisButtonThreshold)));
                buttonInput = buttonInput.FillEmptyValues(MenuActionDetails.MenuActionButtons, () => ButtonState.Released);

                var moveCursor = axisInput.Adapt<MenuAction, float, Vector2>((menuAction, pollFn) => {
                    Func<Vector2> adaptedFn;
                    if (menuAction == MenuAction.Up) {
                        adaptedFn = () => new Vector2(0, pollFn());
                    } else if (menuAction == MenuAction.Down) {
                        adaptedFn = () => new Vector2(0, -pollFn());
                    } else if (menuAction == MenuAction.Left) {
                        adaptedFn = () => new Vector2(-pollFn(), 0);
                    } else if (menuAction == MenuAction.Right) {
                        adaptedFn = () => new Vector2(pollFn(), 0);
                    } else {
                        adaptedFn = () => Vector2.zero;
                    }
                    return adaptedFn;
                });
                var pollCursor = moveCursor.Merge(Adapters.CombineAxes, MenuAction.Up, MenuAction.Down, MenuAction.Left,
                    MenuAction.Right);
                var pollDiscreteCursor = pollCursor.Adapt(Adapters.DiscreteAxisInput());

                return new MenuActionMap(pollCursor, pollDiscreteCursor, buttonInput);
            }
        }
    }

}
