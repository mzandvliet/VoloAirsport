using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public class GameplaySettingsViewModel {
        public readonly GuiComponentDescriptor.Boolean ShowHud;
        public readonly GuiComponentDescriptor.List UnitSystem;
        public readonly GuiComponentDescriptor.Boolean VisualizeTrajectory;
        public readonly GuiComponentDescriptor.Boolean VisualizeAerodynamics;
        public readonly GuiComponentDescriptor.Boolean CoursesEnabled;
        public readonly GuiComponentDescriptor.Boolean IsRingCollisionOn;
        public readonly GuiComponentDescriptor.Range StallLimiterStrength;
        public readonly GuiComponentDescriptor.Range RollLimiterStrength;
        public readonly GuiComponentDescriptor.Range PitchAttitude;

        public GameplaySettingsViewModel(GameSettingsUpdater settingsUpdater) {
            Func<GameSettings> s = () => settingsUpdater.CurrentSettings;
            ShowHud = new GuiComponentDescriptor.Boolean(
                "show_hud",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.ShowHud = value;
                    return settings; 
                }),
                isChecked: () => s().Gameplay.ShowHud);
            UnitSystem = new GuiComponentDescriptor.List(
                "unit_system",
                GuiComponentDescriptor.GetEnumStrings<UnitSystem>(),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Gameplay.UnitSystem = EnumUtils.GetValues<UnitSystem>()[index];
                    return settings; 
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Gameplay.UnitSystem));
            VisualizeTrajectory = new GuiComponentDescriptor.Boolean(
                "visualize_trajectory",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.VisualizeTrajectory = value;
                    return settings; 
                }),
                isChecked: () => s().Gameplay.VisualizeTrajectory);
            VisualizeAerodynamics = new GuiComponentDescriptor.Boolean(
                "visualize_aerodynamics",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.VisualizeAerodynamics = value;
                    return settings; 
                }),
                isChecked: () => s().Gameplay.VisualizeAerodynamics);
            CoursesEnabled = new GuiComponentDescriptor.Boolean(
                "courses",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.CoursesEnabled = value;
                    return settings; 
                }),
                isChecked: () => s().Gameplay.CoursesEnabled);
            IsRingCollisionOn = new GuiComponentDescriptor.Boolean(
                "ring_collision",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.IsRingCollisionOn = value;
                    return settings; 
                }),
                isChecked: () => s().Gameplay.IsRingCollisionOn);
            StallLimiterStrength = new GuiComponentDescriptor.Range(
                "stall_limiter_strength",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.StallLimiterStrength = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.StallLimiterStrength,
                updateDisplayValue: (descriptor, str) => GuiComponentDescriptor.DisplayPercentage(s().Gameplay.StallLimiterStrength, str),
                stepSize: 0.01f);
            RollLimiterStrength = new GuiComponentDescriptor.Range(
                "roll_limiter_strength",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.RollLimiterStrength = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.RollLimiterStrength,
                updateDisplayValue: (descriptor, str) => GuiComponentDescriptor.DisplayPercentage(s().Gameplay.RollLimiterStrength, str),
                stepSize: 0.01f);
            PitchAttitude = new GuiComponentDescriptor.Range(
                "pitch_attitude",
                minValue: -1f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.PitchAttitude = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.PitchAttitude,
                updateDisplayValue: (descriptor, str) => GuiComponentDescriptor.DisplayNumber(s().Gameplay.PitchAttitude, str, decimalPlaces: 1),
                stepSize: 0.1f);

        }
    }
}
