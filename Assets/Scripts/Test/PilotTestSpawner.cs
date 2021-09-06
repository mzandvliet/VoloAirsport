using RamjetAnvil.Volo.Input;
using UnityEngine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;

public class PilotTestSpawner : MonoBehaviour {
    [SerializeField] private GameObject _pilot;

    [SerializeField] private int _controllerId = 0;
    [SerializeField] private AbstractUnityClock _gameClock;
    [SerializeField] private AbstractUnityClock _fixedClock;
    [SerializeField] private AbstractUnityEventSystem _eventSystem;
    [SerializeField] private WindManager _windManager;

    private IPooledObject<GameObject> _pooledPilot;
    private IObjectPool<GameObject> _pilotPool;
    private ImmutableTransform _originalTransform;

    private void Awake() {
        _pilotPool = new ObjectPool<GameObject>(() => _pilot);

        var container = new DependencyContainer();
        container.AddDependency("eventSystem", _eventSystem);
        container.AddDependency("windManager", _windManager);
        container.AddDependency("gameClock", _gameClock);
        container.AddDependency("fixedClock", _fixedClock);

        DependencyInjector.Default.Inject(_pilot, container);

        _originalTransform = _pilot.transform.MakeImmutable();

        _pooledPilot = _pilotPool.Take();
    }

    private void Update() {
    }
}
