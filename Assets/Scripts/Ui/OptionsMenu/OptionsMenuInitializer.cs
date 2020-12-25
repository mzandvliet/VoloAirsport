using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Util;
using RxUnity.Schedulers;
using UnityEngine;

public class OptionsMenuInitializer : MonoBehaviour {

    [SerializeField] private GameSettingsProvider _gameSettingsProvider;
    [SerializeField] private UserConfigurableSystems _userConfigurableSystems;
    [SerializeField] private MenuActionMapProvider _menuActionMapProvider;
    [SerializeField] private PilotActionMapProvider _pilotActionMapProvider;
    [SerializeField] private ParachuteActionMapProvider _parachuteActionMapProvider;
    [SerializeField] private JoystickActivator _joystickActivator;
    [SerializeField] private OptionsMenu _optionsMenu;
    [SerializeField] private ApplicationQuitter _quitter;

    void Awake() {
        // Hack to initialize the thread dispatcher in the unity thread
        {
            var threadDispathcer = UnityThreadDispatcher.Instance;    
        }

        var applicationLifeCycleEvents = new GameObject("ApplicationLifeCycleEvents")
            .AddComponent<ComponentLifecycleEvents>();

        _gameSettingsProvider.UpdateGameSettings(GameSettings.ReadSettingsFromDisk());
        var gameSettingsApplier = new GameSettingsApplier(_userConfigurableSystems);
        _gameSettingsProvider.SettingChanges
            .Subscribe(settings => gameSettingsApplier.ApplySettings(settings, _gameSettingsProvider.ActiveVrMode));
        _gameSettingsProvider.SettingChanges
            .Select(gameSettings => gameSettings.Audio)
            .CombineLatest(FMODUtil.StudioSystem(), (audioSettings, fmodSystem) => audioSettings)
            .Subscribe(audioSettings => {
                gameSettingsApplier.ApplyAudioSettings(audioSettings);
            });

        _gameSettingsProvider.SettingChanges
            .Skip(1)
            .Throttle(TimeSpan.FromSeconds(2), Scheduler.ThreadPool)
            .Select(gameSettings => new SerializeTask<GameSettings> {FilePath = GameSettings.UserSettingsConfigPath.Value, SerializableValue = gameSettings})
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(_quitter.RunWithLock<SerializeTask<GameSettings>>(settingsConfig => {
                var gameSettings = settingsConfig.SerializableValue;
                gameSettings.Serialize2Disk(settingsConfig.FilePath);
            }));
        
        var inputSettings = _gameSettingsProvider.SettingChanges
            .Select(settings => InputSettings.FromGameSettings(settings.Input));
        InputBindings<MenuAction> menuInputBindings = new InputBindings<MenuAction>(
            MenuInput.Bindings.InitialMapping(), 
            inputSettings,
            MenuInput.Bindings.DefaultControllerMappings.Value);
        InputBindings<WingsuitAction> pilotInputBindings = new InputBindings<WingsuitAction>(
            PilotInput.Bindings.InitialMapping(),
            inputSettings,
            PilotInput.Bindings.DefaultControllerMappings.Value);
        InputBindings<SpectatorAction> spectatorInputBindings = new InputBindings<SpectatorAction>(
            SpectatorInput.Bindings.InitialMapping(),
            inputSettings,
            SpectatorInput.Bindings.DefaultControllerMappings.Value);
        InputBindings<ParachuteAction> parachuteInputBindings = new InputBindings<ParachuteAction>(
            ParachuteControls.InitialMapping(),
            inputSettings,
            ParachuteControls.DefaultMappings.Value);
        _joystickActivator.ActiveController.Subscribe(controllerId => {
            menuInputBindings.UpdateControllerId(controllerId);
            pilotInputBindings.UpdateControllerId(controllerId);
            spectatorInputBindings.UpdateControllerId(controllerId);
            parachuteInputBindings.UpdateControllerId(controllerId);
        });

        // TODO Inject Inputbindings into options menu

        applicationLifeCycleEvents.OnDestroyEvent += () => {
            menuInputBindings.Dispose();
            parachuteInputBindings.Dispose();
            pilotInputBindings.Dispose();
            spectatorInputBindings.Dispose();
        };
        
        _menuActionMapProvider.SetInputMappingSource(menuInputBindings.InputMappingChanges);
        _pilotActionMapProvider.SetInputMappingSource(pilotInputBindings.InputMappingChanges);
        _parachuteActionMapProvider.SetInputMappingSource(parachuteInputBindings.InputMappingChanges);

        menuInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<MenuAction>> {
                FilePath = MenuInput.Bindings.CustomInputMappingFilePath.Value,
                SerializableValue = actionMapConfig.InputMapping })
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(_quitter.RunWithLock<SerializeTask<InputSourceMapping<MenuAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));
        pilotInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<WingsuitAction>>{ 
                FilePath = PilotInput.Bindings.CustomInputMappingFilePath.Value, 
                SerializableValue = actionMapConfig.InputMapping})
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(_quitter.RunWithLock<SerializeTask<InputSourceMapping<WingsuitAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));
        spectatorInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<SpectatorAction>>{ 
                FilePath = SpectatorInput.Bindings.CustomInputMappingFilePath.Value, 
                SerializableValue = actionMapConfig.InputMapping})
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(_quitter.RunWithLock<SerializeTask<InputSourceMapping<SpectatorAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));
        parachuteInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<ParachuteAction>> {
                FilePath = ParachuteControls.CustomInputMappingFilePath.Value,
                SerializableValue = actionMapConfig.InputMapping })
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(_quitter.RunWithLock<SerializeTask<InputSourceMapping<ParachuteAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));

        _optionsMenu.MenuInputBindings = menuInputBindings;
        _optionsMenu.ParachuteInputBindings = parachuteInputBindings;
        _optionsMenu.PilotInputBindings = pilotInputBindings;
        _optionsMenu.SpectatorInputBindings = spectatorInputBindings;
    }
}
