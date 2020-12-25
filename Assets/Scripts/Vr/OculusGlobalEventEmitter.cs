using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using UnityEngine;

namespace RamjetAnvil.Unity.Vr {
    public class OculusGlobalEventEmitter : MonoBehaviour {
        [Dependency, SerializeField] private AbstractUnityEventSystem _eventSystem;

        private bool? _isOculusHomeCurrentlyOpened;

        void Awake() {
            _isOculusHomeCurrentlyOpened = null;
        }

        void Update() {
            var isOculusHomeOpened = !OVRPlugin.hasVrFocus && OVRPlugin.userPresent;
            if (_isOculusHomeCurrentlyOpened != isOculusHomeOpened) {
                _isOculusHomeCurrentlyOpened = isOculusHomeOpened;
                if (!isOculusHomeOpened) {
                    Debug.Log("VR focus acquired!");
                    _eventSystem.Emit(new Events.UnfreezeGame());
                } else {
                    Debug.Log("VR focus lost!");
                    _eventSystem.Emit(new Events.FreezeGame());
                }
            }
        }
    }
}
