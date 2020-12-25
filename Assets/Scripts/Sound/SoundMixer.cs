using System.Collections.Generic;
using FMOD.Studio;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Util;
using UnityEngine;

// 

public class SoundMixer : MonoBehaviour {
    private IDictionary<SoundLayer, Layer> _layers;

    private void Start() {
        _layers = ArrayDictionary.EnumDictionary<SoundLayer, Layer>();
        _layers.Add(SoundLayer.Effects, new Layer("bus:/global_SFX"));
        _layers.Add(SoundLayer.GameEffects, new Layer("bus:/global_SFX/game_SFX"));
        _layers.Add(SoundLayer.Music, new Layer("bus:/global_music"));
    }

    public float GetVolume(SoundLayer layer) {
        return _layers[layer].Volume;
    }

    /// <summary>
    /// Used by game components to change layer mix based on game context (in menu, in game, etc.)
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="volume"></param>
    public void SetVolume(SoundLayer layer, float volume) {
        _layers[layer].Volume = volume;
    }

    public float GetMaxVolume(SoundLayer layer) {
        return _layers[layer].MaxVolume;
    }

    /// <summary>
    /// Used by user-settings to define maximum audible levels
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="maxVolume"></param>
    public void SetMaxVolume(SoundLayer layer, float maxVolume) {
        _layers[layer].MaxVolume = maxVolume;
    }

    public void Pause(SoundLayer layer) {
        _layers[layer].Pause(isPaused: true);
    }

    public void Unpause(SoundLayer layer) {
        _layers[layer].Pause(isPaused: false);
    }

    private class Layer {
        private float _maxVolume;
        private float _volume;
        private readonly Bus _bus;

        public Layer(string busPath) {
            _bus = FMODUtil.GetBus(busPath);
            _volume = 1f;
            _maxVolume = 1f;
        }

        public void Pause(bool isPaused) {
            _bus.setPaused(isPaused);
        }

        public float MaxVolume {
            get { return _volume; }
            set {
                _maxVolume = value;
                Apply();
            }
        }

        public float Volume {
            get { return _volume; }
            set {
                _volume = value;
                Apply();
            }
        }

        private void Apply() {
            _bus.setFaderLevel(_maxVolume * _volume);
        }
    }
}

public enum SoundLayer {
    Music,
    Effects,
    GameEffects,
}