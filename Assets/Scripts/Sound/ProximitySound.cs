using FMOD.Studio;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using UnityExecutionOrder;
using Fmod = FMODUnity.RuntimeManager;

[Run.After(typeof(FlightStatistics))]
public class ProximitySound : MonoBehaviour, ISpawnable {
    [SerializeField]
    private FlightStatistics _statistics;

    private EventInstance _emitter;
    private ParameterInstance _altitudeGround;
    private ParameterInstance _airspeed;

    void Awake() {
        _emitter = Fmod.CreateInstance("event:/wingsuit/proximity_ground");
        _emitter.getParameter("altitude_ground", out _altitudeGround);
        _emitter.getParameter("airspeed", out _airspeed);
    }

    public void OnSpawn() {
        _emitter.start();
    }

    public void OnDespawn() {
        _emitter.stop(STOP_MODE.IMMEDIATE);
        _altitudeGround.setValue(100f);
        _airspeed.setValue(0f);
    }

    void Update() {
        _altitudeGround.setValue(_statistics.AltitudeGround);
		_airspeed.setValue(_statistics.TrueAirspeed);
    }
}