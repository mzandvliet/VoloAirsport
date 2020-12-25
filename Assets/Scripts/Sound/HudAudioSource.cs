using UnityEngine;
using Fmod = FMODUnity.RuntimeManager;
/*
 * Todo:
 * This system is pretty bad right now.
 * 
 * - Use a pool of sources and loop through them to have multiple voice support
 * - Make a single instance of this in the scene to avoid duplicate state
 * - Accessing this system through the cameramanager/rig is not ideal.
 * - If clients cache references to this system the might nullpointer later.
 * 
 */

/*
 * course/start
 * course/win
 * course/pass
 */

public class HudAudioSource : MonoBehaviour {
    [SerializeField] private InterfaceSound[] _sounds;

    public void Play(string id) {
        for (int i = 0; i < _sounds.Length; i++) {
            var sound = _sounds[i];
            if (id.Equals(sound.Id)) {
                Fmod.PlayOneShot(sound.FmodEventId, Vector3.zero);
            }
        }
    }

    [System.Serializable]
    public class InterfaceSound {
        public string Id;
        public FMODAsset Asset;
        public string FmodEventId;
    }
}
