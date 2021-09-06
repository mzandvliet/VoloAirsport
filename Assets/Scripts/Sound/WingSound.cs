using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityExecutionOrder;

[Run.After(typeof(Airfoil1D))]
public class WingSound : MonoBehaviour, ISpawnable {
    [Dependency("gameClock"), SerializeField] private AbstractUnityClock _clock;
    [SerializeField] private Airfoil1D _wing;
    [SerializeField] private string _eventName;

    public AbstractUnityClock Clock {
        get { return _clock; }
        set { _clock = value; }
    }

    public Airfoil1D Wing {
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

    void Awake() {
        _transform = gameObject.GetComponent<Transform>();
    }

    public void OnSpawn() {
        _prevPosition = _transform.position;
    }

    public void OnDespawn() {
    }
}
