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
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public enum ParachuteAction {
        //ParachuteConfigToggle,

        WeightShiftLeft,
        WeightShiftRight,
        WeightShiftFront,
        WeightShiftBack,

        // Hold-line mode
        PullLeftLines, PullRightLines,
        PullBothLines,
        HoldFrontLines, HoldRearLines,

        // Direct-line mode
        PullFrontLineLeft, PullFrontLineRight, 
        PullRearLineLeft, PullRearLineRight, 
        PullBrakeLineLeft, PullBrakeLineRight, 
    }

    public static class ParachuteActionDetails {
        public static readonly IEnumerable<ParachuteAction> MappableButtons = new [] {
            ParachuteAction.HoldFrontLines, ParachuteAction.HoldRearLines, };
        public static readonly IEnumerable<ParachuteAction> MappableAxes = new [] {
            ParachuteAction.WeightShiftLeft, ParachuteAction.WeightShiftRight,
            ParachuteAction.WeightShiftFront, ParachuteAction.WeightShiftBack,
            ParachuteAction.PullLeftLines, ParachuteAction.PullRightLines,
            ParachuteAction.PullBothLines, 
            ParachuteAction.PullFrontLineLeft, ParachuteAction.PullFrontLineRight,
            ParachuteAction.PullRearLineLeft, ParachuteAction.PullRearLineRight,
            ParachuteAction.PullBrakeLineLeft, ParachuteAction.PullBrakeLineRight,
        };
        public static readonly IEnumerable<ParachuteAction> MappableActions = MappableAxes.Concat(MappableButtons);
    }

    public class ParachuteActionMap : IParachuteActionMap {

        private readonly Func<ParachuteInput> _parachuteInput;
        private readonly Func<ButtonEvent> _parachuteConfigToggle;

        public ParachuteActionMap(Func<ParachuteInput> parachuteInput, Func<ButtonEvent> parachuteConfigToggle) {

            _parachuteConfigToggle = parachuteConfigToggle;
            _parachuteInput = parachuteInput;
        }

        public ButtonEvent ParachuteConfigToggle { get { return _parachuteConfigToggle(); } }
        public ParachuteInput Input { get { return _parachuteInput(); } }
    }

    public static class ParachuteControls {

        private static readonly IObjectPool<InputMappingBuilder<InputSource, ParachuteAction>> InputMapBuilderPool = ObjectPool.FromSpawnable(
            factory: () => new InputMappingBuilder<InputSource, ParachuteAction>(),
            growthStep: 1);

        public static readonly Lazy<string> CustomInputMappingFilePath = new Lazy<string>(() => {
            return Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "ParachuteInputConfig.json");
        });
        public static readonly EmptyBinding<ParachuteAction>[] EmptyBindings;

        static ParachuteControls() {
            EmptyBindings = Enumerable.Concat(
                ParachuteActionDetails.MappableButtons.Select(
                    playerAction => new EmptyBinding<ParachuteAction>(playerAction, EnumUtils.ToPrettyString(playerAction), InputType.Button.ToString())),
                ParachuteActionDetails.MappableAxes.Select(
                    playerAction => new EmptyBinding<ParachuteAction>(playerAction, EnumUtils.ToPrettyString(playerAction), InputType.Axis.ToString())))
                .ToArray();
        }

        public static IEnumerable<InputBindingViewModel> ToBindings(LanguageTable languageTable, 
            InputSourceMapping<ParachuteAction> inputMapping,
            Maybe<UnityInputDeviceProfile> currentControllerProfile) {
            return InputBindingView.ToBindings(languageTable, InputBindingGroup.Parachute, EmptyBindings, inputMapping, 
                currentControllerProfile);
        }

        public static InputSourceMapping<ParachuteAction> InitialMapping() {
            if (File.Exists(CustomInputMappingFilePath.Value)) {
                try {
                    return InputSourceExtensions.DeserializeMapping<ParachuteAction>(CustomInputMappingFilePath.Value);
                } catch (Exception e) {
                    Debug.LogError(new Exception("Failed to load input mapping: " + CustomInputMappingFilePath.Value, e));
                }
            }
            return DefaultMapping.Value;
        }

        public static readonly Lazy<InputSourceMapping<ParachuteAction>> DefaultXInputMapping = new Lazy<InputSourceMapping<ParachuteAction>>(
            () => {
                using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                    return inputMappingBuilder.Instance
                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickY), ParachuteAction.WeightShiftBack)
                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickY), ParachuteAction.WeightShiftFront)
                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickX), ParachuteAction.WeightShiftLeft)
                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickX), ParachuteAction.WeightShiftRight)

                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftTrigger), ParachuteAction.PullLeftLines)
                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.RightTrigger), ParachuteAction.PullRightLines)

                        // Alternative config for parachute control with stick
//                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickX), ParachuteAction.PullLeftLines)
//                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Positive, XInput.Axis.LeftStickX), ParachuteAction.PullRightLines)
//                        .Map(InputSource.PolarizedXInputAxis(AxisPolarity.Negative, XInput.Axis.LeftStickY), ParachuteAction.PullBothLines)

                        //.Map(InputSource.XInputButton(XInput.Button.LeftShoulder), ParachuteAction.ParachuteConfigToggle)
                        .Map(InputSource.XInputButton(XInput.Button.X), ParachuteAction.HoldFrontLines)
                        .Map(InputSource.XInputButton(XInput.Button.A), ParachuteAction.HoldRearLines)
                        .Build()
                        .ToInputSourceMapping();
                }
            });

        public static readonly Lazy<InputSourceMapping<ParachuteAction>> DefaultKeyboardOnlyMapping = new Lazy<InputSourceMapping<ParachuteAction>>(
            () => {
                using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                    return inputMapBuilder.Instance
                        .Map(InputSource.Key(KeyCode.UpArrow), ParachuteAction.WeightShiftFront)
                        .Map(InputSource.Key(KeyCode.DownArrow), ParachuteAction.WeightShiftBack)
                        .Map(InputSource.Key(KeyCode.LeftArrow), ParachuteAction.WeightShiftLeft)
                        .Map(InputSource.Key(KeyCode.RightArrow), ParachuteAction.WeightShiftRight)

                        //.Map(InputSource.Key(KeyCode.F1), ParachuteAction.ParachuteConfigToggle)

                        .Map(InputSource.Key(KeyCode.Q), ParachuteAction.PullFrontLineLeft)
                        .Map(InputSource.Key(KeyCode.E), ParachuteAction.PullFrontLineRight)
                        .Map(InputSource.Key(KeyCode.A), ParachuteAction.PullBrakeLineLeft)
                        .Map(InputSource.Key(KeyCode.D), ParachuteAction.PullBrakeLineRight)
                        .Map(InputSource.Key(KeyCode.Z), ParachuteAction.PullRearLineLeft)
                        .Map(InputSource.Key(KeyCode.C), ParachuteAction.PullRearLineRight)

                        .Build()
                        .ToInputSourceMapping();
                }});

        public static readonly Lazy<InputSourceMapping<ParachuteAction>> DefaultKeyboardAndMouseMapping = new Lazy<InputSourceMapping<ParachuteAction>>(
            () => {
                using (var inputMapBuilder = InputMapBuilderPool.Take()) {
                    return inputMapBuilder.Instance
                        .Map(InputSource.Key(KeyCode.W), ParachuteAction.WeightShiftFront)
                        .Map(InputSource.Key(KeyCode.S), ParachuteAction.WeightShiftBack)
                        .Map(InputSource.Key(KeyCode.A), ParachuteAction.WeightShiftLeft)
                        .Map(InputSource.Key(KeyCode.D), ParachuteAction.WeightShiftRight)

                        //.Map(InputSource.Key(KeyCode.F1), ParachuteAction.ParachuteConfigToggle)

                        .Map(InputSource.Key(KeyCode.Q), ParachuteAction.HoldFrontLines)
                        .Map(InputSource.Key(KeyCode.E), ParachuteAction.HoldRearLines)

                        .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.X), ParachuteAction.PullLeftLines)
                        .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Positive, MouseAxisId.X), ParachuteAction.PullRightLines)
                        //.Map(InputSource.LeftMouseButton, ParachuteAction.IncreaseLinePullStrength)

                        // Moving Y actually pulls both lines
                        .Map(InputSource.PolarizedMouseAxis(AxisPolarity.Negative, MouseAxisId.Y), ParachuteAction.PullBothLines)

                        .Build()
                        .ToInputSourceMapping();
                }});

        public static readonly Lazy<InputSourceMapping<ParachuteAction>> DefaultXbox360Mapping = new Lazy<InputSourceMapping<ParachuteAction>>(
            () => {
                using (var inputMappingBuilder = InputMapBuilderPool.Take()) {
                    if (PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.MacOsx) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), ParachuteAction.WeightShiftFront)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), ParachuteAction.WeightShiftBack)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), ParachuteAction.WeightShiftLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), ParachuteAction.WeightShiftRight)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 4), ParachuteAction.PullLeftLines)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), ParachuteAction.PullRightLines)

                            .Map(InputSource.JoystickButton(18), ParachuteAction.HoldFrontLines)
                            .Map(InputSource.JoystickButton(17), ParachuteAction.HoldRearLines)

                            //.Map(InputSource.JoystickButton(9), ParachuteAction.ParachuteConfigToggle)
                            .Build()
                            .ToInputSourceMapping();
                    } else if(PlatformUtil.CurrentOs() == PlatformUtil.OperatingSystem.Linux) {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), ParachuteAction.WeightShiftFront)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), ParachuteAction.WeightShiftBack)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), ParachuteAction.WeightShiftLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), ParachuteAction.WeightShiftRight)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), ParachuteAction.PullLeftLines)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 5), ParachuteAction.PullRightLines)

                            .Map(InputSource.JoystickButton(2), ParachuteAction.HoldFrontLines)
                            .Map(InputSource.JoystickButton(1), ParachuteAction.HoldRearLines)

                            //.Map(InputSource.JoystickButton(5), ParachuteAction.ParachuteConfigToggle)
                            .Build()
                            .ToInputSourceMapping();
                    } else {
                        return inputMappingBuilder.Instance
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 1), ParachuteAction.WeightShiftFront)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 1), ParachuteAction.WeightShiftBack)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 0), ParachuteAction.WeightShiftLeft)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 0), ParachuteAction.WeightShiftRight)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Positive, 2), ParachuteAction.PullLeftLines)
                            .Map(InputSource.PolarizedJoystickAxis(AxisPolarity.Negative, 2), ParachuteAction.PullRightLines)

                            .Map(InputSource.JoystickButton(2), ParachuteAction.HoldFrontLines)
                            .Map(InputSource.JoystickButton(1), ParachuteAction.HoldRearLines)

                            //.Map(InputSource.JoystickButton(5), ParachuteAction.ParachuteConfigToggle)
                            .Build()
                            .ToInputSourceMapping();
                    }
                }
            });

        public static readonly Lazy<InputSourceMapping<ParachuteAction>> DefaultMapping = DefaultKeyboardOnlyMapping;

        public static Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<ParachuteAction>>> DefaultMappings = new Lazy<IImmutableDictionary<InputDefaults, InputSourceMapping<ParachuteAction>>>(
            () => new Dictionary<InputDefaults, InputSourceMapping<ParachuteAction>> {
                {InputDefaults.KeyboardAndMouse, DefaultKeyboardAndMouseMapping.Value},
                {InputDefaults.KeyboardOnly, DefaultKeyboardOnlyMapping.Value},
                {InputDefaults.XInput, DefaultXInputMapping.Value},
                {InputDefaults.Xbox360, DefaultXbox360Mapping.Value},
                {InputDefaults.XboxOne, DefaultXbox360Mapping.Value},
                {InputDefaults.SteamController, DefaultXbox360Mapping.Value},
                {InputDefaults.Playstation4, DefaultXbox360Mapping.Value},
            }.ToFastImmutableEnumDictionary());

        public static ParachuteActionMap Create(ActionMapConfig<ParachuteAction> actionMapConfig, float axisButtonThreshold = 0.5f) {
            var inputSettings = actionMapConfig.InputSettings;
            var inputMapping = actionMapConfig.InputMapping;

            var controllerPeripheral = Peripherals.Controller.GetPeripheral(actionMapConfig.ControllerId,
                inputSettings);

            var buttonInputSource = ImperoCore.MergeAll(
                Adapters.MergeButtons,
                Peripherals.Keyboard,
                Peripherals.Mouse.Buttons,
                controllerPeripheral.Buttons);

            var inputWithAxisSource = inputMapping.ApplyMapping(controllerPeripheral.Axes, Adapters.MergeAxes);
            var inputWithMouseAxisSource = inputMapping.ApplyMapping(Peripherals.Mouse.PolarizedAxes, Adapters.MergeAxes);
            var inputWithButtonSource = inputMapping.ApplyMapping(buttonInputSource, Adapters.MergeButtons);

            bool isMouseInput = inputWithMouseAxisSource.Source.ContainsKey(ParachuteAction.PullBothLines) ||
                inputWithMouseAxisSource.Source.ContainsKey(ParachuteAction.PullLeftLines) ||
                inputWithMouseAxisSource.Source.ContainsKey(ParachuteAction.PullRightLines);

            // Finally merge all input maps
            Debug.Log("parachute sensitivity: " + inputSettings.ParachuteMouseSensitivity);
            var axisInput = ImperoCore.MergeAll(
                Adapters.MergeAxes,
                inputWithAxisSource,
                inputWithMouseAxisSource
                    .Adapt(Adapters.ApplySensitivity(inputSettings.ParachuteMouseSensitivity))
                    .Adapt(ParachuteAction.PullBothLines, Adapters.Deadzone(deadzone: 0.1f)),
                // TODO Instead of button2axis use gradual button2axis
                inputWithButtonSource.Adapt<ParachuteAction, ButtonState, float>(Adapters.Button2Axis));
            var buttonInput = ImperoCore.MergeAll(
                Adapters.MergeButtons,
                inputWithButtonSource,
                inputWithMouseAxisSource.Adapt(Adapters.Axis2Button(3f)),
                inputWithAxisSource.Adapt<ParachuteAction, float, ButtonState>(Adapters.Axis2Button(axisButtonThreshold)));

            axisInput = axisInput.FillEmptyValues(ParachuteActionDetails.MappableActions,
                () => 0.0f);
            buttonInput = buttonInput.FillEmptyValues(ParachuteActionDetails.MappableActions,
                () => ButtonState.Released);

            Func<Vector2> weightShift;
            {
                var horizontalShiftLeft = axisInput.Source[ParachuteAction.WeightShiftLeft];
                var horizontalShiftRight = axisInput.Source[ParachuteAction.WeightShiftRight];
                var verticalShiftFront = axisInput.Source[ParachuteAction.WeightShiftFront];
                var verticalShiftBack = axisInput.Source[ParachuteAction.WeightShiftBack];
                weightShift = () => new Vector2(
                    horizontalShiftRight() - horizontalShiftLeft(),
                    verticalShiftFront() - verticalShiftBack());
            }

            Func<ParachuteLine> pollSelectedLine;
            Func<Vector2> pullBrakeLines;
            Func<Vector2> pullRearLines;
            Func<Vector2> pullFrontLines;
            {
                var pullLeft = axisInput.Source[ParachuteAction.PullLeftLines];
                var pullRight = axisInput.Source[ParachuteAction.PullRightLines];
                var pullBoth = axisInput.Source[ParachuteAction.PullBothLines];
                Func<Vector2> pullLines = () => {
                    var linePull = pullBoth();
                    var leftPull = Mathf.Max(linePull, pullLeft());
                    var rightPull = Mathf.Max(linePull, pullRight());
                    return new Vector2(leftPull, rightPull);
                };

                var toggleFrontlines = buttonInput.Source[ParachuteAction.HoldFrontLines];
                var toggleRearlines = buttonInput.Source[ParachuteAction.HoldRearLines];

                pollSelectedLine = () => {
                    if (toggleFrontlines() == ButtonState.Pressed) {
                        return ParachuteLine.Front;
                    }
                    if (toggleRearlines() == ButtonState.Pressed) {
                        return ParachuteLine.Rear;
                    }
                    return ParachuteLine.Brake;
                };

                pullBrakeLines = ImperoCore.MergePollFns(
                    Adapters.MergeAxes,
                    FilterInput(pullLines, v => pollSelectedLine() == ParachuteLine.Brake),
                    CombineAxes(axisInput, ParachuteAction.PullBrakeLineLeft, ParachuteAction.PullBrakeLineRight));
                pullRearLines = ImperoCore.MergePollFns(
                    Adapters.MergeAxes,
                    FilterInput(pullLines, v => pollSelectedLine() == ParachuteLine.Rear),
                    CombineAxes(axisInput, ParachuteAction.PullRearLineLeft, ParachuteAction.PullRearLineRight));
                pullFrontLines = ImperoCore.MergePollFns(
                    Adapters.MergeAxes,
                    FilterInput(pullLines, v => pollSelectedLine() == ParachuteLine.Front),
                    CombineAxes(axisInput, ParachuteAction.PullFrontLineLeft, ParachuteAction.PullFrontLineRight));
            }

//            var configToggle = buttonInput.Source[ParachuteAction.ParachuteConfigToggle]
//                .Adapt(Adapters.ButtonEvents(() => Time.frameCount));
            Func<ButtonEvent> configToggle = () => ButtonEvent.Nothing;
            Func<ParachuteInput> pollParachuteInput = () => {
                var selectedLine = pollSelectedLine();
                Vector2 brakes = pullBrakeLines();
                Vector2 rearLines = pullRearLines();
                Vector2 frontLines = pullFrontLines();
                return new ParachuteInput(selectedLine, brakes, rearLines, frontLines, weightShift(), isMouseInput);
            };

            return new ParachuteActionMap(pollParachuteInput, configToggle);
        }

        private static Func<Vector2> CombineAxes<TAction>(InputMap<TAction, float> inputMap, TAction xAction, TAction yAction) {
            var pollX = inputMap.Source[xAction];
            var pollY = inputMap.Source[yAction];
            return () => new Vector2(pollX(), pollY());
        }

        private static Func<T> FilterInput<T>(Func<T> poll, Func<T, bool> predicate, T @default = default (T)) {
            return () => {
                var value = poll();
                if (predicate(value)) {
                    return value;
                }
                return @default;
            };
        }
    }

    public struct ParachuteInput {
        public readonly ParachuteLine SelectedLine;
        public Vector2 Brakes;
        public Vector2 RearRisers;
        public Vector2 FrontRisers;
        public readonly Vector2 WeightShift;
        public readonly bool IsMouseInput;

        public ParachuteInput(ParachuteLine selectedLine, Vector2 brakes, Vector2 rearRisers, Vector2 frontRisers, Vector2 weightShift, bool isMouseInput) {
            SelectedLine = selectedLine;
            Brakes = brakes;
            RearRisers = rearRisers;
            FrontRisers = frontRisers;
            WeightShift = weightShift;
            IsMouseInput = isMouseInput;
        }

        public static ParachuteInput SmoothInput(ParachuteInputConfig config, ParachuteInput prevInput, ParachuteInput newInput, float deltaTime) {
            return new ParachuteInput(
                selectedLine: newInput.SelectedLine,
                brakes: Vector2.Lerp(prevInput.Brakes, newInput.Brakes, config.BrakeSmoothingSpeed * deltaTime),
                rearRisers: Vector2.Lerp(prevInput.RearRisers, newInput.RearRisers, config.RearRisersSmoothingSpeed * deltaTime),
                frontRisers: Vector2.Lerp(prevInput.FrontRisers, newInput.FrontRisers, config.FrontRisersSmoothingSpeed * deltaTime),
                weightShift: Vector2.Lerp(prevInput.WeightShift, newInput.WeightShift, config.WeightShiftSmoothingSpeed * deltaTime),
                isMouseInput: newInput.IsMouseInput);
        }

        public Vector2 SelectedLinePull {
            get {
                if (SelectedLine == ParachuteLine.Brake) {
                    return Brakes;
                }
                if (SelectedLine == ParachuteLine.Rear) {
                    return RearRisers;
                }
                return FrontRisers;
            }
        }

        public static readonly ParachuteInput Zero = new ParachuteInput(
            selectedLine: ParachuteLine.Brake,
            brakes: Vector2.zero,
            rearRisers: Vector2.zero,
            frontRisers: Vector2.zero,
            weightShift: Vector2.zero,
            isMouseInput: false);
    }

    [Serializable]
    public struct ParachuteInputConfig {
        public float BrakeSmoothingSpeed;
        public float RearRisersSmoothingSpeed;
        public float FrontRisersSmoothingSpeed;
        public float WeightShiftSmoothingSpeed;
    }
}
