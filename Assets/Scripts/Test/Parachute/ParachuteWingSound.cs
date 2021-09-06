using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityExecutionOrder;

[Run.After(typeof(Airfoil1D))]
public class ParachuteWingSound : MonoBehaviour, ISpawnable {
    [SerializeField] private ParachuteAirfoil _wing;
    [SerializeField] private string _eventName;

    public ParachuteAirfoil Wing {
        get { return _wing; }
        set { _wing = value; }
    }

    public string EventName {
        get { return _eventName; }
        set { _eventName = value; }
    }

    private Transform _transform;
    private float _airspeed;
    private float _angleOfAttack;

    private Vector3 _prevPosition;

    public void Initialize()
    {
        _transform = gameObject.GetComponent<Transform>();
    }

    public void OnSpawn()
    {
        _prevPosition = _transform.position;
    }

    public void OnDespawn()
    {
    }

    private void Update()
    {
        // Todo: handle paused 0 deltatime
        Vector3 velocity = (_prevPosition - _transform.position) / Time.deltaTime;
        _prevPosition = _transform.position;
    }
}
