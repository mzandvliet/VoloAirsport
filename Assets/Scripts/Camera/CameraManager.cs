using System;
using System.Collections.Generic;
using RamjetAnvil.Cameras;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField] private GameObject[] _rigPrefabs;
    [Dependency, SerializeField] private AbstractUnityClock _clock;

    private ICameraMount _mount;
    private ICameraRig _rig;
    private VrMode _vrMode;

    private bool _transitionActive;

    public ICameraRig Rig {
        get { return _rig; }
    }

    public bool IsRigInitialized {
        get { return _rig != null; }
    }

    public VrMode VrMode {
        get { return _vrMode; }
    }

    private void LateUpdate() {
        if (_mount == null || _transitionActive) {
            return;
        }

        _rig.transform.position = _mount.transform.position;
        _rig.transform.rotation = _mount.transform.rotation;
    }

    public ICameraRig CreateCameraRig(VrMode mode, DependencyContainer dependencyContainer) {
        _vrMode = mode;

        var prefab = _rigPrefabs[(int)mode];

        prefab.SetActive(false);
        var cameraRig = Instantiate(prefab, new Vector3(0f, 0f, -6f), Quaternion.identity);
        prefab.SetActive(true);

        _rig = cameraRig.GetComponent<ICameraRig>();
        _rig.Initialize();

        DependencyInjector.Default.Inject(cameraRig, dependencyContainer);
        cameraRig.SetActive(true);

        // VectorLine.SetCamera3D(_rig.GetMainCamera()); // Todo: Replace Vectrocity

        return _rig;
    }

    public void SwitchMount(ICameraMount mount) {
        if (_transitionActive) {
            Debug.LogError("Attempting to transition between mounts while another transition is still active");
            return;
        }

        if (mount == null) {
            throw new ArgumentException("Mount cannot be null");
        }

        if (_mount != null) {
            _mount.OnDismount(Rig);
        }

        _mount = mount;
        if (_mount != null) {
            _mount.OnMount(Rig);
        }

        // Timeless transition implies teleportation, so inform the mounts
        _rig.Teleport();
    }

    public IEnumerator<WaitCommand> AnimateToMount(ICameraMount mount, TimeSpan duration) {
        if (_transitionActive) {
            Debug.LogError("Attempting to transition between mounts while another transition is still active");
            yield break;
        }

        _transitionActive = true;

        yield return Routines.Animate(_clock.PollDeltaTime, duration, lerp => {
            _rig.transform.position = Vector3.Lerp(_mount.transform.position, mount.transform.position, lerp);
            _rig.transform.rotation = Quaternion.Slerp(_mount.transform.rotation, mount.transform.rotation, lerp);
        }).AsWaitCommand();

        if (_mount != null) {
            _mount.OnDismount(Rig);
        }

        _mount = mount;
        if (_mount != null) {
            _mount.OnMount(Rig);
        }

        _transitionActive = false;
    }
}
