using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.VR;

public class OpenVrCameraRig : CameraRig {
    [SerializeField]
    private Camera[] _cameras;
    [SerializeField]
    private ScreenFaderEffect _screenFader;
    [SerializeField]
    private HudAudioSource _hudAudio;

    [Dependency, SerializeField] private MenuActionMapProvider _menuActionMapProvider;

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
        // Todo: check attached devices and all that shit, initialize properly
    }

    private void Update() {
        if (_menuActionMapProvider.ActionMap.V.PollButton(MenuAction.RecenterVrHeadset) == ButtonState.Pressed) {
            Debug.Log("Recentering...");
            Valve.VR.OpenVR.System.ResetSeatedZeroPose();
        }
    }
}