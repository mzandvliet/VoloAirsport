using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RamjetAnvil.Unity.Landmass;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace RamjetAnvil.Volo {

    public struct Resolution : IEquatable<Resolution> {
        public int Width;
        public int Height;

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }

        public bool Equals(Resolution other) {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Resolution && Equals((Resolution) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (Width * 397) ^ Height;
            }
        }

        public static bool operator ==(Resolution left, Resolution right) {
            return left.Equals(right);
        }

        public static bool operator !=(Resolution left, Resolution right) {
            return !left.Equals(right);
        }


        public override string ToString() {
            return string.Format("{0}x{1}", Width, Height);
        }
    }

    public enum BloomQuality {
        Off, Cheap, High
    }

    public enum VoloShadowQuality {
        Low, Medium, High
    }

    public enum UnitSystem {
        Metric, Imperial
    }

    public enum TextureQuality {
        Eighth, Quarter, Half, Full
    }

    public enum AnisotropicFilteringMode {
        None, Some, Full
    }

    public enum AntiAliasingMode {
        Off, Fxaa, Smaa
    }

    public enum AntiAliasingQuality {
        Low, Medium, High, Ultra
    }

    public enum VrMode {
        None, Oculus, OpenVr
    }

    public static class VrModeExtensions {
        public static VrMode? FromCommandlineArgument(string s) {
            switch (s) {
                case "none":
                    return VrMode.None;
                case "oculus":
                    return VrMode.Oculus;
                case "openvr":
                    return VrMode.OpenVr;
                default:
                    Debug.LogWarning("Unknown command-line VR mode: " + s);
                    return null;
            }
        }
    }

    [Serializable]
    public struct GameSettings {

        public const int CurrentFormatVersion = 5;

        public int FormatVersion;
        public GraphicsSettings Graphics;
        public AudioSettings Audio;
        public InputSettings Input;
        public GameplaySettings Gameplay;
        public OtherSettings Other;
        public TutorialSettings Tutorial;

        public struct GraphicsSettings {
            public bool IsWindowed;
            public Resolution ScreenResolution;
            public float ScreenshotMagnificationFactor;
            public int FieldOfView;
            public float ScreenShakeIntensity;
            public VoloShadowQuality VoloShadowQuality;
            public float ShadowDistance;
            public int DetailObjectDistance;
            public int DetailObjectDensity;
            public float TerrainRenderingAccuracy;
            public bool TerrainCastShadows;
            public BloomQuality BloomQuality;
            public int MotionBlurQualitySteps;
            public AntiAliasingMode AntiAliasingMode;
            public AntiAliasingQuality AntiAliasingQuality;
            public bool IsVSyncEnabled;
            public int TargetFrameRate;
            public float MaxParticles;
            public TextureQuality TextureQuality;
            public AnisotropicFilteringMode AnisotropicFilteringMode;
            public float LodBias;
        }

        public struct AudioSettings {
            public bool IsMuted;
            public float SoundEffectsVolume;
            public float MusicVolume;
        }

        // Input
        public struct InputSettings {
            public float WingsuitMouseSensitivity;
            public float ParachuteMouseSensitivity;
            public float InputGamma;
            public float InputSpeedScaling;
            public float JoystickDeadzone;            
        }


        // Gameplay
        public struct GameplaySettings {
            public bool ShowHud;
            public UnitSystem UnitSystem;
            public bool VisualizeTrajectory;
            public bool VisualizeAerodynamics;
            public bool CoursesEnabled;
            public bool IsRingCollisionOn;
            public float StallLimiterStrength;
            public float RollLimiterStrength;
            public float PitchAttitude;
            public Ecology.TimeSettings Time;
            public Ecology.WeatherSettings Weather;
        }

        public struct OtherSettings {
            public string Language;
            public bool AutomaticNetworkPort;
            public int NetworkPort;
            public string NatFacilitatorEndpoint;
            public float ConnectionAttemptTimeout;
            public string MasterServerUrl;
            public string SelectedParachuteId;
        }

        public struct TutorialSettings {
            public bool HasUnfoldedParachute;
        }

        public static readonly Lazy<string> DefaultSettingsConfigPath = new Lazy<string>(() => 
            Path.Combine(Application.streamingAssetsPath, "Config/DefaultGameSettings.json"));
        public static readonly Lazy<string> UserSettingsConfigPath = new Lazy<string>(() => 
            Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "GameSettings.json")); 

        public static Lazy<Resolution[]> AvailableScreenResolutions = new Lazy<Resolution[]>(
            () => Screen.resolutions
                .Select(r => new Resolution(r.width, r.height))
                .OrderBy(r => r.Width)
                .ToArray());

        public static readonly Lazy<JsonSerializer> JsonSerializer = new Lazy<JsonSerializer>(() => {
            var s = new JsonSerializer();
            s.Converters.Add(new StringEnumConverter());
            s.Formatting = Formatting.Indented;
            return s;
        });

        public static readonly Lazy<GameSettings> DefaultSettings = new Lazy<GameSettings>(() => {
            return ReadSettingsFromDisk(new StreamReader(DefaultSettingsConfigPath.Value, Encoding.UTF8));
        });

        public static GameSettings ReadSettingsFromDisk() {
            return ReaderUtil.FallbackReader(
                () => ReadSettingsFromDisk(new StreamReader(UserSettingsConfigPath.Value, Encoding.UTF8)),
                () => ReadSettingsFromDisk(new StreamReader(DefaultSettingsConfigPath.Value, Encoding.UTF8)));
        }

        public static GameSettings ReadSettingsFromDisk(TextReader t) {
            using (var reader = t) {
                var serializedJson = reader.ReadToEnd();
                var deserializedSettings = JsonConvert.DeserializeObject<GameSettings>(serializedJson);
                if (deserializedSettings.FormatVersion < CurrentFormatVersion) {
                    throw new Exception("Cannot read game settings that are of an older format version, " +
                        "serialized version is " + deserializedSettings.FormatVersion + " current version is " + CurrentFormatVersion);
                }
                return deserializedSettings;
            }
        }
    }

    public static class GameSettingsExtensions {
        
        public static void Serialize2Disk(this GameSettings settings, string path) {
            settings.FormatVersion = GameSettings.CurrentFormatVersion;
            using (var writer = File.CreateText(path)) {
                GameSettings.JsonSerializer.Value.Serialize(writer, settings);
            }
        }
    }

}
