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

    public enum SpectatorAction {
        MoveForward, MoveBackward, MoveLeft, MoveRight, MoveUp, MoveDown,
        SpeedUp,
        LookLeft, LookRight, LookUp, LookDown
    }

    public class SpectatorActionMap {
        private readonly Func<float> _speedUp; 
        private readonly Func<Vector2> _lookDirection;
        private readonly Func<Vector3> _movement;

        public SpectatorActionMap(Func<Vector2> lookDirection, Func<Vector3> movement, Func<float> speedUp) {
            _lookDirection = lookDirection;
            _movement = movement;
            _speedUp = speedUp;
        }

        public Func<Vector2> LookDirection {
            get { return _lookDirection; }
        }

        public Func<Vector3> Movement {
            get { return _movement; }
        }

        public Func<float> SpeedUp {
            get {
                return _speedUp;    
            }
        }
    }

    public static class SpectatorInput {

        public static class Bindings {

            public static readonly IEnumerable<SpectatorAction> MappableActions = Enum.GetValues(typeof (SpectatorAction)).Cast<SpectatorAction>(); 

            private static readonly IObjectPool<InputMappingBuilder<InputSource, SpectatorAction>> InputMapBuilderPool = ObjectPool.FromSpawnable(
                factory: () => new InputMappingBuilder<InputSource, SpectatorAction>(), 
                growthStep: 1);

            public static readonly Lazy<InputSourceMapping<SpectatorAction>> DefaultMapping = new Lazy<InputSourceMapping<SpectatorAction>>(
                () => {
                    using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                        return inputMapBuilder.Instance
                            .Map(InputSource.Key(KeyCode.W), SpectatorAction.MoveForward)
                            .Map(InputSource.Key(KeyCode.S), SpectatorAction.MoveBackward)
                            .Map(InputSource.Key(KeyCode.A), SpectatorAction.MoveLeft)
                            .Map(InputSource.Key(KeyCode.D), SpectatorAction.MoveRight)
                            .Map(InputSource.Key(KeyCode.Q), SpectatorAction.MoveUp)
                            .Map(InputSource.Key(KeyCode.E), SpectatorAction.MoveDown)

                            .Map(InputSource.Key(KeyCode.LeftShift), SpectatorAction.SpeedUp)

                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.Y), SpectatorAction.LookUp)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.Y), SpectatorAction.LookDown)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.X), SpectatorAction.LookLeft)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.X), SpectatorAction.LookRight)
                            .Build()
                            .ToInputSourceMapping();
                    }
            });

            public static readonly Lazy<InputSourceMapping<SpectatorAction>> DefaultXInputMapping = new Lazy<InputSourceMapping<SpectatorAction>>(
                () => {
                    using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickY), SpectatorAction.MoveForward)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickY), SpectatorAction.MoveBackward)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickX), SpectatorAction.MoveLeft)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickX), SpectatorAction.MoveRight)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftTrigger), SpectatorAction.MoveDown)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightTrigger), SpectatorAction.MoveUp)

                            .Map(InputSource.XInputButton(XInput.Button.A), SpectatorAction.SpeedUp)

                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightStickY), SpectatorAction.LookUp)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.RightStickY), SpectatorAction.LookDown)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.RightStickX), SpectatorAction.LookLeft)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightStickX), SpectatorAction.LookRight)

                            .Build()
                            .ToInputSourceMapping();
                    }
                });

            public static readonly Lazy<InputSourceMapping<SpectatorAction>> DefaultXbox360Mapping = new Lazy<InputSourceMapping<SpectatorAction>>(
                () => {
                    using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                        if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                            return inputMapBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), SpectatorAction.MoveForward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), SpectatorAction.MoveBackward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), SpectatorAction.MoveLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), SpectatorAction.MoveRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), SpectatorAction.MoveUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), SpectatorAction.MoveDown)

                                .Map(InputSource.JoystickButton(14), SpectatorAction.SpeedUp)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), SpectatorAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), SpectatorAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), SpectatorAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), SpectatorAction.LookRight)

                                .Build()
                                .ToInputSourceMapping();
                        } else {
                            return inputMapBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), SpectatorAction.MoveForward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), SpectatorAction.MoveBackward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), SpectatorAction.MoveLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), SpectatorAction.MoveRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), SpectatorAction.MoveUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), SpectatorAction.MoveDown)

                                .Map(InputSource.JoystickButton(4), SpectatorAction.SpeedUp)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 4), SpectatorAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), SpectatorAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), SpectatorAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), SpectatorAction.LookRight)

                                .Build()
                                .ToInputSourceMapping();
                        }
                    }
            });

            public static readonly Lazy<InputSourceMapping<SpectatorAction>> DefaultXboxOneMapping = new Lazy<InputSourceMapping<SpectatorAction>>(
                () => {
                    using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                        if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                            return inputMapBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), SpectatorAction.MoveForward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), SpectatorAction.MoveBackward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), SpectatorAction.MoveLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), SpectatorAction.MoveRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), SpectatorAction.MoveUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), SpectatorAction.MoveDown)

                                .Map(InputSource.JoystickButton(14), SpectatorAction.SpeedUp)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), SpectatorAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), SpectatorAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), SpectatorAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), SpectatorAction.LookRight)

                                .Build()
                                .ToInputSourceMapping();
                        } else {
                            return inputMapBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), SpectatorAction.MoveForward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), SpectatorAction.MoveBackward)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), SpectatorAction.MoveLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), SpectatorAction.MoveRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 8), SpectatorAction.MoveUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 9), SpectatorAction.MoveDown)

                                .Map(InputSource.JoystickButton(4), SpectatorAction.SpeedUp)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 4), SpectatorAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), SpectatorAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), SpectatorAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), SpectatorAction.LookRight)

                                .Build()
                                .ToInputSourceMapping();
                        }
                    }
            });

            public static readonly Lazy<InputSourceMapping<SpectatorAction>> DefaultPlaystation4Mapping = new Lazy<InputSourceMapping<SpectatorAction>>(
                () => {
                    using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                        return inputMapBuilder.Instance
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), SpectatorAction.MoveForward)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), SpectatorAction.MoveBackward)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), SpectatorAction.MoveLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), SpectatorAction.MoveRight)
                            .Map(InputSource.JoystickButton(5), SpectatorAction.MoveUp)
                            .Map(InputSource.JoystickButton(4), SpectatorAction.MoveDown)

                            .Map(InputSource.JoystickButton(7), SpectatorAction.SpeedUp)

                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), SpectatorAction.LookUp)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 5), SpectatorAction.LookDown)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), SpectatorAction.LookLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), SpectatorAction.LookRight)

                            .Build()
                            .ToInputSourceMapping();
                    }
            });

            public static Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<SpectatorAction>>> DefaultControllerMappings = new Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<SpectatorAction>>>(
                () => new Dictionary<InputDefaults, InputSourceMapping<SpectatorAction>> {
                    {InputDefaults.KeyboardAndMouse, DefaultMapping.Value},
                    {InputDefaults.KeyboardOnly, DefaultMapping.Value},
                    {InputDefaults.XInput, DefaultXInputMapping.Value},
                    {InputDefaults.Xbox360, DefaultXbox360Mapping.Value},
                    {InputDefaults.XboxOne, DefaultXboxOneMapping.Value},
                    {InputDefaults.SteamController, DefaultXbox360Mapping.Value},
                    {InputDefaults.Playstation4, DefaultPlaystation4Mapping.Value},
                }.ToFastImmutableEnumDictionary());

            public static Lazy<EmptyBinding<SpectatorAction>[]> EmptyBindings = new Lazy<EmptyBinding<SpectatorAction>[]>(
                () => {
                    return MappableActions
                        .Select(action => {
                            var actionStr = EnumUtils.ToPrettyString(action);
                            return new EmptyBinding<SpectatorAction>(action, actionStr, InputType.Axis.ToString());
                        })
                        .ToArray(); 
                });

            public static IEnumerable<InputBindingViewModel> ToBindings(LanguageTable languageTable, 
                InputSourceMapping<SpectatorAction> inputMapping,
                Maybe<UnityInputDeviceProfile> currentControllerProfile) {

                return InputBindingView.ToBindings(languageTable, InputBindingGroup.Spectator, EmptyBindings.Value, 
                    inputMapping, currentControllerProfile);
            }

            public static readonly Lazy<string> CustomInputMappingFilePath = new Lazy<string>(() => {
                return Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "SpectatorInputConfig.json");
            });

            public static InputSourceMapping<SpectatorAction> InitialMapping() {
                if (File.Exists(CustomInputMappingFilePath.Value)) {
                    try {
                        return InputSourceExtensions.DeserializeMapping<SpectatorAction>(CustomInputMappingFilePath.Value);
                    } catch (Exception e) {
                        Debug.LogError(new Exception("Failed to load input mapping: " + CustomInputMappingFilePath.Value, e));
                    }
                }
                return DefaultMapping.Value;
            }
        }

        public static class ActionMap {
            public static SpectatorActionMap Create(ActionMapConfig<SpectatorAction> actionMapConfig, IClock clock) {
                var inputSettings = actionMapConfig.InputSettings;
                var inputMapping = actionMapConfig.InputMapping;

                var controllerPeripheral = Peripherals.Controller.GetPeripheral(actionMapConfig.ControllerId,
                    inputSettings);

                var sourceMap = ImperoCore.MergeAll<InputSource, float>(
                    Adapters.MergeAxes,

                    Peripherals.Mouse.PolarizedAxes
                        .Adapt<InputSource, float, float>(input => input * inputSettings.WingsuitMouseSensitivity),
                    (controllerPeripheral.Axes),

                    Peripherals.Keyboard.Adapt<InputSource, ButtonState, float>(Adapters.Button2Axis),
                    Peripherals.Mouse.Buttons.Adapt<InputSource, ButtonState, float>(Adapters.Button2Axis),
                    controllerPeripheral.Buttons.Adapt<InputSource, ButtonState, float>(Adapters.Button2Axis));

                var inputMap = inputMapping.ApplyMapping(sourceMap, Adapters.MergeAxes);
            
                var moveForward = inputMap.Source.GetValueOrDefault(SpectatorAction.MoveForward, () => 0.0f);
                var moveBackward = inputMap.Source.GetValueOrDefault(SpectatorAction.MoveBackward, () => 0.0f);
                var moveLeft = inputMap.Source.GetValueOrDefault(SpectatorAction.MoveLeft, () => 0.0f);
                var moveRight = inputMap.Source.GetValueOrDefault(SpectatorAction.MoveRight, () => 0.0f);
                var moveUp = inputMap.Source.GetValueOrDefault(SpectatorAction.MoveUp, () => 0.0f);
                var moveDown = inputMap.Source.GetValueOrDefault(SpectatorAction.MoveDown, () => 0.0f);
                Func<Vector3> movement = () => new Vector3 {
                    x = moveRight() - moveLeft(),
                    y = moveUp() - moveDown(),
                    z = moveForward() - moveBackward() };

                var lookUp = inputMap.Source.GetValueOrDefault(SpectatorAction.LookUp, () => 0.0f);
                var lookDown = inputMap.Source.GetValueOrDefault(SpectatorAction.LookDown, () => 0.0f);
                var lookLeft = inputMap.Source.GetValueOrDefault(SpectatorAction.LookLeft, () => 0.0f);
                var lookRight = inputMap.Source.GetValueOrDefault(SpectatorAction.LookRight, () => 0.0f);
                Func<Vector2> lookDirection = () => new Vector2 {
                    x = lookRight() - lookLeft(),
                    y = lookDown() - lookUp() };

                var isSpeedUp = inputMap.Source.GetValueOrDefault(SpectatorAction.SpeedUp, () => 0.0f);

                return new SpectatorActionMap(lookDirection, movement, isSpeedUp);
            }
        }
    }
}
