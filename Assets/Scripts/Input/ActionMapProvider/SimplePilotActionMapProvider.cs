using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public class SimplePilotActionMapProvider : PilotActionMapProvider {
    [Dependency, SerializeField] private AbstractUnityClock _clock;

    private bool _isOff;
    private PilotActionMap _currentActionMap;
    private Ref<PilotActionMap> _actionMap;

    void Awake() {
        Initialize();
    }

    public override IReadonlyRef<PilotActionMap> ActionMapRef {
        get {
            Initialize();
            return _actionMap;
        }
    }

    public override PilotActionMap ActionMap {
        get {
            Initialize();
            return _actionMap.V;
        }
    }

    public override void SetInputMappingSource(IObservable<ActionMapConfig<WingsuitAction>> actionMapConfigChanges) {
        actionMapConfigChanges.Subscribe(actionMapConfig => {
            var newActionMap = PilotInput.ActionMap.Create(actionMapConfig, _clock);
            SetActionMap(newActionMap);
        });
    }

    private void Initialize() {
        if (_actionMap == null) {
            var defaultActionMap = CreateDefault();
            _actionMap = new Ref<PilotActionMap>(_isOff ? PilotActionMap.NoOpActionMap : defaultActionMap);
            SetActionMap(defaultActionMap);
        }
    }

    private PilotActionMap CreateDefault() {
        // TODO Merge multiple mappings into one so we can have
        // keyboard and controllers active at the same time.
        return PilotInput.ActionMap.Create(
            new ActionMapConfig<WingsuitAction> {
                ControllerId = null,
                InputSettings = InputSettings.Default,
                InputMapping = PilotInput.Bindings.DefaultMapping.Value
            }, 
            _clock);
    }

    private void SetActionMap(PilotActionMap actionMap) {
        Initialize();

        _currentActionMap = actionMap;
        if (!_isOff) {
            _actionMap.V = actionMap;
        }
    }
}
