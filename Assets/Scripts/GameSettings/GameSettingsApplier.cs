using System;
using System.Collections.Generic;
using RamjetAnvil.Landmass;
using RamjetAnvil.Volo;
using UnityEngine;
using Resolution = RamjetAnvil.Volo.Resolution;

/// <summary>
/// Applies any changes that are made to the game's graphics settings
/// </summary>
public class GameSettingsApplier {

    private readonly UserConfigurableSystems _configurableSystems;
    private bool _isRunOnce;
    private GameSettings? _currentGameSettings;

    public GameSettingsApplier(UserConfigurableSystems configurableSystems) {
        _configurableSystems = configurableSystems;
        _currentGameSettings = null;
        _isRunOnce = false;
    }

    public void ApplyAudioSettings(GameSettings.AudioSettings settings) {
        float soundEffectsVolume;
        float musicVolume;
        if (settings.IsMuted) {
            soundEffectsVolume = 0f;
            musicVolume = 0f;
        } else {
            soundEffectsVolume = settings.SoundEffectsVolume;
            musicVolume = settings.MusicVolume;
        }
        if (_configurableSystems.SoundMixer != null) {
            _configurableSystems.SoundMixer.SetMaxVolume(SoundLayer.Effects, soundEffectsVolume);
            _configurableSystems.SoundMixer.SetMaxVolume(SoundLayer.Music, musicVolume);   
        }
    }

    // Todo: Why is this called *before* it is called the first time in VoloModule?
    public void ApplySettings(GameSettings settings, VrMode vrMode) {
        if (!_isRunOnce) {
            if (!QualitySettings.names[QualitySettings.GetQualityLevel()].Equals("Fantastic")) {
                for (int i = 0; i < QualitySettings.names.Length; i++) {
                    var qualityLevel = QualitySettings.names[i];
                    if (qualityLevel.Equals("Fantastic")) {
                        QualitySettings.SetQualityLevel(i, applyExpensiveChanges: true);        
                    }
                }
            }
            _configurableSystems.ActiveLanguage.SetLanguage(CultureCode.FromString(settings.Other.Language));

            _isRunOnce = true;
        }

        UpdateWhenChanged(settings, 
            s => new ScreenSettings(s.Graphics.ScreenResolution, s.Graphics.IsWindowed), 
            updatedScreenSettings => {
                var currentScreenSetting = new ScreenSettings(new Resolution(Screen.width, Screen.height),
                    isWindowed: !Screen.fullScreen);
                if (!currentScreenSetting.Equals(updatedScreenSettings)) {
                    Screen.SetResolution(
                        updatedScreenSettings.Resolution.Width,
                        updatedScreenSettings.Resolution.Height,
                        fullscreen: !updatedScreenSettings.IsWindowed);
                }
            });

        var challengeManager = _configurableSystems.CourseManager;
        // TODO Configure challenge manager
//	    if (courseManager != null) {
//	        courseManager.UpdateSettings(new CourseManager.CourseSettings {
//	            IsCoursesEnabled = settings.Gameplay.CoursesEnabled,
//                IsRingCollision = settings.Gameplay.IsRingCollisionOn
//	        });
//	    }

        UpdateWhenChanged(settings, s => s.Graphics.TargetFrameRate, fps => Application.targetFrameRate = fps);
        UpdateWhenChanged(settings, s => s.Graphics.AnisotropicFilteringMode, 
            mode => QualitySettings.anisotropicFiltering = AnisotropicFilteringMode2Unity(mode));
        UpdateWhenChanged(settings, s => s.Graphics.LodBias, lodBias => QualitySettings.lodBias = lodBias);
        UpdateWhenChanged(settings, s => s.Graphics.TextureQuality, 
            textureQuality => QualitySettings.masterTextureLimit = TextureQuality2MasterTextureLimit(settings.Graphics.TextureQuality));
        UpdateWhenChanged(settings, s => s.Graphics.VoloShadowQuality, quality => QualitySettings.shadowCascades = ShadowQuality2Cascades(quality));
        UpdateWhenChanged(settings, s => s.Graphics.ShadowDistance, shadowDistance => QualitySettings.shadowDistance = shadowDistance);

        var cameraManager = _configurableSystems.CameraManager;
	    if (cameraManager != null && cameraManager.Rig != null) {
	        var rig = cameraManager.Rig;
            rig.ApplySettings(settings);
	        if (rig.Shake != null) {
                rig.Shake.Amplitude = settings.Graphics.ScreenShakeIntensity;
            }
	    } 

        UpdateWhenChanged(settings, s => s.Other.Language, languageCode => {
            _configurableSystems.ActiveLanguage.SetLanguage(CultureCode.FromString(languageCode));
        });

        var terrainManager = _configurableSystems.TerrainManager;
	    if (terrainManager != null) {
	        var currentTerrainConfig = terrainManager.TerrainConfiguration;

            currentTerrainConfig.CastShadows = settings.Graphics.TerrainCastShadows;
            currentTerrainConfig.DetailObjectDensity = settings.Graphics.DetailObjectDensity;
            currentTerrainConfig.DetailObjectDistance = settings.Graphics.DetailObjectDistance;
            // The accuracy of the mapping between the terrain maps (heightmap, textures, etc) and the generated terrain; 
            // higher values indicate lower accuracy but lower rendering overhead.
            // Pixel accuracy examples: 3:1, 50:1
	        currentTerrainConfig.HeightmapPixelError = settings.Graphics.TerrainRenderingAccuracy;
            terrainManager.ApplyTerrainQualitySettings(currentTerrainConfig);
//	        currentTerrainConfig.TreeQuality = settings.Graphics.TreeQuality;
//          UpdateWhenChanged(settings, s => s.Graphics.TreeQuality, treeQuality => terrainManager.ApplyTreeQualitySettings(treeQuality));
        }

        // Todo: it would be great if we could initialize the grassmanager earlier
        var grassManager = _configurableSystems.GrassManager;
        if (grassManager != null) {
            grassManager.Config = new GrassManagerConfig(settings.Graphics.DetailObjectDistance != 0, settings.Graphics.DetailObjectDistance, settings.Graphics.DetailObjectDensity);
            grassManager.Initialize();
        }

        var vsyncCount = settings.Graphics.IsVSyncEnabled ? 1 : 0;
        if (vrMode == VrMode.None && QualitySettings.vSyncCount != vsyncCount) {
            QualitySettings.vSyncCount = vsyncCount;
        }
        
        _currentGameSettings = settings;
    }

    private void UpdateWhenChanged<T>(
        GameSettings currentSettings,
        Func<GameSettings, T> propertyValue,
        Action<T> update) {
        var previousSettings = _currentGameSettings;

        var currentProperty = propertyValue(currentSettings);
        bool isChanged;
        if (previousSettings.HasValue) {
            var previousProperty = propertyValue(previousSettings.Value);
            isChanged = !EqualityComparer<T>.Default.Equals(previousProperty, currentProperty);
        } else {
            isChanged = true;
        }

        if (isChanged) {
            update(currentProperty);
        }
    }

    private static UnityEngine.AnisotropicFiltering AnisotropicFilteringMode2Unity(AnisotropicFilteringMode mode) {
        switch (mode) {
            case AnisotropicFilteringMode.None:
                return UnityEngine.AnisotropicFiltering.Disable;
            case AnisotropicFilteringMode.Some:
                return UnityEngine.AnisotropicFiltering.Enable;
            case AnisotropicFilteringMode.Full:
                return UnityEngine.AnisotropicFiltering.ForceEnable;
            default:
                Debug.LogError("Could not convert anisotropic mode: " + mode + ", defaulting to None.");
                return UnityEngine.AnisotropicFiltering.Disable;
        }
    }

    private static int TextureQuality2MasterTextureLimit(TextureQuality textureQuality) {
        switch (textureQuality) {
            case TextureQuality.Eighth:
                return 3;
            case TextureQuality.Quarter:
                return 2;
            case TextureQuality.Half:
                return 1;
            case TextureQuality.Full:
                return 0;
            default:
                Debug.LogError("Could not convert texture quality level to int: " + textureQuality + ", defaulting to Half.");
                return 1;
        }
    }

    private static int ShadowQuality2Cascades(VoloShadowQuality quality) {
        switch (quality) {
            case VoloShadowQuality.Low:
                return 0;
            case VoloShadowQuality.Medium:
                return 2;
            case VoloShadowQuality.High:
                return 4;
            default:
                Debug.LogError("Could not convert shadow quality level to int: " + quality + ", defaulting to medium.");
                return 1;
        }
    }

    public struct ScreenSettings : IEquatable<ScreenSettings> {
        public readonly Resolution Resolution;
        public readonly bool IsWindowed;

        public ScreenSettings(Resolution resolution, bool isWindowed) {
            Resolution = resolution;
            IsWindowed = isWindowed;
        }

        public bool Equals(ScreenSettings other) {
            return Resolution.Equals(other.Resolution) && IsWindowed.Equals(other.IsWindowed);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ScreenSettings && Equals((ScreenSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (Resolution.GetHashCode() * 397) ^ IsWindowed.GetHashCode();
            }
        }

        public static bool operator ==(ScreenSettings left, ScreenSettings right) {
            return left.Equals(right);
        }

        public static bool operator !=(ScreenSettings left, ScreenSettings right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("Resolution: {0}, IsWindowed: {1}", Resolution, IsWindowed);
        }
    }

}
