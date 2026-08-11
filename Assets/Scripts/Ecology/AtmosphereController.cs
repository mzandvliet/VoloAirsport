using UnityEngine;

/// <summary>
/// Feeds the shared atmospheric scattering model (AtmosphericScattering.cginc) used by both
/// Skybox/AtmosphericScattering and Custom/AtmosphericFog. Replaces the parameter-pushing
/// half of the stripped Time of Day plugin.
///
/// Everything here is a global shader property, because the sky shader and the fullscreen
/// fog pass have to agree exactly - if they disagreed about the sun or the haze density, the
/// horizon would show a seam.
///
/// Deliberately self-sufficient: finds the sun and the Ecology component itself, so it works
/// by just being dropped on a GameObject. It avoids [Dependency] on purpose - injected
/// components in this project are authored disabled and enabled by the injector, which is an
/// easy trap for something that must run every frame.
/// </summary>
[ExecuteInEditMode]
public class AtmosphereController : MonoBehaviour {

    [Header("Sun")]
    [Tooltip("Leave empty to use RenderSettings.sun, or the brightest directional light.")]
    [SerializeField] private Light _sun;
    [Tooltip("Scales the sun colour driving the scattering. Raise for a brighter sky.")]
    [SerializeField] private float _sunIntensityScale = 20f;

    [Header("Atmosphere")]
    [SerializeField] private float _planetRadius = 6371000f;
    [SerializeField] private float _atmosphereHeight = 60000f;
    [Tooltip("World Y treated as sea level.")]
    [SerializeField] private float _groundHeight = 0f;
    [SerializeField] private float _rayleighScaleHeight = 8000f;
    [SerializeField] private float _mieScaleHeight = 1200f;

    [Tooltip("Per-metre Rayleigh scattering, x1e-6. The RGB imbalance is what makes the sky " +
             "blue and sunsets red - blue scatters (and so extincts) far more strongly.")]
    [SerializeField] private Vector3 _rayleighCoefficient = new Vector3(5.8f, 13.5f, 33.1f);
    [Tooltip("Per-metre Mie scattering, x1e-6. Raise for a hazier, whiter atmosphere.")]
    [SerializeField] private float _mieCoefficient = 21f;
    [Range(0f, 0.95f)]
    [Tooltip("Mie forward-scattering anisotropy - controls the glow around the sun.")]
    [SerializeField] private float _mieAnisotropy = 0.76f;

    [Header("Ground")]
    [Tooltip("Stand-in planet surface, so view rays below the horizon that miss real terrain " +
             "terminate on something instead of integrating out into space. Only visible " +
             "beyond the terrain tiles, where haze has fully saturated - so in practice this " +
             "tints the far ground haze rather than shading anything legible.")]
    [SerializeField] private Color _groundAlbedo = new Color(0.29f, 0.28f, 0.26f);
    [SerializeField] private float _groundBrightness = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float _groundAmbient = 0.1f;

    [Header("Exposure")]
    [SerializeField] private float _exposure = 1.0f;
    [Tooltip("Colour floor so the sky doesn't go pure black at night.")]
    [SerializeField] private Color _nightColor = new Color(0.005f, 0.008f, 0.02f);

    [Header("Weather response")]
    [Tooltip("Overall haze multiplier at Ecology's minimum and maximum FogIntensity.")]
    [SerializeField] private float _densityAtClearWeather = 0.8f;
    [SerializeField] private float _densityAtFoggyWeather = 2.5f;
    [Tooltip("Used when no Ecology component is present (e.g. in an isolated test scene).")]
    [SerializeField] private float _densityWithoutEcology = 1f;

    [Header("Misc")]
    [Tooltip("Unity's built-in fog would double up with the AtmosphericFog image effect.")]
    [SerializeField] private bool _disableBuiltinFog = true;

    private Ecology _ecology;

    private void OnEnable() {
        if (_disableBuiltinFog) {
            RenderSettings.fog = false;
        }
        PushGlobalShaderParams();
    }

    private void LateUpdate() {
        PushGlobalShaderParams();
    }

    private Light ResolveSun() {
        if (_sun != null) {
            return _sun;
        }
        if (RenderSettings.sun != null) {
            return RenderSettings.sun;
        }
        Light brightest = null;
        float best = float.NegativeInfinity;
        var lights = FindObjectsOfType<Light>();
        for (int i = 0; i < lights.Length; i++) {
            if (lights[i].type == LightType.Directional && lights[i].intensity > best) {
                best = lights[i].intensity;
                brightest = lights[i];
            }
        }
        return brightest;
    }

    private float WeatherDensityMultiplier() {
        if (_ecology == null) {
            _ecology = FindObjectOfType<Ecology>();
        }
        if (_ecology == null) {
            return _densityWithoutEcology;
        }
        // Ecology.FogIntensity is a 0..1 seasonal signal. Squared to match the shaping
        // CameraEcologyEffects already applies to it for the fog particle systems.
        float fogginess = Mathf.Clamp01(_ecology.Weather.FogIntensity);
        return Mathf.Lerp(_densityAtClearWeather, _densityAtFoggyWeather, fogginess * fogginess);
    }

    private void PushGlobalShaderParams() {
        var sun = ResolveSun();
        if (sun != null) {
            // Pointing *toward* the sun, which is what the phase functions expect.
            Shader.SetGlobalVector("_AtmosSunDir", -sun.transform.forward);
            Shader.SetGlobalColor("_AtmosSunColor", sun.color * sun.intensity * _sunIntensityScale);
        } else {
            Shader.SetGlobalVector("_AtmosSunDir", Vector3.up);
            Shader.SetGlobalColor("_AtmosSunColor", Color.black);
        }

        Shader.SetGlobalColor("_AtmosNightColor", _nightColor);

        Shader.SetGlobalColor("_AtmosGroundAlbedo", _groundAlbedo);
        Shader.SetGlobalFloat("_AtmosGroundBrightness", _groundBrightness);
        Shader.SetGlobalFloat("_AtmosGroundAmbient", _groundAmbient);

        Shader.SetGlobalFloat("_AtmosPlanetRadius", _planetRadius);
        Shader.SetGlobalFloat("_AtmosAtmosphereHeight", _atmosphereHeight);
        Shader.SetGlobalFloat("_AtmosGroundHeight", _groundHeight);
        Shader.SetGlobalFloat("_AtmosRayleighScaleHeight", _rayleighScaleHeight);
        Shader.SetGlobalFloat("_AtmosMieScaleHeight", _mieScaleHeight);

        Shader.SetGlobalVector("_AtmosRayleighCoeff", _rayleighCoefficient * 1e-6f);
        Shader.SetGlobalFloat("_AtmosMieCoeff", _mieCoefficient * 1e-6f);
        Shader.SetGlobalFloat("_AtmosMieG", _mieAnisotropy);

        Shader.SetGlobalFloat("_AtmosDensityMultiplier", WeatherDensityMultiplier());
        Shader.SetGlobalFloat("_AtmosExposure", _exposure);
    }
}
