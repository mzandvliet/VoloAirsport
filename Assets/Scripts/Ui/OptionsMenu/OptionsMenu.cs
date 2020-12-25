using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using InControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RamjetAnvil;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Gui;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.States;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.UI;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fmod = FMODUnity.RuntimeManager;
using VersionInfo = RamjetAnvil.Volo.VersionInfo;

/* Review 08-04-2014 This model has an explicit reference to its view. But I realize moving the
 * glue somewhere else may not matter much. 
 * 
 * Still, this class does three things:
 * 1. holding config models
 * 2. binding events to COUI
 * 3. Wrapping input in event streams
 */


public class OptionsMenu : MonoBehaviour {

    [Dependency, SerializeField] private UnityCoroutineScheduler _coroutineScheduler;
    [Dependency, SerializeField] private InputBinder _inputRebinder;
    [Dependency, SerializeField] private ActiveLanguage _activeLanguage;
    [Dependency, SerializeField] private MenuActionMapProvider _menuActionMap;
    [Dependency, SerializeField] private BaseInputModule _inputModule;
    [Dependency, SerializeField] private InputMappingsViewModel _inputMappingsViewModel;
    [Dependency, SerializeField] private JoystickActivator _joystickActivator;
    [Dependency] private InputBindings<WingsuitAction> _pilotInputBindings;
    [Dependency] private InputBindings<MenuAction> _menuInputBindings; 
    [Dependency] private InputBindings<SpectatorAction> _spectatorInputBindings;
    [Dependency] private InputBindings<ParachuteAction> _parachuteInputBindings;
    [Dependency, SerializeField] private GameSettingsProvider _gameSettingsProvider;
    [SerializeField] private List<AbstractMenu> _availableMenus;
    [SerializeField] private GameObject _renderer;

    [SerializeField] private VersionInfo _versionInfo;
    [SerializeField] private Text _versionNumber;
    [SerializeField] private Text _connectedController;

    private IDictionary<Menu, AbstractMenu> _menus; 
    private Action<MenuActionId, Action> _menuCloseHandler;

    private OptionsMenuModel _model;

    void Update() {
        if (_renderer.activeInHierarchy && _menuActionMap.ActionMap.V.PollButtonEvent(MenuAction.Back) == ButtonEvent.Down) {
            _model.PopMenu();
        }
    }

    public void Initialize() {
        _model = new OptionsMenuModel(_gameSettingsProvider, 
            _activeLanguage.Languages, 
            _versionInfo.VersionNumber,
            StartRebind,
            RestoreInputToDefaults);
        _activeLanguage.TableUpdates.Subscribe(languageTable => {
            _model.LanguageTable = languageTable;
        });


        _renderer.SetActive(false);

        _menus = new Dictionary<Menu, AbstractMenu>();
        for (int i = 0; i < _availableMenus.Count; i++) {
            var menu = _availableMenus[i];
            _menus[menu.Id] = menu;
            menu.Initialize(_model);
        }
        _model.MenuLoaded += menuId => {
            var menu = _menus[menuId];
            var firstSelected = menu.GetComponentsInChildren<Selectable>()
                .Find(s => s.isActiveAndEnabled && s.navigation.mode != Navigation.Mode.None);
            if (firstSelected.IsJust) {
                EventSystem.current.firstSelectedGameObject = firstSelected.Value.gameObject;
                EventSystem.current.SetSelectedGameObject(firstSelected.Value.gameObject);
            }
        };
        _model.Updated += model => {
            for (int i = 0; i < _availableMenus.Count; i++) {
                var menu = _availableMenus[i];
                menu.gameObject.SetActive(model.ActiveMenu == menu.Id);
            }

            var activeMenu = _menus[model.ActiveMenu];
            activeMenu.SetState(model, model.LanguageTable.AsFunc);
            _versionNumber.text = model.AppVersion;
        };
        _model.OnMenuClosed += action => {
            if (_menuCloseHandler != null) {
                _menuCloseHandler(action, null);
            }
        };

        _inputMappingsViewModel.InputMappings.Subscribe(inputBindings => {
            _model.InputBindings = inputBindings;
        });

        _joystickActivator.ActiveController.Subscribe(controller => {
            if (controller.HasValue) {
                _connectedController.text = "Active device: <i>" + controller.Value.Name + "</i>";
            } else {
                _connectedController.text = "Active device: <i>keyboard and mouse</i>";
            }
        });
    }

    public void Open(MenuId id) {
        _coroutineScheduler.Run(OpenInternal(id, (a,b) => { }));
    }

    public void Open(MenuId id, Action<MenuActionId, Action> menuCloseHandler) {
        _coroutineScheduler.Run(OpenInternal(id, menuCloseHandler));
    }

    /// <summary>
    /// Open the menu in the next frame to prevent input from the game to be accidentally sent to the menu.
    /// </summary>
    private IEnumerator<WaitCommand> OpenInternal(MenuId id, Action<MenuActionId, Action> menuCloseHandler) {
        _menuCloseHandler = (action, onComplete) => {
            menuCloseHandler(action, onComplete);
            _renderer.SetActive(false);
        };

        yield return WaitCommand.WaitForNextFrame;

        _renderer.SetActive(true);
        _model.MenuState = id;
        _model.OpenMenu();
        _inputModule.ActivateModule();
    }

    private void StartRebind(InputBindingViewModel binding) {
        _coroutineScheduler.Run(StartRebindInternal(binding));
    }
    
    private IEnumerator<WaitCommand> StartRebindInternal(InputBindingViewModel binding) {
        var bindingId = binding.Id;
        // TODO Update selected binding in the menu model
        Fmod.PlayOneShot("event:/ui/forward");
        // Disable menu input to view
        //_mouseInputToView.Disable();
        _inputModule.DeactivateModule();

        yield return WaitCommand.WaitForNextFrame;

        _model.Rebinding = binding;

        if (bindingId.Group == InputBindingGroup.Menu) {
            MenuAction menuAction = (MenuAction) bindingId.ActionId;
            _inputRebinder.StartRebind(inputSource => {
                if (inputSource.IsJust) {
                    _menuInputBindings.UpdateMapping(menuAction, inputSource.Value);
                }
                _coroutineScheduler.Run(CompleteRebind(isCanceled: inputSource.IsNothing));
            });
        } else if (bindingId.Group == InputBindingGroup.Wingsuit) {
            WingsuitAction pilotAction = (WingsuitAction) bindingId.ActionId;
            _inputRebinder.StartRebind(inputSource => {
                if (inputSource.IsJust) {
                    _pilotInputBindings.UpdateMapping(pilotAction, inputSource.Value);
                }
                _coroutineScheduler.Run(CompleteRebind(isCanceled: inputSource.IsNothing));
            });
        } else if (bindingId.Group == InputBindingGroup.Spectator) {
            SpectatorAction spectatorAction = (SpectatorAction) bindingId.ActionId;
            _inputRebinder.StartRebind(inputSource => {
                if (inputSource.IsJust) {
                    _spectatorInputBindings.UpdateMapping(spectatorAction, inputSource.Value);
                }
                _coroutineScheduler.Run(CompleteRebind(isCanceled: inputSource.IsNothing));
            });
        } else if (bindingId.Group == InputBindingGroup.Parachute) {
            ParachuteAction parachuteAction = (ParachuteAction) bindingId.ActionId;
            _inputRebinder.StartRebind(inputSource => {
                if (inputSource.IsJust) {
                    _parachuteInputBindings.UpdateMapping(parachuteAction, inputSource.Value);
                }
                _coroutineScheduler.Run(CompleteRebind(isCanceled: inputSource.IsNothing));
            });
        }
    }

    private IEnumerator<WaitCommand> CompleteRebind(bool isCanceled) {
        if (isCanceled) {
            Fmod.PlayOneShot("event:/ui/back");
        } else {
            Fmod.PlayOneShot("event:/ui/forward");
        }

        yield return WaitCommand.WaitForNextFrame;

        _inputModule.ActivateModule();
        _model.Rebinding = null;
    }

    private void RestoreInputToDefaults(InputDefaults defaultsType) {
        _menuInputBindings.LoadDefaultActionMap(defaultsType);
        _pilotInputBindings.LoadDefaultActionMap(defaultsType);
        _spectatorInputBindings.LoadDefaultActionMap(defaultsType);
        _parachuteInputBindings.LoadDefaultActionMap(defaultsType);
    }

    public void Close(Action onComplete = null) {
        _menuCloseHandler(MenuActionId.Resume, onComplete);
    }

    public InputBindings<WingsuitAction> PilotInputBindings {
        set { _pilotInputBindings = value; }
    }

    public InputBindings<MenuAction> MenuInputBindings {
        set { _menuInputBindings = value; }
    }

    public InputBindings<SpectatorAction> SpectatorInputBindings {
        set { _spectatorInputBindings = value; }
    }

    public InputBindings<ParachuteAction> ParachuteInputBindings {
        set { _parachuteInputBindings = value; }
    }
}
