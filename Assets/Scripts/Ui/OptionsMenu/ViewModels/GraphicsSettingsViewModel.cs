using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Landmass;
using RamjetAnvil.Unity.Utility;
using StringLeakTest;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace RamjetAnvil.Volo.Ui {

    public class GraphicsSettingsViewModel {
        public readonly GuiComponentDescriptor.List ScreenResolution;
        public readonly GuiComponentDescriptor.Boolean IsWindowed;
        public readonly GuiComponentDescriptor.Range FieldOfView;
        public readonly GuiComponentDescriptor.Range ScreenshotMagnification;
        public readonly GuiComponentDescriptor.Range ScreenShakeIntensity;
        public readonly GuiComponentDescriptor.Range TargetFrameRate;
        public readonly GuiComponentDescriptor.List TextureQuality;
        public readonly GuiComponentDescriptor.List AnisotropicFilteringMode;
        public readonly GuiComponentDescriptor.List ShadowQuality;
        public readonly GuiComponentDescriptor.Range ShadowDistance;
//        public readonly GuiComponentDescriptor.List TreeQuality;
        public readonly GuiComponentDescriptor.Range DetailObjectDistance;
        public readonly GuiComponentDescriptor.Range DetailObjectDensity;
        public readonly GuiComponentDescriptor.Range MaxParticles;
        public readonly GuiComponentDescriptor TerrainRenderingAccuracy;
        public readonly GuiComponentDescriptor.Boolean TerrainCastShadows;
        public readonly GuiComponentDescriptor.List BloomQuality;
        public readonly GuiComponentDescriptor.Range MotionBlurQualitySteps;
        public readonly GuiComponentDescriptor.List AntiAliasingMode;
        public readonly GuiComponentDescriptor.List AntiAliasingQuality;
        public readonly GuiComponentDescriptor.Boolean VSync;

        public GraphicsSettingsViewModel(GameSettingsUpdater settingsUpdater) {
            Func<GameSettings> s = () => settingsUpdater.CurrentSettings;
            var availableResolutions = GetScreenResolutions(s().Graphics.ScreenResolution);
            ScreenResolution = new GuiComponentDescriptor.List(
                "screen_resolution",
                availableResolutions.Select(r => r.Width + "×" + r.Height).ToArray(),
                updateIndex: newIndex => {
                    var unappliedSettings = settingsUpdater.UnappliedSettings;
                    unappliedSettings.ScreenResolution = availableResolutions[newIndex];
                    settingsUpdater.UnappliedSettings = unappliedSettings;
                },
                currentIndex: () => {
                    var unappliedSettings = settingsUpdater.UnappliedSettings;
                    Resolution resolution;
                    if (unappliedSettings.ScreenResolution.HasValue) {
                        resolution = unappliedSettings.ScreenResolution.Value;
                    } else {
                        resolution = s().Graphics.ScreenResolution;
                    }
                    return availableResolutions.IndexOf(resolution);
                });
            IsWindowed = new GuiComponentDescriptor.Boolean(
                "windowed",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Graphics.IsWindowed = value;
                    return settings; 
                }),
                isChecked: () => s().Graphics.IsWindowed);
            FieldOfView = new GuiComponentDescriptor.Range(
                "field_of_view",
                minValue: 60f,
                maxValue: 120f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.FieldOfView = Mathf.RoundToInt(value);
                    return settings; 
                }),
                currentValue: () => s().Graphics.FieldOfView,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.IsEnabled = true;
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.FieldOfView, str, decimalPlaces: 0,
                        postFix: "°");
                });
            ScreenshotMagnification = new GuiComponentDescriptor.Range(
                "screenshot_magnification",
                minValue: 0.5f,
                maxValue: 4f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.ScreenshotMagnificationFactor = value;
                    return settings; 
                }),
                currentValue: () => s().Graphics.ScreenshotMagnificationFactor,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.ScreenshotMagnificationFactor, str,
                        decimalPlaces: 1, postFix: " ×");
                },
                stepSize: 0.1f);
            ScreenShakeIntensity = new GuiComponentDescriptor.Range(
                "screen_shake_intensity",
                minValue: 0f,
                maxValue: 2f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.ScreenShakeIntensity = value;
                    return settings; 
                }),
                currentValue: () => s().Graphics.ScreenShakeIntensity,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.ScreenShakeIntensity, str, decimalPlaces: 1);
                },
                stepSize: 0.1f);
            TargetFrameRate = new GuiComponentDescriptor.Range(
                "target_framerate",
                minValue: 30f,
                maxValue: 120f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.TargetFrameRate = Mathf.RoundToInt(value);
                    return settings; 
                }),
                currentValue: () => s().Graphics.TargetFrameRate,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.TargetFrameRate, str, decimalPlaces: 0,
                        postFix: " fps");
                });
            TextureQuality = new GuiComponentDescriptor.List(
                "texture_quality",
                GuiComponentDescriptor.GetEnumStrings<TextureQuality>("texq"),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Graphics.TextureQuality = EnumUtils.GetValues<TextureQuality>()[index];
                    return settings; 
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Graphics.TextureQuality));
            AnisotropicFilteringMode = new GuiComponentDescriptor.List(
                "anisotropic_filtering",
                GuiComponentDescriptor.GetEnumStrings<AnisotropicFilteringMode>("aniso"),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Graphics.AnisotropicFilteringMode = EnumUtils.GetValues<AnisotropicFilteringMode>()[index];
                    return settings; 
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Graphics.AnisotropicFilteringMode));
            ShadowQuality = new GuiComponentDescriptor.List(
                "shadow_quality",
                GuiComponentDescriptor.GetEnumStrings<VoloShadowQuality>(),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Graphics.VoloShadowQuality = EnumUtils.GetValues<VoloShadowQuality>()[index];
                    return settings; 
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Graphics.VoloShadowQuality));
            ShadowDistance = new GuiComponentDescriptor.Range(
                "shadow_distance",
                minValue: 0f,
                maxValue: 2000f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.ShadowDistance = value;
                    return settings; 
                }),
                currentValue: () => s().Graphics.ShadowDistance,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.ShadowDistance, str, decimalPlaces: 0,
                        postFix: " m");
                },
                stepSize: 20f);
//            TreeQuality = new GuiComponentDescriptor.List(
//                "tree_quality",
//                GuiComponentDescriptor.GetEnumStrings<TreeQuality>(),
//                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
//                    settings.Graphics.TreeQuality = EnumUtils.GetValues<TreeQuality>()[index];
//                    return settings; 
//                }),
//                currentIndex: () => EnumUtils.IndexOf(s().Graphics.TreeQuality));
            DetailObjectDistance = new GuiComponentDescriptor.Range(
                "grass_view_distance",
                minValue: 0f,
                maxValue: 7f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.DetailObjectDistance = Mathf.RoundToInt(value);
                    return settings; 
                }),
                currentValue: () => s().Graphics.DetailObjectDistance,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.DetailObjectDistance, str, decimalPlaces: 0);
                });
            DetailObjectDensity = new GuiComponentDescriptor.Range(
                "grass_density",
                minValue: 1f,
                maxValue: 5f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.DetailObjectDensity = Mathf.RoundToInt(value);
                    return settings; 
                }),
                currentValue: () => s().Graphics.DetailObjectDensity,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.DetailObjectDensity, str, decimalPlaces: 0);
                });
            MaxParticles = new GuiComponentDescriptor.Range(
                "max_number_of_particles",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.MaxParticles = value;
                    return settings; 
                }),
                currentValue: () => s().Graphics.MaxParticles,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayPercentage(s().Graphics.MaxParticles, str);
                },
                stepSize: 0.01f);
            TerrainRenderingAccuracy = new GuiComponentDescriptor.Range(
                "terrain_rendering_accuracy",
                minValue: 0f,
                maxValue: 47f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.TerrainRenderingAccuracy = 50f - value;
                    return settings; 
                }),
                currentValue: () => 50f - s().Graphics.TerrainRenderingAccuracy,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Graphics.TerrainRenderingAccuracy, str, decimalPlaces: 0,
                        postFix: ":1 px");
                });
            TerrainCastShadows = new GuiComponentDescriptor.Boolean(
                "cast_terrain_shadows",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Graphics.TerrainCastShadows = value;
                    return settings; }),
                isChecked: () => s().Graphics.TerrainCastShadows);
            BloomQuality = new GuiComponentDescriptor.List(
                "bloom",
                GuiComponentDescriptor.GetEnumStrings<BloomQuality>(),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Graphics.BloomQuality = EnumUtils.GetValues<BloomQuality>()[index];
                    return settings; 
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Graphics.BloomQuality));
            MotionBlurQualitySteps = new GuiComponentDescriptor.Range(
                "motion_blur",
                minValue: 0f,
                maxValue: 20f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Graphics.MotionBlurQualitySteps = Mathf.RoundToInt(value);
                    return settings; 
                }),
                currentValue: () => s().Graphics.MotionBlurQualitySteps,
                updateDisplayValue: (descriptor, displayStr) => {
                    var value = s().Graphics.MotionBlurQualitySteps;
                    if (value == 0) {
                        displayStr.Append("off");
                    } else {
                        displayStr.Append(value);
                    }
                });
            AntiAliasingMode = new GuiComponentDescriptor.List(
                "anti_aliasing_mode",
                GuiComponentDescriptor.GetEnumStrings<AntiAliasingMode>(),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Graphics.AntiAliasingMode = EnumUtils.GetValues<AntiAliasingMode>()[index];
                    return settings;
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Graphics.AntiAliasingMode));
            AntiAliasingQuality = new GuiComponentDescriptor.List(
                "anti_aliasing_quality",
                GuiComponentDescriptor.GetEnumStrings<AntiAliasingQuality>(),
                updateIndex: settingsUpdater.Updater<int>((settings, index) => {
                    settings.Graphics.AntiAliasingQuality = EnumUtils.GetValues<AntiAliasingQuality>()[index];
                    return settings;
                }),
                currentIndex: () => EnumUtils.IndexOf(s().Graphics.AntiAliasingQuality));
            VSync = new GuiComponentDescriptor.Boolean(
                "vsync",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Graphics.IsVSyncEnabled = value;
                    return settings; 
                }),
                isChecked: () => s().Graphics.IsVSyncEnabled);
        }
        
        private static IList<Resolution> _screenResolutions = null;
        private static IList<Resolution> GetScreenResolutions(Resolution currentResolution) {
            if (_screenResolutions == null) {
                _screenResolutions = GameSettings.AvailableScreenResolutions.Value.ToList();
            }
            if (!_screenResolutions.Contains(currentResolution)) {
                _screenResolutions.Add(currentResolution);
                _screenResolutions = _screenResolutions
                    .OrderBy(r => r.Width)
                    .ToList();
            }
            return _screenResolutions;
        }
    }

}
