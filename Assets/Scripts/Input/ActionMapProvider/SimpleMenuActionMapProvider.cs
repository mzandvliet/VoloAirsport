using System;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;

public class SimpleMenuActionMapProvider : MenuActionMapProvider {

    private Ref<MenuActionMap> _menuActionMap;

    void Awake() {
        GetOrCreate();
    }

    public override IReadonlyRef<MenuActionMap> ActionMap {
        get {
            return GetOrCreate();
        }
    }

    private Ref<MenuActionMap> GetOrCreate() {
        if (_menuActionMap == null) {
            _menuActionMap = new Ref<MenuActionMap>(CreateDefault());
        }
        return _menuActionMap;
    }

    private MenuActionMap CreateDefault() {
        var mapping = MenuInput.Bindings.DefaultMapping.Value;
        return MenuInput.ActionMap.Create(new ActionMapConfig<MenuAction> {
            ControllerId = null,
            InputSettings = InputSettings.Default,
            InputMapping = mapping
        });
    }

    public override void SetInputMappingSource(IObservable<ActionMapConfig<MenuAction>> actionMapConfigChanges) {
        actionMapConfigChanges.Subscribe(actionMapConfig => {
            var newActionMap = MenuInput.ActionMap.Create(actionMapConfig);
            var actionMapRef = GetOrCreate();
            actionMapRef.V = newActionMap;
        });
    }
}
