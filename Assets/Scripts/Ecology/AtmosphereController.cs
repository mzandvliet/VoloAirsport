using RamjetAnvil.DependencyInjection;
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
/// Takes the sun via the same injected "sunLight" dependency StaticTiledTerrain uses, so the
/// sky, the aerial perspective and the terrain shaders cannot disagree about where the sun
/// is. Falls back to RenderSettings.sun and then to a scene scan, but neither is expected to
/// fire in this project - see the comment on _sunLightTransform.
///
/// Note the [Dependency] consequence: MonoBehaviourInjector disables a component until all
/// its dependencies resolve, so this will sit disabled until the camera rig (which carries
/// the sun) has been instantiated and Resolve() has run. That is the desired behaviour here -
/// pushing a bogus sun direction for the first few frames would be worse - but it does mean
/// this component must be left ENABLED in the inspector for the injector to manage.
/// </summary>
[ExecuteInEditMode]
public class AtmosphereController : MonoBehaviour {

    [Header("Sun")]
    [Tooltip("Explicit override. Leave empty to use the injected 'sunLight' dependency, then " +
             "RenderSettings.sun, then the brightest directional light.")]
    [SerializeField] private Light _sun;

    // Same dependency StaticTiledTerrain uses to drive the _SunDir/_SunIntensity globals that
    // the terrain and grass shaders read. Sharing it is what keeps the sky, the aerial
    // perspective and the terrain lighting agreeing about where the sun is.
    //
    // This matters more than it looks: the game's only directional light lives on the
    // 'Light' GameObject inside the EcologyEffects prefab, which is instantiated at runtime
    // with the camera rig. There are no Lights in the scene files at all and
    // RenderSettings.sun is unset, so without this the fallback below is scanning for a
    // runtime-instantiated light and picking whichever happens to be brightest - fragile,
    // and it re-resolves differently depending on instantiation order.
    [Dependency("sunLight"), SerializeField] private Transform _sunLightTransform;
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
    private Light _fallbackSun;

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
        if (_sunLightTransform != null) {
            var injected = _sunLightTransform.GetComponent<Light>();
            if (injected != null) {
                return injected;
            }
        }
        if (RenderSettings.sun != null) {
            return RenderSettings.sun;
        }

        // Last resort. Cached, because this runs every frame and the scene scan is not cheap;
        // re-scanned only while nothing has been found yet, since the sun arrives partway
        // through boot along with the camera rig.
        if (_fallbackSun == null) {
            Light brightest = null;
            float best = float.NegativeInfinity;
            var lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++) {
                if (lights[i].type == LightType.Directional && lights[i].intensity > best) {
                    best = lights[i].intensity;
                    brightest = lights[i];
                }
            }
            _fallbackSun = brightest;
        }
        return _fallbackSun;
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
