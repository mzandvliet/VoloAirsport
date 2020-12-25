using FMOD;
using FMOD.Studio;
using FMODUnity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityExecutionOrder;
using Fmod = FMODUnity.RuntimeManager;

[Run.After(typeof(Airfoil1D))]
public class ParachuteWingSound : MonoBehaviour, ISpawnable {
    [SerializeField] private ParachuteAirfoil _wing;
    [SerializeField] private string _eventName;
    [SerializeField] private EventInstance _emitter;

    public ParachuteAirfoil Wing {
        get { return _wing; }
        set { _wing = value; }
    }

    public string EventName {
        get { return _eventName; }
        set { _eventName = value; }
    }

    private Transform _transform;
    private ParameterInstance _airspeedParam;
    private float _airspeed;
    private ParameterInstance _angleOfAttackParam;
    private float _angleOfAttack;

    private Vector3 _prevPosition;

    public void Initialize()
    {
        _transform = gameObject.GetComponent<Transform>();

        _emitter = Fmod.CreateInstance(_eventName);
        _emitter.getParameter("airspeed", out _airspeedParam);
        _emitter.getParameter("AoA", out _angleOfAttackParam);
    }

    public void OnSpawn()
    {
        _emitter.start();
        _prevPosition = _transform.position;
    }

    public void OnDespawn()
    {
        _emitter.stop(STOP_MODE.ALLOWFADEOUT);

        _airspeedParam.setValue(0f);
        _angleOfAttackParam.setValue(0f);
    }

    private void Update()
    {
        // Todo: handle paused 0 deltatime
        Vector3 velocity = (_prevPosition - _transform.position) / Time.deltaTime;
        _prevPosition = _transform.position;

        ATTRIBUTES_3D attributes = new ATTRIBUTES_3D();
        attributes.forward = _transform.forward.ToFMODVector();
        attributes.up = _transform.up.ToFMODVector();
        attributes.position = _transform.position.ToFMODVector();
        attributes.velocity = velocity.ToFMODVector();
        _emitter.set3DAttributes(attributes);

        _airspeed = Mathf.Lerp(_airspeed, _wing.AirSpeed * 2f, 20f * Time.deltaTime);
        _airspeedParam.setValue(_airspeed);

        _angleOfAttack = Mathf.Lerp(_angleOfAttack, _wing.AngleOfAttack * 3f, 20f * Time.deltaTime);
        _angleOfAttackParam.setValue(_angleOfAttack);
    }
}
