using UnityEngine;
using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;

public class SimpleRig : CameraRig {
    [SerializeField]
    private Camera[] _cameras;
    [SerializeField]
    private ScreenFaderEffect _screenFader;
    [SerializeField]
    private HudAudioSource _hudAudio;
    [SerializeField]
    private CameraShake _shake;

    public override IScreenFader ScreenFader {
        get { return _screenFader; }
    }

    public override CameraShake Shake {
        get { return _shake; }
    }

    public override IList<Camera> Cameras {
        get { return _cameras; }
    }

    public override float FieldOfView {
        get { return _cameras[0].fieldOfView; }
        set {
            for (int i = 0; i < _cameras.Length; i++) {
                _cameras[i].fieldOfView = value;
            }
        }
    }

    public override HudAudioSource HudAudio {
        get { return _hudAudio; }
    }

    // Todo: let this class specify a camerasettings class, such that it becomes decoupled from gamesettingsmanager
    public override void ApplySettings(GameSettings settings) {
        base.ApplySettings(settings);

        for (var i = 0; i < _cameras.Length; i++) {
            var cam = _cameras[i];

            if (Mathf.Abs(cam.fieldOfView - settings.Graphics.FieldOfView) > float.Epsilon) {
                cam.fieldOfView = settings.Graphics.FieldOfView;
            }
        }
    }
}
