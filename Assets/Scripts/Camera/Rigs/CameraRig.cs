using System;
using System.Collections.Generic;
using RamjetAnvil.Cameras;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;
using Bloom = UnityStandardAssets.ImageEffects.Bloom;
using Motion = Kino.Motion;

public interface ICameraRig : IComponent {
    void Initialize();

    IScreenFader ScreenFader { get; }
    CameraShake Shake { get; }
    CameraEffects Effects { get; }
    IList<Camera> Cameras { get; }
    float FieldOfView { get; set; }
    HudAudioSource HudAudio { get; }

    void ApplySettings(GameSettings settings);
    void Teleport();

    event Action OnTeleported;
}

public abstract class CameraRig : MonoBehaviour, ICameraRig
{
    public abstract IScreenFader ScreenFader { get; }
    public abstract CameraShake Shake { get; }
    public CameraEffects Effects { get { return _effects; } }
    public abstract IList<Camera> Cameras { get; }
    public abstract float FieldOfView { get; set; }
    public abstract HudAudioSource HudAudio { get; }

    private bool _initialized;
    private CameraEffects _effects;
    private List<ICameraTeleportHandler> _teleportHandlers;
    public event Action OnTeleported;

    [SerializeField]
    private GameObject _ecologyEffectsPrefab;

    private void Awake() {
        Initialize();
    }

    public void Initialize() {
        if (!_initialized) {

            _teleportHandlers = new List<ICameraTeleportHandler>();
            gameObject.GetComponentsInChildren<ICameraTeleportHandler>(_teleportHandlers);

            _effects = new CameraEffects {
                Bloom = gameObject.GetComponentInChildren<Bloom>(),
                MotionBlur = gameObject.GetComponentInChildren<Motion>(),
                AntiAliasing = gameObject.GetComponentInChildren<AntiAliasing>()
            };

            var ecoEffects = Instantiate(_ecologyEffectsPrefab, transform.position, transform.rotation);
            ecoEffects.transform.parent = transform;

            OnInitialize();

            _initialized = true;
        }
    }

    protected virtual void OnInitialize() {
        
    }

    public virtual void ApplySettings(GameSettings settings) {
        if (!_initialized) {
            Initialize();
        }

        // Note: comparisons below are NOT redundant. Changing the properties triggers heavy sideffects.

        if (_effects.Bloom != null) {
            if (settings.Graphics.BloomQuality == BloomQuality.Cheap &&

            _effects.Bloom.quality != Bloom.BloomQuality.Cheap) {
                _effects.Bloom.quality = Bloom.BloomQuality.Cheap;
            } else if (settings.Graphics.BloomQuality == BloomQuality.High &&
                  _effects.Bloom.quality != Bloom.BloomQuality.High) {
                _effects.Bloom.quality = Bloom.BloomQuality.High;
            }
            var isBloomEnabled = settings.Graphics.BloomQuality > BloomQuality.Off;
            if (_effects.Bloom.enabled != isBloomEnabled) {
                _effects.Bloom.enabled = isBloomEnabled;
            }
        }

        if (_effects.AntiAliasing != null) {
            bool shouldBeEnabled = settings.Graphics.AntiAliasingMode != AntiAliasingMode.Off;
            if (_effects.AntiAliasing.enabled != shouldBeEnabled) {
                _effects.AntiAliasing.enabled = shouldBeEnabled;
            }

            int method = GetAntiAliasingMethod(settings.Graphics.AntiAliasingMode);
            if (_effects.AntiAliasing.method != method) {
                _effects.AntiAliasing.method = method;
            }

            if (settings.Graphics.AntiAliasingMode == AntiAliasingMode.Fxaa) {
                var fxaa = (FXAA)_effects.AntiAliasing.current;
                var preset = GetFxaaPreset(settings.Graphics.AntiAliasingQuality);
                if (!fxaa.preset.Equals(preset)) {
                    fxaa.preset = preset;
                }
            } else if (settings.Graphics.AntiAliasingMode == AntiAliasingMode.Smaa) {
                var smaa = (SMAA)_effects.AntiAliasing.current;
                var preset = GetMsaaPreset(settings.Graphics.AntiAliasingQuality);
                if (!smaa.settings.quality.Equals(preset)) {
                    smaa.settings.quality = preset;
                }
            }
        }

        if (_effects.MotionBlur != null) {
            var isMotionBlurEnabled = settings.Graphics.MotionBlurQualitySteps > 0;
            if (_effects.MotionBlur.enabled != isMotionBlurEnabled) {
                _effects.MotionBlur.enabled = isMotionBlurEnabled;
            }
            if (_effects.MotionBlur.sampleCount != settings.Graphics.MotionBlurQualitySteps && isMotionBlurEnabled) {
                _effects.MotionBlur.sampleCount = settings.Graphics.MotionBlurQualitySteps;
            }
        }
    }

    /// <summary>
    /// This lets related systems like the grass streaming manager know we've teleported,
    /// which is important for how and when such systems anticipate camera motion.
    /// 
    /// Todo: Move this to camera manager? Saves us a lot of indirection.
    /// </summary>
    public virtual void Teleport() {
        for (int i = 0; i < _teleportHandlers.Count; i++) {
            _teleportHandlers[i].OnTeleported();
        }

        if (OnTeleported != null) {
            OnTeleported();
        }
    }

    private int GetAntiAliasingMethod(AntiAliasingMode mode) {
        switch (mode) {
            case AntiAliasingMode.Off:
            case AntiAliasingMode.Fxaa:
                return (int)AntiAliasing.Method.Fxaa;
            case AntiAliasingMode.Smaa:
                return (int)AntiAliasing.Method.Smaa;
            default:
                throw new ArgumentOutOfRangeException("mode", mode, null);
        }
    }

    private FXAA.Preset GetFxaaPreset(AntiAliasingQuality quality) {
        switch (quality) {
            case AntiAliasingQuality.Low:
                return FXAA.Preset.performancePreset;
            case AntiAliasingQuality.Medium:
                return FXAA.Preset.defaultPreset;
            case AntiAliasingQuality.High:
                return FXAA.Preset.qualityPreset;
            case AntiAliasingQuality.Ultra:
                return FXAA.Preset.extremeQualityPreset;
            default:
                throw new ArgumentOutOfRangeException("quality", quality, null);
        }
    }

    private SMAA.QualityPreset GetMsaaPreset(AntiAliasingQuality quality) {
        switch (quality) {
            case AntiAliasingQuality.Low:
                return SMAA.QualityPreset.Low;
            case AntiAliasingQuality.Medium:
                return SMAA.QualityPreset.Medium;
            case AntiAliasingQuality.High:
                return SMAA.QualityPreset.High;
            case AntiAliasingQuality.Ultra:
                return SMAA.QualityPreset.Ultra;
            default:
                throw new ArgumentOutOfRangeException("quality", quality, null);
        }
    }
}

namespace RamjetAnvil.Cameras {
    public static class CameraRigExtensions {

        public static Camera GetMainCamera(this ICameraRig cameraRig) {
            return cameraRig.Cameras[0];
        }
    }

    public class CameraEffects {
        private Bloom _bloom;
        private AntiAliasing _antiAliasing;
        private Motion _motionBlur;

        public AntiAliasing AntiAliasing {
            get { return _antiAliasing; }
            set { _antiAliasing = value; }
        }
        
        public Bloom Bloom {
            get { return _bloom; }
            set { _bloom = value; }
        }

        public Motion MotionBlur {
            get { return _motionBlur; }
            set { _motionBlur = value; }
        }
    }
}
