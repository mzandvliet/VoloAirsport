using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using InControl;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.UI {

    public class InputMappingsViewModel : MonoBehaviour {
        [Dependency, SerializeField] private ActiveLanguage _activeLanguage;
        [Dependency, SerializeField] private JoystickActivator _joystickActivator;
        [Dependency] private InputBindings<WingsuitAction> _pilotInputBindings;
        [Dependency] private InputBindings<MenuAction> _menuInputBindings; 
        [Dependency] private InputBindings<SpectatorAction> _spectatorInputBindings;
        [Dependency] private InputBindings<ParachuteAction> _parachuteInputBindings;

        private IObservable<InputBindingViewModel[]> _inputMappings;

        private bool _isInitialized;

        void OnEnable() {
            if (!_isInitialized) {
                var inputMappings = _pilotInputBindings.InputMappingChanges
                    .CombineLatest(
                        _menuInputBindings.InputMappingChanges,
                        _parachuteInputBindings.InputMappingChanges,
                        _spectatorInputBindings.InputMappingChanges,
                        _activeLanguage.TableUpdates,
                        _joystickActivator.ActiveController,
                        (pilotConfig, menuConfig, parachuteConfig, spectatorConfig, languageTable, activeControllerId) => {
                            var inputDeviceProfile = activeControllerId.HasValue
                                ? activeControllerId.Value.DeviceProfile
                                : Maybe.Nothing<UnityInputDeviceProfile>();

                            var pilotBindings = PilotInput.Bindings.ToBindings(languageTable, pilotConfig.InputMapping,
                                inputDeviceProfile);
                            var menuBindings = MenuInput.Bindings.ToBindings(languageTable, menuConfig.InputMapping,
                                inputDeviceProfile);
                            var parachuteBindings = ParachuteControls.ToBindings(languageTable, parachuteConfig.InputMapping,
                                inputDeviceProfile);
                            var spectatorBindings = SpectatorInput.Bindings.ToBindings(languageTable,
                                spectatorConfig.InputMapping, inputDeviceProfile);
                            return
                                parachuteBindings.Concat(pilotBindings)
                                    .Concat(menuBindings)
                                    .Concat(spectatorBindings)
                                    .ToArray();
                        }).Replay(1);
                inputMappings.Connect();
                _inputMappings = inputMappings;

                _isInitialized = true;
            }
        }

        public IObservable<InputBindingViewModel[]> InputMappings {
            get { return _inputMappings; }
        }
    }
}
