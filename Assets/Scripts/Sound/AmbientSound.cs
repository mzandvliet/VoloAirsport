using FMODUnity;
using UnityEngine;
using FMOD.Studio;
using Fmod = FMODUnity.RuntimeManager;

public class AmbientSound : MonoBehaviour {

    private FMOD.Studio.EventInstance _event;
    private ParameterInstance _altitude;

    private void Awake() {
        _event = Fmod.CreateInstance("event:/Amb/amb_2D");
        _event.getParameter("altitude", out _altitude);
    }

    private void Update() {
        _altitude.setValue(Mathf.Clamp(transform.position.y, 0f, 6000f));
    }

    void OnEnable() {
        _event.start();
    }

    void OnDisable() {
        _event.stop(STOP_MODE.IMMEDIATE);
    }
}
