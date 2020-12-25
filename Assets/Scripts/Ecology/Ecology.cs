using System;
using UnityEngine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityStandardAssets.ImageEffects;

/* Todo:
 * - This does too much camera and particle stuff, is just too dependent on it.
 * - All this should really do is do the weather simulation, not the application to rendering or other systems
 *
 */

public class Ecology : MonoBehaviour {
    [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;
    [SerializeField, Dependency] private GameSettingsProvider _gameSettingsProvider;
    [SerializeField, Dependency] private WindManager _wind;
    [SerializeField] private float _nightLightIntensityThreshold = 2f; // Used to determine whether it is night

    // [SerializeField, Dependency] private TOD_Sky _sky; // Todo: replace with non-prorietary atmospheric scattering
    [SerializeField, Dependency] private StaticTiledTerrain _terrainManager;

    private TimeSettings _time;
    private WeatherSettings _weather;

    private IDisposable _settingUpdates;

    public TimeSettings Time {
        get { return _time; }
    }

    public WeatherSettings Weather {
        get { return _weather; }
    }

    public bool IsNight {
        // get { return _sky && _sky.LightIntensity < _nightLightIntensityThreshold; }
        get { return false; }
    }

    private void Awake() {
        _time = new TimeSettings();
        _time.DayLengthInMinutes = 2f;
        _time.CurrentDateTime = new DateTime(2015, 3, 1, 5, 0, 0);
        _time.IsNightSkipped = false;
        _time.IsTimeSimulated = true;
        // Lauterbrunnen, Switzerland
        _time.Longitude = -28f;
        _time.Latitude = 55f;

        _weather = new WeatherSettings();
        _weather.SnowAltitude = 2750f;
        _weather.IsWeatherSimulated = true;
        _weather.DaysPerSeason = 2f;
    }

    void OnEnable() {
        _settingUpdates = _gameSettingsProvider.SettingChanges.Subscribe(settings => {
            _time = settings.Gameplay.Time;
            _weather = settings.Gameplay.Weather;
        });
    }

    void OnDisable() {
        _settingUpdates.Dispose();    
    }

	void Update () {
        const float minutesPerDay = 1440;
	    const float nightSpeedFactor = 10;

	    const float minSnowAltitude = 2300f;
	    const float maxSnowAltitude = 3300f;

	    var timeSpeed = (minutesPerDay / _time.DayLengthInMinutes);
	    if (_time.IsTimeSimulated) {
            float nightSkipMultiplier = 1f;
            if (_time.IsNightSkipped) {
                nightSkipMultiplier = nightSpeedFactor * _time.DayLengthInMinutes;
            }

            var deltaSeconds = _clock.DeltaTime * timeSpeed;
            _time.CurrentDateTime = _time.CurrentDateTime.AddSeconds(deltaSeconds * nightSkipMultiplier);
	    }
        ApplyTimeSettings();

        /* Todo:
         * 
         * - Add day of year slider in options menu
         * - Summer/Winter switch in options screen feels like it should instantly change the weather
         */

	    if (_weather.IsWeatherSimulated) {
            double daysPassed = TimePassedSinceBeginningOfYear(_time.CurrentDateTime).TotalDays;
	        float seasonLerp = (float) (0.5 + 0.5 * Math.Sin(daysPassed * Math.PI / _weather.DaysPerSeason));

            _weather.SnowAltitude = Mathf.Lerp(minSnowAltitude, maxSnowAltitude, 1f - seasonLerp);

            _weather.SnowfallIntensity = seasonLerp * (float)(0.5 + 0.5 * Math.Sin(daysPassed * MathUtils.TwoPi * 2.5));
            _weather.FogIntensity = seasonLerp * (float)(0.5 + 0.5 * Math.Sin(daysPassed * MathUtils.TwoPi * 1.5));

	        //Debug.Log(daysPassed + ", " + seasonLerp);
	    }
        ApplyWeatherSettings();
	}

    private static TimeSpan TimePassedSinceBeginningOfYear(DateTime dateTime) {
        return dateTime - new DateTime(1970, 1, 1, 0, 0, 0);
    }

    private void ApplyTimeSettings() {
        // if (_sky != null) {
        //     _sky.World.UTC = 0f;
	    //     _sky.World.Longitude = _time.Longitude;
	    //     _sky.World.Latitude = _time.Latitude;
        //     _sky.Cycle.DateTime = _time.CurrentDateTime;   
        // }
    }

    private void ApplyWeatherSettings() {
        if (_terrainManager != null) {
            var terrainSettings = _terrainManager.TerrainConfiguration;
	        terrainSettings.SnowAltitude = _weather.SnowAltitude;
            terrainSettings.Fogginess = _weather.FogIntensity;
            _terrainManager.ApplyWeatherSettings(terrainSettings);            
        }
    }

    public struct TimeSettings {
        public bool IsTimeSimulated;
        public DateTime CurrentDateTime;
        public float DayLengthInMinutes;
        public bool IsNightSkipped;
        public float Longitude;
        public float Latitude;
    }

    public struct WeatherSettings {
        public float Season;
        public bool IsWeatherSimulated;
        public float SnowAltitude;
        public float SnowfallIntensity;
        public float FogIntensity;
        public float DaysPerSeason;
    }
}
