using FMOD.Studio;
using FMODUnity;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using UnityExecutionOrder;

[RequireComponent(typeof(FMOD_StudioEventEmitter))]
[Run.After(typeof(FlightStatistics))]
public class EarDistortionSound : MonoBehaviour, ISpawnable {
    [SerializeField, Dependency] private WindManager _wind;
    [SerializeField, Dependency("gameClock")] private AbstractUnityClock _gameClock;

    [SerializeField] private FlightStatistics _statistics;
    private Transform _transform;
    private EventInstance _emitter;
    private ParameterInstance _airspeedParam;
    private ParameterInstance _angleOfAttackParam;
    private ParameterInstance _groundProximityParam;

    private float _airspeed;
    private float _angleOfAttack;

    private Vector3 _lastPosition;

    void Awake() {
        _transform = gameObject.GetComponent<Transform>();
        _emitter = RuntimeManager.CreateInstance("event:/wingsuit/fp_earDistortion");
        _emitter.getParameter("airspeed", out _airspeedParam);
        _emitter.getParameter("AoA", out _angleOfAttackParam);
        _emitter.getParameter("proximity_ground", out _groundProximityParam);
    }

    public void OnSpawn() {
        _emitter.start();
    }

    public void OnDespawn() {
        _emitter.stop(STOP_MODE.IMMEDIATE);
        _airspeedParam.setValue(0f);
        _angleOfAttackParam.setValue(0f);
        _groundProximityParam.setValue(100f);
    }

    void OnEnable() {
        _lastPosition = _transform.position;        
    }

    void Update() {
        Vector3 windVelocity = _wind.GetWindVelocity(_transform.position);
        Vector3 velocity = (_transform.position - _lastPosition) / _gameClock.DeltaTime;
        _lastPosition = _transform.position;
        Vector3 relativeVelocity = velocity - windVelocity;
        relativeVelocity.Normalize();

        _airspeed = Mathf.Lerp(_airspeed, _statistics.TrueAirspeed, 20f * _gameClock.DeltaTime);
        _angleOfAttack = Mathf.Lerp(_angleOfAttack, _statistics.AngleOfAttack, 20f * _gameClock.DeltaTime);

        _airspeedParam.setValue(_airspeed);
        _angleOfAttackParam.setValue(_angleOfAttack);
        _groundProximityParam.setValue(_statistics.AltitudeGround);
    }
}