using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Util;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using XInputDotNetPure;

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
    private PilotActionMap _pilotActionMap;

    private void Awake() {
        var inputMapping = PilotInput.Bindings.DefaultXbox360Mapping.Value;

        _pilotActionMap = PilotInput.ActionMap.Create(
            new ActionMapConfig<WingsuitAction> {
                ControllerId = new ControllerId.XInput((PlayerIndex)_controllerId),
                InputSettings = InputSettings.Default,
                InputMapping = inputMapping
            }, _gameClock);

        _pilotPool = new ObjectPool<GameObject>(() => _pilot);

        var container = new DependencyContainer();
        container.AddDependency("eventSystem", _eventSystem);
        container.AddDependency("windManager", _windManager);
        container.AddDependency("actionMap", new Ref<PilotActionMap>(_pilotActionMap));
        container.AddDependency("gameClock", _gameClock);
        container.AddDependency("fixedClock", _fixedClock);

        DependencyInjector.Default.Inject(_pilot, container);

        _originalTransform = _pilot.transform.MakeImmutable();

        _pooledPilot = _pilotPool.Take();
    }

    private void Update() {
        if (_pilotActionMap.PollButtonEvent(WingsuitAction.Respawn) == ButtonEvent.Down) {
            _pooledPilot.Dispose();
            _pilot.transform.Set(_originalTransform);
            _pooledPilot = _pilotPool.Take();
        }
    }
}
