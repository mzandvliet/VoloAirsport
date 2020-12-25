using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo.Ui {
    public class InputSettingsViewModel {
        public readonly GuiComponentDescriptor.Range WingsuitMouseSensitivity;
        public readonly GuiComponentDescriptor.Range ParachuteMouseSensitivity;
        public readonly GuiComponentDescriptor.Range InputSpeedScaling;
        public readonly GuiComponentDescriptor.Range InputGamma;
        public readonly GuiComponentDescriptor.Range JoystickDeadzone;
        public readonly GuiComponentDescriptor.List RestoreInputDefaults;

        public InputSettingsViewModel(GameSettingsUpdater settingsUpdater) {
            Func<GameSettings> s = () => settingsUpdater.CurrentSettings;
            WingsuitMouseSensitivity = new GuiComponentDescriptor.Range(
                "wingsuit_mouse_sensitivity",
                minValue: 0.1f,
                maxValue: 10.00f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Input.WingsuitMouseSensitivity = value;    
                    return settings;
                }),
                currentValue: () => s().Input.WingsuitMouseSensitivity,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Input.WingsuitMouseSensitivity, str, decimalPlaces: 2);
                },
                stepSize: 0.01f);
            ParachuteMouseSensitivity = new GuiComponentDescriptor.Range(
                "chute_mouse_sensitivity",
                minValue: 0.1f,
                maxValue: 10f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Input.ParachuteMouseSensitivity = value;    
                    return settings;
                }),
                currentValue: () => s().Input.ParachuteMouseSensitivity,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Input.ParachuteMouseSensitivity, str, decimalPlaces: 2);
                },
                stepSize: 0.01f);
            InputSpeedScaling = new GuiComponentDescriptor.Range(
                "input_speed_scaling",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Input.InputSpeedScaling = value;    
                    return settings;
                }),
                currentValue: () => s().Input.InputSpeedScaling,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Input.InputSpeedScaling, str, decimalPlaces: 2);
                },
                stepSize: 0.01f);
            InputGamma = new GuiComponentDescriptor.Range(
                "input_gamma",
                minValue: 1f,
                maxValue: 3f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Input.InputGamma = value;    
                    return settings;
                }),
                currentValue: () => s().Input.InputGamma,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Input.InputGamma, str, decimalPlaces: 2);
                },
                stepSize: 0.01f);
            JoystickDeadzone = new GuiComponentDescriptor.Range(
                "joystick_deadzone",
                minValue: 0f,
                maxValue: 0.99f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Input.JoystickDeadzone = value;    
                    return settings;
                }),
                currentValue: () => s().Input.JoystickDeadzone,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Input.JoystickDeadzone, str, decimalPlaces: 2);
                },
                stepSize: 0.01f);
            var inputDefaults = EnumUtils.GetValues<InputDefaults>().ToList();
            inputDefaults.Remove(InputDefaults.XInput);
            RestoreInputDefaults = new GuiComponentDescriptor.List(
                "defaults",
                inputDefaults.Select(e => GuiComponentDescriptor.GetEnumString(e)).ToArray(),
                updateIndex: index => {
                    var unappliedSettings = settingsUpdater.UnappliedSettings;
                    unappliedSettings.InputDefaults = inputDefaults[index];
                    settingsUpdater.UnappliedSettings = unappliedSettings;
                },
                currentIndex: () => {
                    var unappliedSettings = settingsUpdater.UnappliedSettings;
                    return inputDefaults.IndexOf(unappliedSettings.InputDefaults);
                });
        }
    }
}
