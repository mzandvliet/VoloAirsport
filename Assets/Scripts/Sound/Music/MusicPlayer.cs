using UnityEngine;
using FMOD.Studio;
using Fmod = FMODUnity.RuntimeManager;

public class MusicPlayer : MonoBehaviour {
    [SerializeField] private string _introMusic;
    [SerializeField] private string[] _musicClips;

    private System.Random _random;
    private EventInstance _event;

    void Awake() {
        _random = new System.Random();
    }

    public void PlayIntro() {
        _event = Fmod.CreateInstance(_introMusic);
        _event.start();
    }

    public void PlayRandomMusic() {
        var clip = _musicClips[_random.Next(_musicClips.Length)];
        _event = Fmod.CreateInstance(clip);
        _event.start();
    }

    public void StopAndFade() {
        if (_event != null) {
            _event.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    private void OnDestroy() {
        if (_event != null) {
            _event.release();            
        }
    }
}
