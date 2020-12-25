using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class GameSettingsProvider : MonoBehaviour {

    [SerializeField] private GameSettings _initialGameSettings = new GameSettings {
        Graphics = new GameSettings.GraphicsSettings {
            ScreenResolution = {Width = 1280, Height = 720},
            FieldOfView = 85,
            ScreenShakeIntensity = 1.0f,
            IsWindowed = true,
            ShadowDistance = 0.0f,
            VoloShadowQuality = VoloShadowQuality.Low,
            DetailObjectDistance = 3,
            DetailObjectDensity = 2,
            TerrainRenderingAccuracy = 0.5f,
            TerrainCastShadows = true,
            BloomQuality = BloomQuality.Off,
            MotionBlurQualitySteps = 0,
            AntiAliasingMode = AntiAliasingMode.Fxaa,
            AntiAliasingQuality = AntiAliasingQuality.Medium,
            IsVSyncEnabled = false,
            TargetFrameRate = 60,
        },
        Audio = new GameSettings.AudioSettings {
            SoundEffectsVolume = 1.0f,
            MusicVolume = 0.7f,
        },
        Input = new GameSettings.InputSettings {
            WingsuitMouseSensitivity = 1f,
            ParachuteMouseSensitivity = 0.07f,
            InputGamma = 2.0f,
            InputSpeedScaling = 0.85f,
            JoystickDeadzone = 0.2f,            
        },
        Gameplay = new GameSettings.GameplaySettings {
            ShowHud = true,
            UnitSystem = UnitSystem.Metric,
            VisualizeTrajectory = true,
            VisualizeAerodynamics = false,
            CoursesEnabled = true,
            IsRingCollisionOn = false,
            StallLimiterStrength = 0.5f,
            RollLimiterStrength = 0.5f,

            Time = new Ecology.TimeSettings {
                IsTimeSimulated = true,
                CurrentDateTime = new DateTime(2015, 3, 1, 12, 0, 0, DateTimeKind.Utc),
                DayLengthInMinutes = 15f,
                IsNightSkipped = false,
                // Lauterbrunnen, Switzerland
                Longitude = 7.9012568f,
                Latitude = 46.5557724f
            },
            Weather = new Ecology.WeatherSettings {
                IsWeatherSimulated = true,
                SnowAltitude = 3000f,
                DaysPerSeason = 2f,
                SnowfallIntensity = 0f,
                FogIntensity = 0f
            }
        },
        Other = new GameSettings.OtherSettings {
            Language = "en-US",
            AutomaticNetworkPort = true,
            NetworkPort = 5676,
            NatFacilitatorEndpoint = "95.85.31.166:15493",
            ConnectionAttemptTimeout = 30,
            MasterServerUrl = ""
        }
    };

    private bool _isInitialized;
    private Ref<GameSettings> _activeGameSettings;
    private ISubject<GameSettings> _gameSettingsChanges;

    void Awake() {
        Initialize();
    }

    void Initialize() {
        if (!_isInitialized) {
            _activeGameSettings = new Ref<GameSettings>(_initialGameSettings);
            _gameSettingsChanges = new BehaviorSubject<GameSettings>(_initialGameSettings);
            _isInitialized = true;
        }
    }

    void OnDestroy() {
        _gameSettingsChanges.OnCompleted();
    }

    public void UpdateGameSettings(GameSettings newGameSettings) {
        Initialize();
        _activeGameSettings.V = newGameSettings;
        _gameSettingsChanges.OnNext(newGameSettings);
    }

    public GameSettings ActiveSettings {
        get {
            Initialize();
            return _activeGameSettings.V;
        }
    }

    public IReadonlyRef<GameSettings> SettingsRef {
        get {
            Initialize();
            return _activeGameSettings;
        }
    }

    public IObservable<GameSettings> SettingChanges {
        get {
            Initialize();
            return _gameSettingsChanges;
        }
    }

    public VrMode ActiveVrMode { get; set; }

    public bool IsVrActive {
        get { return ActiveVrMode != VrMode.None; }
    }
}
