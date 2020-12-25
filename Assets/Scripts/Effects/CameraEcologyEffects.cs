using UnityEngine;
using RamjetAnvil.DependencyInjection;

public class CameraEcologyEffects : MonoBehaviour {
    [SerializeField, Dependency] private Ecology _ecology;
    [SerializeField, Dependency] private GameSettingsProvider _settingsProvider;
    [SerializeField, Dependency("menuClock")] private AbstractUnityClock _menuClock;
    // [SerializeField, Dependency] private TOD_Sky _sky; // Todo: replace with non-prorietary atmospheric scattering

    [SerializeField] private float _maxTrails = 40f;
    [SerializeField] private float _maxFogParticles = 40f;

    [SerializeField] private ParticleSystem _fogParticles;
    [SerializeField] private ParticleSystem _snowParticles;

    void Update() {
        var graphicsSettings = _settingsProvider.ActiveSettings.Graphics;

        float foginess = Mathf.Pow(_ecology.Weather.FogIntensity, 2.0f);
        // if (_sky) {
        //     _sky.Atmosphere.Fogginess = foginess * 0.25f;
        // }

        if (_snowParticles && _fogParticles) {
            // Todo: push model for graphics settings to decouple this
            _fogParticles.SetEmissionRate(_maxFogParticles * foginess * graphicsSettings.MaxParticles);
            _snowParticles.SetEmissionRate(_maxTrails * graphicsSettings.MaxParticles);
        }
    }
}
