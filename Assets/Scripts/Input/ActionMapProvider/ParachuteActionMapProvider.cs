using System;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class ParachuteActionMapProvider : MonoBehaviour, IReadonlyRef<IParachuteActionMap> {
        private Ref<IParachuteActionMap> _actionMap;

        private bool _isOverride;

        void Awake() {
            GetOrCreate();
        }
    
        public IParachuteActionMap V {
            get { return GetOrCreate().V; }
        }

        private Ref<IParachuteActionMap> GetOrCreate() {
            if (_actionMap == null) {
                _actionMap = new Ref<IParachuteActionMap>(CreateDefault());
            }
            return _actionMap;
        }

        private IParachuteActionMap CreateDefault() {
            // TODO Merge multiple mappings into one so we can have
            // keyboard and controllers active at the same time.
            return ParachuteControls.Create(
                new ActionMapConfig<ParachuteAction> {
                    ControllerId = null,
                    InputSettings = InputSettings.Default,
                    InputMapping = ParachuteControls.DefaultMapping.Value
                });
        }

        public void SetInputMappingSource(IObservable<ActionMapConfig<ParachuteAction>> actionMapConfigChanges) {
            actionMapConfigChanges.Subscribe(actionMapConfig => {
                if (!_isOverride) {
                    var newActionMap = ParachuteControls.Create(actionMapConfig);
                    var actionMapRef = GetOrCreate();
                    actionMapRef.V = newActionMap;
                }
            });
        }

        public void OverrideActionMap(IParachuteActionMap pilotActionMap) {
            _actionMap.V = pilotActionMap;
            _isOverride = true;
        }
    }
}