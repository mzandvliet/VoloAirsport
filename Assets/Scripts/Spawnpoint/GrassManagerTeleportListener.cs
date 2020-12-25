using UnityEngine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Landmass;
using RamjetAnvil.Volo;

public class GrassManagerTeleportListener : MonoBehaviour {
    [SerializeField, Dependency] private CameraManager _cameraManager;
    [SerializeField, Dependency] private AbstractUnityEventSystem _eventSystem;
    [SerializeField, Dependency] private GrassManager _grassManager;

    public CameraManager CameraManager {
        get { return _cameraManager; }
        set { _cameraManager = value; }
    }

    // Todo: ditch listening for player respawn events. Camera is what matters.

    void OnEnable() {
        _eventSystem.Listen<Events.PlayerSpawned>(spawned => _grassManager.OnSubjectTeleported());
        _cameraManager.Rig.OnTeleported += _grassManager.OnSubjectTeleported;
    }
}
