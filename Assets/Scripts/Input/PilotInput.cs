using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using InControl;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public enum WingsuitAction {
        Respawn, ToStartSelection, UnfoldParachute,

        // Player Movement
        Cannonball, CloseArms, CloseLeftArm, CloseRightArm,
        PitchUp, PitchDown, RollLeft, RollRight, YawLeft, YawRight,

        // Camera Movement
        LookUp, LookDown, LookLeft, LookRight, ChangeCamera,

        // Combined movement
        Pitch, Roll, Yaw, LookVertical, LookHorizontal,

        ActivateSlowMo,
        ActivateMouseLook, ToggleSpectatorView
    }


    public class PilotActionMap {

        public static readonly PilotActionMap NoOpActionMap = PilotInput.ActionMap.Create(
            new ActionMapConfig<WingsuitAction> {
                ControllerId = null,
                InputMapping = InputSourceMapping<WingsuitAction>.Empty,
                InputSettings = InputSettings.Default
            },
            NoOpClock.Default);

        private readonly InputMap<WingsuitAction, ButtonState> _buttons;
        private readonly InputMap<WingsuitAction, ButtonEvent> _buttonEvents;

        private readonly InputMap<WingsuitAction, float> _mouseAxes;
        private readonly InputMap<WingsuitAction, float> _axes;

        public PilotActionMap(InputMap<WingsuitAction, ButtonState> buttons, InputMap<WingsuitAction, float> axes, InputMap<WingsuitAction, float> mouseAxes) {
            _buttons = buttons.Optimize();
            _buttonEvents = _buttons
                .Adapt<WingsuitAction, ButtonState, ButtonEvent>(() => Adapters.ButtonEvents(Clock.FrameCounter))
                .Optimize();

            _axes = axes.Optimize();
            _mouseAxes = mouseAxes.Optimize();
        }

        public ButtonState PollButton(WingsuitAction id) {
            return _buttons.Poll(id);
        }

        public float PollAxis(WingsuitAction id) {
            return _axes.Poll(id);
        }

        public float PollMouseAxis(WingsuitAction id) {
            return _mouseAxes.Poll(id);
        }

        public ButtonEvent PollButtonEvent(WingsuitAction id) {
            return _buttonEvents.Poll(id);
        }
    }

    public static class PilotInput {

        public static class Bindings {
            public static readonly Lazy<string> CustomInputMappingFilePath = new Lazy<string>(() => {
                return Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "PlayerInputConfig.json");
            });
            public static readonly EmptyBinding<WingsuitAction>[] EmptyBindings;

            static Bindings() {
                EmptyBindings = Enumerable.Concat(
                    PilotActionDetails.MappableButtons.Select(
                        playerAction => new EmptyBinding<WingsuitAction>(playerAction, EnumUtils.ToPrettyString(playerAction), InputType.Button.ToString())),
                    PilotActionDetails.MappableAxes.Select(
                        playerAction => new EmptyBinding<WingsuitAction>(playerAction, EnumUtils.ToPrettyString(playerAction), InputType.Axis.ToString())))
                    .ToArray();
            }

            public static IEnumerable<InputBindingViewModel> ToBindings(LanguageTable languageTable, 
                InputSourceMapping<WingsuitAction> inputMapping,
                Maybe<UnityInputDeviceProfile> currentControllerProfile) {
                return InputBindingView.ToBindings(languageTable, InputBindingGroup.Wingsuit, EmptyBindings, inputMapping, 
                    currentControllerProfile);
            }

            public static InputSourceMapping<WingsuitAction> InitialMapping() {
                if (File.Exists(CustomInputMappingFilePath.Value)) {
                    try {
                        return InputSourceExtensions.DeserializeMapping<WingsuitAction>(CustomInputMappingFilePath.Value);
                    } catch (Exception e) {
                        Debug.LogError(new Exception("Failed to load input mapping: " + CustomInputMappingFilePath.Value, e));
                    }
                }
                return DefaultMapping.Value;
            }

            private static readonly IObjectPool<InputMappingBuilder<InputSource, WingsuitAction>> InputMapBuilderPool = ObjectPool.FromSpawnable(
                factory: () => new InputMappingBuilder<InputSource, WingsuitAction>(),
                growthStep: 1);

            public static readonly Lazy<InputSourceMapping<WingsuitAction>> DefaultMapping = new Lazy<InputSourceMapping<WingsuitAction>>(
                () => {
                    using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                        return inputMapBuilder.Instance
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.Y),
                                WingsuitAction.PitchUp)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.Y),
                                WingsuitAction.PitchDown)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.X),
                                WingsuitAction.RollLeft)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.X),
                                WingsuitAction.RollRight)
                            .Map(InputSource.Key(KeyCode.Q), WingsuitAction.YawLeft)
                            .Map(InputSource.Key(KeyCode.E), WingsuitAction.YawRight)

                            .Map(InputSource.Key(KeyCode.F), WingsuitAction.ActivateSlowMo)

                            .Map(InputSource.Key(KeyCode.S), WingsuitAction.Cannonball)
                            .Map(InputSource.Key(KeyCode.W), WingsuitAction.CloseArms)
                            .Map(InputSource.Key(KeyCode.A), WingsuitAction.CloseLeftArm)
                            .Map(InputSource.Key(KeyCode.D), WingsuitAction.CloseRightArm)

                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.Y),
                                WingsuitAction.LookUp)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.Y),
                                WingsuitAction.LookDown)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.X),
                                WingsuitAction.LookLeft)
                            .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.X),
                                WingsuitAction.LookRight)
                            .Map(InputSource.MouseButton(MouseButtonId.Left), WingsuitAction.ActivateMouseLook)
                            .Map(InputSource.Key(KeyCode.T), WingsuitAction.UnfoldParachute)
                            .Map(InputSource.MouseButton(MouseButtonId.Middle), WingsuitAction.ChangeCamera)

                            .Map(InputSource.Key(KeyCode.R), WingsuitAction.Respawn)
                            .Map(InputSource.Key(KeyCode.B), WingsuitAction.ToStartSelection)
                            .Map(InputSource.Key(KeyCode.F3), WingsuitAction.ToggleSpectatorView)
                            .Build()
                            .ToInputSourceMapping();
                    }
            });

            public static readonly Lazy<InputSourceMapping<WingsuitAction>> DefaultKeyboardOnlyMapping = new Lazy<InputSourceMapping<WingsuitAction>>(
                () => {
                    using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                        return inputMapBuilder.Instance
                            .Map(InputSource.Key(KeyCode.W), WingsuitAction.PitchDown)
                            .Map(InputSource.Key(KeyCode.S), WingsuitAction.PitchUp)
                            .Map(InputSource.Key(KeyCode.A), WingsuitAction.RollLeft)
                            .Map(InputSource.Key(KeyCode.D), WingsuitAction.RollRight)
                            .Map(InputSource.Key(KeyCode.Q), WingsuitAction.YawLeft)
                            .Map(InputSource.Key(KeyCode.E), WingsuitAction.YawRight)

                            .Map(InputSource.Key(KeyCode.F), WingsuitAction.ActivateSlowMo)

                            .Map(InputSource.Key(KeyCode.UpArrow), WingsuitAction.Cannonball)
                            .Map(InputSource.Key(KeyCode.DownArrow), WingsuitAction.CloseArms)
                            .Map(InputSource.Key(KeyCode.LeftArrow), WingsuitAction.CloseLeftArm)
                            .Map(InputSource.Key(KeyCode.RightArrow), WingsuitAction.CloseRightArm)

                            .Map(InputSource.Key(KeyCode.I), WingsuitAction.LookUp)
                            .Map(InputSource.Key(KeyCode.K), WingsuitAction.LookDown)
                            .Map(InputSource.Key(KeyCode.J), WingsuitAction.LookLeft)
                            .Map(InputSource.Key(KeyCode.L), WingsuitAction.LookRight)

                            .Map(InputSource.Key(KeyCode.T), WingsuitAction.UnfoldParachute)

                            .Map(InputSource.Key(KeyCode.R), WingsuitAction.Respawn)
                            .Map(InputSource.Key(KeyCode.B), WingsuitAction.ToStartSelection)
                            .Map(InputSource.Key(KeyCode.C), WingsuitAction.ChangeCamera)
                            .Map(InputSource.Key(KeyCode.F3), WingsuitAction.ToggleSpectatorView)
                            .Build()
                            .ToInputSourceMapping();
                    }});

            public static readonly Lazy<InputSourceMapping<WingsuitAction>> DefaultXInputMapping = new Lazy<InputSourceMapping<WingsuitAction>>(
                () => {
                    using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickY), WingsuitAction.PitchUp)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickY), WingsuitAction.PitchDown)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickX), WingsuitAction.RollLeft)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickX), WingsuitAction.RollRight)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftTrigger), WingsuitAction.YawLeft)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightTrigger), WingsuitAction.YawRight)

                            .Map(InputSource.XInputButton(XInput.Button.X), WingsuitAction.CloseLeftArm)
                            .Map(InputSource.XInputButton(XInput.Button.B), WingsuitAction.CloseRightArm)
                            .Map(InputSource.XInputButton(XInput.Button.Y), WingsuitAction.CloseArms)
                            .Map(InputSource.XInputButton(XInput.Button.A), WingsuitAction.Cannonball)

                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightStickY), WingsuitAction.LookUp)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.RightStickY), WingsuitAction.LookDown)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.RightStickX), WingsuitAction.LookLeft)
                            .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightStickX), WingsuitAction.LookRight)
                            .Map(InputSource.XInputButton(XInput.Button.RightStick), WingsuitAction.ChangeCamera)
                            .Map(InputSource.XInputButton(XInput.Button.LeftShoulder), WingsuitAction.ActivateSlowMo)
                            .Map(InputSource.XInputButton(XInput.Button.RightShoulder), WingsuitAction.UnfoldParachute)

                            .Map(InputSource.XInputButton(XInput.Button.Back), WingsuitAction.Respawn)
                            .Map(InputSource.XInputButton(XInput.Button.DPadRight), WingsuitAction.ToggleSpectatorView)
                            .Map(InputSource.XInputButton(XInput.Button.DPadDown), WingsuitAction.ToStartSelection)
                            .Build()
                            .ToInputSourceMapping();
                    }
                });

            public static readonly Lazy<InputSourceMapping<WingsuitAction>> DefaultXbox360Mapping = new Lazy<InputSourceMapping<WingsuitAction>>(
                () => {
                    using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                        if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), WingsuitAction.PitchUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), WingsuitAction.PitchDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), WingsuitAction.RollLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), WingsuitAction.RollRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), WingsuitAction.YawLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), WingsuitAction.YawRight)

                                .Map(InputSource.JoystickButton(18), WingsuitAction.CloseLeftArm)
                                .Map(InputSource.JoystickButton(17), WingsuitAction.CloseRightArm)
                                .Map(InputSource.JoystickButton(19), WingsuitAction.CloseArms)
                                .Map(InputSource.JoystickButton(16), WingsuitAction.Cannonball)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), WingsuitAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), WingsuitAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), WingsuitAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), WingsuitAction.LookRight)
                                .Map(InputSource.JoystickButton(12), WingsuitAction.ChangeCamera)

                                .Map(InputSource.JoystickButton(13), WingsuitAction.ActivateSlowMo)
                                .Map(InputSource.JoystickButton(14), WingsuitAction.UnfoldParachute)

                                .Map(InputSource.JoystickButton(10), WingsuitAction.Respawn)
                                .Map(InputSource.JoystickButton(7), WingsuitAction.ToStartSelection)
                                .Build()
                                .ToInputSourceMapping();
                        } else if(PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.Linux) {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), WingsuitAction.PitchUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), WingsuitAction.PitchDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), WingsuitAction.RollLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), WingsuitAction.RollRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), WingsuitAction.YawLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), WingsuitAction.YawRight)

                                .Map(InputSource.JoystickButton(2), WingsuitAction.CloseLeftArm)
                                .Map(InputSource.JoystickButton(1), WingsuitAction.CloseRightArm)
                                .Map(InputSource.JoystickButton(3), WingsuitAction.CloseArms)
                                .Map(InputSource.JoystickButton(0), WingsuitAction.Cannonball)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 4), WingsuitAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), WingsuitAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), WingsuitAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), WingsuitAction.LookRight)
                                .Map(InputSource.JoystickButton(10), WingsuitAction.ChangeCamera)
                                .Map(InputSource.JoystickButton(4), WingsuitAction.UnfoldParachute)

                                .Map(InputSource.JoystickButton(6), WingsuitAction.Respawn)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 7), WingsuitAction.ToStartSelection)
                                .Build()
                                .ToInputSourceMapping();
                        } else {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), WingsuitAction.PitchUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), WingsuitAction.PitchDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), WingsuitAction.RollLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), WingsuitAction.RollRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), WingsuitAction.YawLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), WingsuitAction.YawRight)

                                .Map(InputSource.JoystickButton(2), WingsuitAction.CloseLeftArm)
                                .Map(InputSource.JoystickButton(1), WingsuitAction.CloseRightArm)
                                .Map(InputSource.JoystickButton(3), WingsuitAction.CloseArms)
                                .Map(InputSource.JoystickButton(0), WingsuitAction.Cannonball)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 4), WingsuitAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), WingsuitAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), WingsuitAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), WingsuitAction.LookRight)
                                .Map(InputSource.JoystickButton(9), WingsuitAction.ChangeCamera)
                                .Map(InputSource.JoystickButton(4), WingsuitAction.UnfoldParachute)

                                .Map(InputSource.JoystickButton(6), WingsuitAction.Respawn)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 5), WingsuitAction.ToStartSelection)
                                .Build()
                                .ToInputSourceMapping();
                        }
                    }
                });

            public static readonly Lazy<InputSourceMapping<WingsuitAction>> DefaultXboxOneMapping = new Lazy<InputSourceMapping<WingsuitAction>>(
                () => {
                    using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                        if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), WingsuitAction.PitchUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), WingsuitAction.PitchDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), WingsuitAction.RollLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), WingsuitAction.RollRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), WingsuitAction.YawLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), WingsuitAction.YawRight)

                                .Map(InputSource.JoystickButton(18), WingsuitAction.CloseLeftArm)
                                .Map(InputSource.JoystickButton(17), WingsuitAction.CloseRightArm)
                                .Map(InputSource.JoystickButton(19), WingsuitAction.CloseArms)
                                .Map(InputSource.JoystickButton(16), WingsuitAction.Cannonball)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), WingsuitAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), WingsuitAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), WingsuitAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), WingsuitAction.LookRight)
                                .Map(InputSource.JoystickButton(12), WingsuitAction.ChangeCamera)
                                .Map(InputSource.JoystickButton(13), WingsuitAction.ActivateSlowMo)
                                .Map(InputSource.JoystickButton(14), WingsuitAction.UnfoldParachute)

                                .Map(InputSource.JoystickButton(10), WingsuitAction.Respawn)
                                .Map(InputSource.JoystickButton(7), WingsuitAction.ToStartSelection)
                                .Build()
                                .ToInputSourceMapping();
                        } else {
                            return inputMappingBuilder.Instance
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), WingsuitAction.PitchUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), WingsuitAction.PitchDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), WingsuitAction.RollLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), WingsuitAction.RollRight)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 8), WingsuitAction.YawLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 9), WingsuitAction.YawRight)

                                .Map(InputSource.JoystickButton(2), WingsuitAction.CloseLeftArm)
                                .Map(InputSource.JoystickButton(1), WingsuitAction.CloseRightArm)
                                .Map(InputSource.JoystickButton(3), WingsuitAction.CloseArms)
                                .Map(InputSource.JoystickButton(0), WingsuitAction.Cannonball)

                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 4), WingsuitAction.LookUp)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), WingsuitAction.LookDown)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 3), WingsuitAction.LookLeft)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 3), WingsuitAction.LookRight)
                                .Map(InputSource.JoystickButton(9), WingsuitAction.ChangeCamera)
                                .Map(InputSource.JoystickButton(4), WingsuitAction.UnfoldParachute)

                                .Map(InputSource.JoystickButton(6), WingsuitAction.Respawn)
                                .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 5), WingsuitAction.ToStartSelection)
                                .Build()
                                .ToInputSourceMapping();
                        }
                    }
                });

            public static readonly Lazy<InputSourceMapping<WingsuitAction>> DefaultPlaystation4Mapping = new Lazy<InputSourceMapping<WingsuitAction>>(
                () => {
                    using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), WingsuitAction.PitchUp)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), WingsuitAction.PitchDown)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), WingsuitAction.RollLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), WingsuitAction.RollRight)
                            .Map(InputSource.JoystickButton(6), WingsuitAction.YawLeft)
                            .Map(InputSource.JoystickButton(7), WingsuitAction.YawRight)

                            .Map(InputSource.JoystickButton(0), WingsuitAction.CloseLeftArm)
                            .Map(InputSource.JoystickButton(2), WingsuitAction.CloseRightArm)
                            .Map(InputSource.JoystickButton(3), WingsuitAction.CloseArms)
                            .Map(InputSource.JoystickButton(1), WingsuitAction.Cannonball)

                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 5), WingsuitAction.LookUp)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), WingsuitAction.LookDown)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), WingsuitAction.LookLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), WingsuitAction.LookRight)
                            .Map(InputSource.JoystickButton(11), WingsuitAction.ChangeCamera)
                            .Map(InputSource.JoystickButton(5), WingsuitAction.UnfoldParachute)

                            .Map(InputSource.JoystickButton(8), WingsuitAction.Respawn)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 7), WingsuitAction.ToStartSelection)
                            .Build()
                            .ToInputSourceMapping();
                    }
                });

            public static Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<WingsuitAction>>> DefaultControllerMappings = new Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<WingsuitAction>>>(
                () => new Dictionary<InputDefaults, InputSourceMapping<WingsuitAction>> {
                    {InputDefaults.KeyboardAndMouse, DefaultMapping.Value},
                    {InputDefaults.KeyboardOnly, DefaultKeyboardOnlyMapping.Value},
                    {InputDefaults.XInput, DefaultXInputMapping.Value},
                    {InputDefaults.Xbox360, DefaultXbox360Mapping.Value},
                    {InputDefaults.XboxOne, DefaultXboxOneMapping.Value},
                    {InputDefaults.SteamController, DefaultXbox360Mapping.Value},
                    {InputDefaults.Playstation4, DefaultPlaystation4Mapping.Value},
                }.ToFastImmutableEnumDictionary());
        }


        public static class PilotActionDetails {
            public static readonly IDictionary<WingsuitAction, Derivable<WingsuitAction>> DerivableActions =
                new Dictionary<WingsuitAction, Derivable<WingsuitAction>> {
                {WingsuitAction.Pitch, new Derivable<WingsuitAction>(WingsuitAction.PitchUp, WingsuitAction.PitchDown)},
                {WingsuitAction.Roll, new Derivable<WingsuitAction>(WingsuitAction.RollLeft, WingsuitAction.RollRight)},
                {WingsuitAction.Yaw, new Derivable<WingsuitAction>(WingsuitAction.YawLeft, WingsuitAction.YawRight)},
                {WingsuitAction.LookVertical, new Derivable<WingsuitAction>(WingsuitAction.LookUp, WingsuitAction.LookDown)},
                {WingsuitAction.LookHorizontal, new Derivable<WingsuitAction>(WingsuitAction.LookLeft, WingsuitAction.LookRight)},
            };

            public static readonly IEnumerable<WingsuitAction> AxisActions = new [] {
                WingsuitAction.Cannonball, WingsuitAction.CloseArms,
                WingsuitAction.CloseLeftArm, WingsuitAction.CloseRightArm,
                WingsuitAction.PitchUp, WingsuitAction.PitchDown,
                WingsuitAction.RollLeft, WingsuitAction.RollRight,
                WingsuitAction.YawLeft, WingsuitAction.YawRight,
                WingsuitAction.LookUp, WingsuitAction.LookDown,
                WingsuitAction.LookLeft, WingsuitAction.LookRight,
                WingsuitAction.Pitch, WingsuitAction.Roll, WingsuitAction.Yaw,
                WingsuitAction.LookVertical, WingsuitAction.LookHorizontal
            };
            public static readonly IEnumerable<WingsuitAction> ButtonActions = new [] {
                WingsuitAction.Respawn, WingsuitAction.ToStartSelection, WingsuitAction.UnfoldParachute,
                WingsuitAction.ChangeCamera, WingsuitAction.ActivateSlowMo,
                WingsuitAction.ActivateMouseLook, WingsuitAction.ToggleSpectatorView,
            };

            public static readonly IEnumerable<WingsuitAction> MappableButtons = ButtonActions;
            public static readonly IEnumerable<WingsuitAction> MappableAxes = new [] {
                WingsuitAction.Cannonball, WingsuitAction.CloseArms,
                WingsuitAction.CloseLeftArm, WingsuitAction.CloseRightArm,
                WingsuitAction.PitchUp, WingsuitAction.PitchDown,
                WingsuitAction.RollLeft, WingsuitAction.RollRight,
                WingsuitAction.YawLeft, WingsuitAction.YawRight,
                WingsuitAction.LookUp, WingsuitAction.LookDown,
                WingsuitAction.LookLeft, WingsuitAction.LookRight,
            };
            public static readonly IEnumerable<WingsuitAction> MappableActions = MappableButtons.Concat(MappableAxes);

            public class Derivable<TAction> {
                private readonly TAction _negative;
                private readonly TAction _positive;

                public Derivable(TAction negative, TAction positive) {
                    _negative = negative;
                    _positive = positive;
                }

                public TAction Positive {
                    get { return _positive; }
                }

                public TAction Negative {
                    get { return _negative;  }
                }
            }
        }

        public static class ActionMap {
            private static readonly IEnumerable<WingsuitAction> PilotBodyMovementActions = new[] {
                WingsuitAction.PitchDown, WingsuitAction.PitchUp,
                WingsuitAction.RollLeft, WingsuitAction.RollRight };

            private static readonly IEnumerable<WingsuitAction> PilotArmMovementActions = new[] {
                WingsuitAction.CloseArms, WingsuitAction.CloseLeftArm,
                WingsuitAction.CloseRightArm};

            public static PilotActionMap Create(ActionMapConfig<WingsuitAction> actionMapConfig, IClock clock) {
                var inputSettings = actionMapConfig.InputSettings;
                var inputMapping = actionMapConfig.InputMapping;

                var controllerPeripheral = Peripherals.Controller.GetPeripheral(actionMapConfig.ControllerId,
                    inputSettings);

                var buttonSourceMap = ImperoCore.MergeAll(
                    Adapters.MergeButtons,
                    Peripherals.Keyboard,
                    Peripherals.Mouse.Buttons,
                    controllerPeripheral.Buttons);

                var inputWithButtonSource = inputMapping.ApplyMapping(buttonSourceMap, Adapters.MergeButtons);
                var inputWithControllerAxisSource = CreateControllerAxisInput(inputMapping, inputSettings, controllerPeripheral);

                var mouseAxisSourceMap = Peripherals.Mouse.PolarizedAxes;
                var inputWithMouseAxisSource = inputMapping.ApplyMapping(mouseAxisSourceMap, Adapters.MergeAxes);
                inputWithMouseAxisSource = AddMouseLook(
                    inputWithButtonSource.Source.GetValueOrDefault(WingsuitAction.ActivateMouseLook, () => ButtonState.Released)
                        .Adapt<ButtonState, bool>(Adapters.ButtonState2Bool),
                    inputWithMouseAxisSource,
                    inputSettings);
                inputWithMouseAxisSource = AddMousePlayerControls(
                    inputWithButtonSource.Source.GetValueOrDefault(WingsuitAction.ActivateMouseLook, () => ButtonState.Released)
                        .Adapt<ButtonState, bool>(Adapters.ButtonState2Bool)
                        .Adapt<bool, bool>(Adapters.Invert),
                    inputWithMouseAxisSource,
                    inputSettings);

                // Separate this from all other input, since we have no logical way to convert a mouse axis to
                // a stick-like axis (an axis with a pivot point)
                var allMouseInput = ImperoCore.MergeAll(
                    Adapters.MergeAxes,
                    inputWithMouseAxisSource,
                    DerivableAxisInput(inputWithMouseAxisSource));

                inputWithMouseAxisSource = inputWithMouseAxisSource.Filter(pilotAction => !PilotBodyMovementActions.Contains(pilotAction));

                var inputWithAxisSource = ImperoCore.MergeAll(Adapters.MergeAxes, inputWithControllerAxisSource, inputWithMouseAxisSource);

                var allAxisInput = ImperoCore.MergeAll(
                    Adapters.MergeAxes,
                    inputWithAxisSource,
                    ButtonsAsAxes(inputWithButtonSource, clock));

                var derivedInput = DerivableAxisInput(allAxisInput);

                // Add the derived input
                allAxisInput = ImperoCore.MergeAll(Adapters.MergeAxes, allAxisInput, derivedInput);
                var allButtonInput = ImperoCore.MergeAll(
                    Adapters.MergeButtons,
                    inputWithButtonSource,
                    inputWithAxisSource.Adapt(Adapters.Axis2Button(threshold: 0.5f)),
                    ImperoCore.Adapt(derivedInput, Adapters.Axis2Button(threshold: 0.5f)));

                allAxisInput = allAxisInput.FillEmptyValues(PilotActionDetails.MappableActions,
                    () => 0.0f);
                allMouseInput = allMouseInput.FillEmptyValues(PilotActionDetails.MappableActions,
                    () => 0.0f);
                allButtonInput = allButtonInput.FillEmptyValues(PilotActionDetails.MappableActions,
                    () => ButtonState.Released);

//                var respawnHoldDuration = (0.5f).Seconds();
//                Func<int> currentFrame = () => (int)clock.FrameCount;
//                var respawn = allButtonInput.Source[PilotAction.Respawn];
//                allButtonInput = allButtonInput.Update(
//                    PilotAction.Respawn, 
//                    respawn.Adapt(Adapters.MinHoldDuration(new Func<TimeSpan>(clock.PollDeltaTime), respawnHoldDuration)).Cache(currentFrame));
//
//                Func<TimeSpan> currentTime = () => TimeSpan.FromSeconds(clock.CurrentTime);
//                allButtonInput = allButtonInput.Update(
//                    PilotAction.SwitchVehicleType, 
//                    respawn.Adapt(Adapters.MaxHoldDuration(currentTime, respawnHoldDuration)).Cache(currentFrame));

                return new PilotActionMap(allButtonInput, allAxisInput, allMouseInput);
            }

            private static InputMap<WingsuitAction, float> ButtonsAsAxes(InputMap<WingsuitAction, ButtonState> inputWithButtonSource, IClock clock) {
                return inputWithButtonSource
                    .Adapt<WingsuitAction, ButtonState, float>((id, pollFn) => {
                        if (PilotActionDetails.MappableAxes.Contains(id)) {
                            Func<ButtonState, float> adapter;
                            if (PilotBodyMovementActions.Contains(id)) {
                                adapter = Adapters.Button2AccumulatedAxis(speed: 1, gravity: 1, deltaTime: () => clock.DeltaTime);
                            } else {
                                adapter = Adapters.Button2AccumulatedAxis(speed: 4, gravity: 4, deltaTime: () => clock.DeltaTime);
                            }
                            return pollFn.Adapt(adapter).Cache(() => clock.FrameCount);
                        }
                        return pollFn.Adapt<ButtonState, float>(Adapters.Button2Axis);
                    });
            }

            private static InputMap<WingsuitAction, float> DerivableAxisInput(InputMap<WingsuitAction, float> axisInput) {
                var derivedInput = ImmutableDictionary<WingsuitAction, Func<float>>.Empty;

                var closeArms = axisInput.Source.GetValueOrDefault(WingsuitAction.CloseArms, () => 0.0f);
                var closeLeftArm = axisInput.Source.GetValueOrDefault(WingsuitAction.CloseLeftArm, () => 0.0f);
                var closeRightArm = axisInput.Source.GetValueOrDefault(WingsuitAction.CloseRightArm, () => 0.0f);
                derivedInput = derivedInput
                    .SetItem(WingsuitAction.CloseLeftArm, ImperoCore.MergePollFns(Adapters.MergeAxes, new[] {closeArms, closeLeftArm}));
                derivedInput = derivedInput
                    .SetItem(WingsuitAction.CloseRightArm, ImperoCore.MergePollFns(Adapters.MergeAxes, new[] {closeArms, closeRightArm}));

                foreach (var derivable in PilotActionDetails.DerivableActions) {
                    var positiveAction = axisInput.Source
                        .GetValueOrDefault(derivable.Value.Positive, () => 0.0f);
                    var negativeAction = axisInput.Source
                        .GetValueOrDefault(derivable.Value.Negative, () => 0.0f)
                        .Adapt<float, float>(Adapters.Invert);
                    derivedInput = derivedInput.SetItem(derivable.Key, ImperoCore.MergePollFns(Adapters.MergeAxes,
                        new[] { negativeAction, positiveAction }));
                }

                return derivedInput.ToInputMap();
            }

            private static InputMap<WingsuitAction, float> CreateControllerAxisInput(
                InputSourceMapping<WingsuitAction> inputMapping, InputSettings inputSettings, Peripherals.ControllerPeripheral peripheral) {

                var controllerAxisSourceMap = peripheral.Axes;

                var inputWithJoystickAxisSource = inputMapping.ApplyMapping(controllerAxisSourceMap, Adapters.MergeAxes);
                // Circularize input of pitch and roll if both are mapped to a joystick
                if (new [] {WingsuitAction.PitchUp, WingsuitAction.PitchDown, WingsuitAction.RollLeft, WingsuitAction.RollRight}.All(inputWithJoystickAxisSource.Keys.Contains)) {
                    var pitchUp = inputWithJoystickAxisSource.Source[WingsuitAction.PitchUp];
                    var pitchDown = inputWithJoystickAxisSource.Source[WingsuitAction.PitchDown];
                    var rollLeft = inputWithJoystickAxisSource.Source[WingsuitAction.RollLeft];
                    var rollRight = inputWithJoystickAxisSource.Source[WingsuitAction.RollRight];
                    Func<Vector2> circularizedInput = () => InputUtilities.CircularizeInput(new Vector2(rollRight() - rollLeft(), pitchUp() - pitchDown()));
                    inputWithJoystickAxisSource = inputWithJoystickAxisSource.Source
                        .SetItem(WingsuitAction.PitchUp, () => Adapters.FilterPositiveInput(circularizedInput().y))
                        .SetItem(WingsuitAction.PitchDown, () => Adapters.Abs(Adapters.FilterNegativeInput(circularizedInput().y)))
                        .SetItem(WingsuitAction.RollLeft, () => Adapters.Abs(Adapters.FilterNegativeInput(circularizedInput().x)))
                        .SetItem(WingsuitAction.RollRight, () => Adapters.FilterPositiveInput(circularizedInput().x))
                        .ToInputMap();
                }
                // Scale all pitch, roll and yaw input of a joystick
                inputWithJoystickAxisSource = inputWithJoystickAxisSource.Adapt<WingsuitAction, float, float>((id, pollFn) => {
                    if (PilotBodyMovementActions.Contains(id)) {
                        return pollFn.Adapt<float, float>(i => MathUtils.ScaleQuadratically(i, inputSettings.InputGamma));
                    } else if (PilotArmMovementActions.Contains(id)) {
                        return pollFn.Adapt<float, float>(i => MathUtils.ScaleQuadratically(i, 2));
                    }
                    return pollFn;
                });
                return inputWithJoystickAxisSource;
            }

            private static InputMap<WingsuitAction, float> AddMousePlayerControls(Func<bool> whenActive,
                InputMap<WingsuitAction, float> mouseInput, InputSettings inputSettings) {
                var adaptedInput = mouseInput.Source;
                Action<WingsuitAction> resetOnFalse = playerAction => {
                    Func<float> pollFn;
                    adaptedInput.TryGetValue(playerAction, out pollFn);
                    if (pollFn != null) {
                        var pollInput = pollFn
                            .Adapt<float, float>(WithDefault(whenActive, 0.0f))
                            .Adapt<float, float>(ApplyMouseSensitivity(inputSettings.WingsuitMouseSensitivity));
                        adaptedInput = adaptedInput.SetItem(playerAction, pollInput);
                    }
                };

                resetOnFalse(WingsuitAction.PitchUp);
                resetOnFalse(WingsuitAction.PitchDown);
                resetOnFalse(WingsuitAction.RollLeft);
                resetOnFalse(WingsuitAction.RollRight);
                return adaptedInput.ToInputMap();
            }

            private static InputMap<WingsuitAction, float> AddMouseLook(
                Func<bool> whenActive, InputMap<WingsuitAction, float> mouseInput, InputSettings inputSettings) {
                var adaptedInput = mouseInput.Source;
                Action<WingsuitAction> activeOn = playerAction => {
                    Func<float> pollFn;
                    adaptedInput.TryGetValue(playerAction, out pollFn);
                    if (pollFn != null) {
                        var pollInput = pollFn
                            .Adapt<float, float>(WithDefault(whenActive, 0.0f))
                            .Adapt<float, float>(ApplyMouseSensitivity(inputSettings.WingsuitMouseSensitivity));
                        adaptedInput = adaptedInput.SetItem(playerAction, pollInput);
                    }
                };

                activeOn(WingsuitAction.LookLeft);
                activeOn(WingsuitAction.LookUp);
                activeOn(WingsuitAction.LookDown);
                activeOn(WingsuitAction.LookRight);
                return adaptedInput.ToInputMap();
            }

            private static Func<float, float> ApplyMouseSensitivity(float sensitivity) {
                return value => value / 5 * sensitivity;
            }

            private static Func<T, T> WithDefault<T>(Func<bool> predicate, T defaultValue) {
                return value => predicate() ? value : defaultValue;
            }
        }
    }


}
