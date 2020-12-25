using System;
using System.Collections.Generic;
using System.Text;
using FMOD;
using Oculus.Platform;
using RamjetAnvil.Cameras;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.VR;
using Debug = UnityEngine.Debug;

public class OculusCameraRig : CameraRig {
    [SerializeField]
    private Camera[] _cameras;
    [SerializeField]
    private ScreenFaderEffect _screenFader;
    [SerializeField]
    private HudAudioSource _hudAudio;
    
    [SerializeField] private Transform[] _hmdRigs;
    [SerializeField] private Transform _eyeCenter;

    private Vector3 _basePosition;
    private Quaternion _baseRotation;

    [Dependency, SerializeField] private MenuActionMapProvider _menuActionMapProvider; // Todo: no input handling in this class

    public override IScreenFader ScreenFader {
        get { return _screenFader; }
    }

    public override CameraShake Shake {
        get { return null; }
    }

    public override IList<Camera> Cameras {
        get { return _cameras; }
    }

    public override float FieldOfView {
        get { return _cameras[0].fieldOfView; }
        set { }
    }

    public override HudAudioSource HudAudio {
        get { return _hudAudio; }
    }

    protected void OnInitialize(DependencyContainer dependencyContainer) {
        Debug.Log("Oculus: Present? " + OVRManager.isHmdPresent);
            
        VRSettings.enabled = true;

        OVRManager.HMDAcquired += () => Debug.Log("focus acquired");
        OVRManager.HMDLost += () => Debug.Log("focus lost");

        SetOculusRiftAudioOutput();
    }

    private static void SetOculusRiftAudioOutput() {
        int originalDevice;
        FMODUnity.RuntimeManager.LowlevelSystem.getDriver(out originalDevice);

        int numDrivers;
        FMODUnity.RuntimeManager.LowlevelSystem.getNumDrivers(out numDrivers);
        StringBuilder stringBuilder = new StringBuilder(256);
        System.Guid guid;
        int rate;
        SPEAKERMODE speakerMode;
        int speakerModeChannels;

        int? riftDevice = null;

        for (int i = 0; i < numDrivers; i++) {
            stringBuilder.Length = 0;
            if (
                FMODUnity.RuntimeManager.LowlevelSystem.getDriverInfo(i, stringBuilder, 256, out guid, out rate, out speakerMode, out speakerModeChannels) == RESULT.OK) {
                Debug.Log("Audio Device " + i + ": " + stringBuilder);

                // Note: GUIDs are actually different on each system and thus completely useless, so match device name
                if (stringBuilder.ToString().Equals("Headphones (Rift Audio)")) {
                    riftDevice = i;
                    break;
                }
            }
        }

        if (riftDevice.HasValue) {
            Debug.Log("Attempting to use Oculus Rift Audio Output...");
            if (FMODUnity.RuntimeManager.LowlevelSystem.setDriver(riftDevice.Value) == RESULT.OK) {
                Debug.Log("Success!");
            }
            else {
                Debug.Log("Failed! Falling back to default system output");
                FMODUnity.RuntimeManager.LowlevelSystem.setDriver(originalDevice);
            }
        }
        else {
            Debug.LogError("Failed to find Oculus Rift Audio Output, using default system output");
        }
    }

    private void Update() {
        var menuActionMap = _menuActionMapProvider.ActionMap;

        if (menuActionMap != null && menuActionMap.V.PollButtonEvent(MenuAction.RecenterVrHeadset) == ButtonEvent.Down) {
            Recenter();
        }
    }

    private void Recenter() {
        Debug.Log("Recentering...");
//        OVRManager.display.RecenterPose();

        // This recenters around all axes, not just Y
        // Todo: Save this to file and use as default next time the game runs

        _basePosition = -_eyeCenter.localPosition;
        _baseRotation = Quaternion.Inverse(_eyeCenter.localRotation);

        for (int i = 0; i < _hmdRigs.Length; i++) {
            _hmdRigs[i].localPosition = _basePosition;
            _hmdRigs[i].localRotation = _baseRotation;
        }
    }
}