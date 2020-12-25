using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public class InputBindings<TAction> : IDisposable {

        private readonly IImmutableDictionary<InputDefaults, InputSourceMapping<TAction>> _defaultMappings; 
        private InputSourceMapping<TAction> _currentMapping;

        private readonly ISubject<InputSourceMapping<TAction>> _inputMappings;
        private ControllerId _activeControllerId;
        private readonly ISubject<JoystickActivator.Controller?> _activeControllerUpdates;

        private readonly IObservable<ActionMapConfig<TAction>> _actionMap;
        private bool _usesDefaultMapping;

        public InputBindings(
            InputSourceMapping<TAction> initialMapping,
            IObservable<InputSettings> inputSettingChanges,
            IImmutableDictionary<InputDefaults, InputSourceMapping<TAction>> defaultMappings) {

            _defaultMappings = defaultMappings;
            _usesDefaultMapping = false;
            foreach (var defaultMapping in defaultMappings.Values) {
                if (initialMapping.Equals(defaultMapping)) {
                    _usesDefaultMapping = true;
                }
            }

            _inputMappings = new BehaviorSubject<InputSourceMapping<TAction>>(initialMapping);
            _inputMappings.Subscribe(mapping => {
                _currentMapping = mapping;
            });
            _activeControllerId = null;
            _activeControllerUpdates = new BehaviorSubject<JoystickActivator.Controller?>(null);

            _actionMap = _inputMappings.CombineLatest(
                _activeControllerUpdates,
                inputSettingChanges.DistinctUntilChanged(EqualityComparer<InputSettings>.Default),
                (mapping, controller, settings) => {
                    var controllerId = controller.HasValue ? controller.Value.Id : null;
                    return new ActionMapConfig<TAction> {
                        InputMapping = mapping,
                        ControllerId = controllerId,
                        InputSettings = settings
                    };
                });
        }

        public void UpdateControllerId(JoystickActivator.Controller? controller) {
            InputDefaults? inputDefaults;
            if (controller.HasValue) {
                inputDefaults = controller.Value.ControllerType.ToInputDefaults();
            } else {
                inputDefaults = InputDefaults.KeyboardAndMouse;
            }

            SetController(controller);
            if (inputDefaults.HasValue && _usesDefaultMapping) {
                LoadDefaultActionMap(inputDefaults.Value);
            }
        }

        public void UpdateMapping(TAction action, InputSource inputSource) {
            var mappings = _currentMapping
                .Mappings
                .SetItem(action, ImmutableList.Create<InputSource>(inputSource));
            _usesDefaultMapping = false;
            var updatedMapping = new InputSourceMapping<TAction>(mappings, formatVersion: InputSourceMapping<TAction>.CurrentFormatVersion);
            _inputMappings.OnNext(updatedMapping);
        }

        private void SetController(JoystickActivator.Controller? controller) {
            _activeControllerId = controller.HasValue ? controller.Value.Id : null;
            _activeControllerUpdates.OnNext(controller);
        }

        public void LoadDefaultActionMap(InputDefaults inputDefaults) {
            inputDefaults = inputDefaults.VerifyController(_activeControllerId);

            InputSourceMapping<TAction> defaultMapping;
            if (_defaultMappings.TryGetValue(inputDefaults, out defaultMapping)) {
                //Debug.Log("loading defaults for " + inputDefaults);
                _usesDefaultMapping = true;
                _inputMappings.OnNext(defaultMapping);
            } else {
                throw new Exception("Could not find default mapping for type " 
                    + inputDefaults + " of action map " + typeof(TAction));
            }
        }

        public InputSourceMapping<TAction> CurrentInputMapping {
            get { return _currentMapping; }
        }

        public IObservable<ActionMapConfig<TAction>> InputMappingChanges {
            get { return _actionMap; }
        }

        public void Dispose() {
            _inputMappings.OnCompleted();
            _activeControllerUpdates.OnCompleted();
        }
    }
}
