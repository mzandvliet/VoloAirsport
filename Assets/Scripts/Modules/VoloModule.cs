using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using RamjetAnvil.DependencyInjection.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Unity.Vr;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using System.Collections.Generic;
using RamjetAnvil.Cameras;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Util;
using RxUnity.Schedulers;
using UnityEngine.SceneManagement;
using UnityEngine.VR;
using Valve.VR;
using Object = UnityEngine.Object;

///
///  Todo: Dependency injection is a mess!
///
/// Differentiate between core, level-specific systems
/// Automate injection for object from dynamically loaded scenes
/// Some dependencies are actually better handled manually, such as those to and from highly volatile objects

public class VoloModule : IModule {

    public IEnumerator<WaitCommand> Load() {
        /* ======== Load Core ======= */

        {
            // Initialize the unity thread dispatcher on the unity thread
            var unityThreadDispatcher = UnityThreadDispatcher.Instance;    
        }

        yield return SceneManager.LoadSceneAsync("Core", LoadSceneMode.Additive).WaitUntilDone();

        var applicationLifeCycleEvents = new GameObject("ApplicationLifeCycleEvents")
            .AddComponent<ComponentLifecycleEvents>();

        ILock applicationLock = GameObject.Find("ApplicationQuitter").GetComponent<ApplicationQuitter>();

        // TODO This needs to move to somewhere inside a scene where we can turn it on/off
        var gameSettingsProvider = Object.FindObjectOfType<GameSettingsProvider>();
        gameSettingsProvider.UpdateGameSettings(GameSettings.ReadSettingsFromDisk());
        var userConfigurableSystems = Object.FindObjectOfType<UserConfigurableSystems>();
        var gameSettingsApplier = new GameSettingsApplier(userConfigurableSystems);
        gameSettingsProvider.SettingChanges
            .Subscribe(settings => gameSettingsApplier.ApplySettings(settings, gameSettingsProvider.ActiveVrMode));
        gameSettingsProvider.SettingChanges
            .Select(gameSettings => gameSettings.Audio)
            .CombineLatest(FMODUtil.StudioSystem(), (audioSettings, fmodSystem) => audioSettings)
            .Subscribe(audioSettings => {
                gameSettingsApplier.ApplyAudioSettings(audioSettings);
            });

        // TODO This needs to move to somewhere inside a scene where we can turn it on/off
        gameSettingsProvider.SettingChanges
            .Skip(1)
            .Throttle(TimeSpan.FromSeconds(2), Scheduler.ThreadPool)
            .Select(gameSettings => new SerializeTask<GameSettings> {FilePath = GameSettings.UserSettingsConfigPath.Value, SerializableValue = gameSettings})
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(applicationLock.RunWithLock<SerializeTask<GameSettings>>(settingsConfig => {
                var gameSettings = settingsConfig.SerializableValue;
                gameSettings.Serialize2Disk(settingsConfig.FilePath);
            }));

        var inputSettings = gameSettingsProvider.SettingChanges
            .Select(settings => InputSettings.FromGameSettings(settings.Input));
        var menuActionMapProvider = Object.FindObjectOfType<MenuActionMapProvider>();
        var pilotActionMapProvider = Object.FindObjectOfType<PilotActionMapProvider>();
        var parachuteActionMapProvider = Object.FindObjectOfType<ParachuteActionMapProvider>();
        JoystickActivator joystickActivator = Object.FindObjectOfType<JoystickActivator>();
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
        joystickActivator.ActiveController.Subscribe(controllerId => {
            menuInputBindings.UpdateControllerId(controllerId);
            pilotInputBindings.UpdateControllerId(controllerId);
            spectatorInputBindings.UpdateControllerId(controllerId);
            parachuteInputBindings.UpdateControllerId(controllerId);
        });

        applicationLifeCycleEvents.OnDestroyEvent += () => {
            menuInputBindings.Dispose();
            pilotInputBindings.Dispose();
            parachuteInputBindings.Dispose();
            spectatorInputBindings.Dispose();
        };

        menuActionMapProvider.SetInputMappingSource(menuInputBindings.InputMappingChanges);
        parachuteActionMapProvider.SetInputMappingSource(parachuteInputBindings.InputMappingChanges);
        pilotActionMapProvider.SetInputMappingSource(pilotInputBindings.InputMappingChanges);
        // Spectator input
        var spectatorCamera = GameObject.Find("InGameSpectatorCamera")
            .GetComponent<SpectatorCamera>();
        var menuClock = GameObject.Find("_RealtimeClock").GetComponent<AbstractUnityClock>();
        spectatorInputBindings.InputMappingChanges.Subscribe(actionMapConfig => {
            spectatorCamera.ActionMap = SpectatorInput.ActionMap.Create(actionMapConfig, menuClock);
        });

        // TODO Create serializer component and add it to the scene
        menuInputBindings.InputMappingChanges
            .Skip(1)
            .Select(
                actionMapConfig => new SerializeTask<InputSourceMapping<MenuAction>> {
                        FilePath = MenuInput.Bindings.CustomInputMappingFilePath.Value,
                        SerializableValue = actionMapConfig.InputMapping })
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(applicationLock.RunWithLock<SerializeTask<InputSourceMapping<MenuAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));
        pilotInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<WingsuitAction>>{
                FilePath = PilotInput.Bindings.CustomInputMappingFilePath.Value,
                SerializableValue = actionMapConfig.InputMapping})
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(applicationLock.RunWithLock<SerializeTask<InputSourceMapping<WingsuitAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));
        spectatorInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<SpectatorAction>>{
                FilePath = SpectatorInput.Bindings.CustomInputMappingFilePath.Value,
                SerializableValue = actionMapConfig.InputMapping})
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(applicationLock.RunWithLock<SerializeTask<InputSourceMapping<SpectatorAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));
        parachuteInputBindings.InputMappingChanges
            .Skip(1)
            .Select(actionMapConfig => new SerializeTask<InputSourceMapping<ParachuteAction>> {
                FilePath = ParachuteControls.CustomInputMappingFilePath.Value,
                SerializableValue = actionMapConfig.InputMapping })
            .ObserveOn(Schedulers.FileWriterScheduler)
            .Subscribe(applicationLock.RunWithLock<SerializeTask<InputSourceMapping<ParachuteAction>>>(mappingConfig => {
                mappingConfig.SerializableValue.Serialize2Disk(mappingConfig.FilePath);
            }));


        LoadVrConfig(gameSettingsProvider);

        var dependencyResolver = new GameObject("GlobalDependencyResolver").AddComponent<UnityDependencyResolver>();
        dependencyResolver.NonSerializableRefs.Add(new DependencyReference("fileWriterScheduler", Schedulers.FileWriterScheduler));
        dependencyResolver.NonSerializableRefs.Add(new DependencyReference("menuInputBindings", menuInputBindings));
        dependencyResolver.NonSerializableRefs.Add(new DependencyReference("pilotInputBindings", pilotInputBindings));
        dependencyResolver.NonSerializableRefs.Add(new DependencyReference("spectatorInputBindings", spectatorInputBindings));
        dependencyResolver.NonSerializableRefs.Add(new DependencyReference("parachuteInputBindings", parachuteInputBindings));
        dependencyResolver.Resolve();

        // Create and inject the player interface dynamically
        var cameraManager = Object.FindObjectOfType<CameraManager>();
        cameraManager.CreateCameraRig(gameSettingsProvider.ActiveVrMode, dependencyResolver.DependencyContainer);
        var canvasManager = Object.FindObjectOfType<UICanvasManager>();
        canvasManager.Initialize(cameraManager);

        if (gameSettingsProvider.ActiveVrMode == VrMode.Oculus) {
            new GameObject("__OculusGlobalEventEmitter").AddComponent<OculusGlobalEventEmitter>();
        }

        // Create a cursor
        var cursor = CreateCursor(gameSettingsProvider.ActiveVrMode, cameraManager.Rig.GetMainCamera());
        dependencyResolver.NonSerializableRefs.Add(new DependencyReference("cursor", cursor));
        var inputModule = GameObject.FindObjectOfType<CursorInputModule>();
        inputModule.Cursor = cursor;
        inputModule.NavigationDevice = new MenuActionMapCursor(menuActionMapProvider);
        var raycasters = GameObject.FindObjectsOfType<PhysicsRayBasedRaycaster>();
        foreach (var raycaster in raycasters) {
            raycaster.SetCamera(cameraManager.Rig.GetMainCamera());
        }

        var hudPositioner = GameObject.Find("HUDElementPositioner").GetComponent<HudElementPositioner>();
        var hudElements = Object.FindObjectsOfType<HudElement>();
        for (int i = 0; i < hudElements.Length; i++) {
            var hudElement = hudElements[i];
            hudPositioner.AddHudElement(hudElement);
        }

//        var courseManager = Object.FindObjectOfType<CourseManager>();
//        courseManager.CameraTransform = cameraManager.Rig.transform;

        var courseEditor = GameObject.Find("CourseEditor");
        courseEditor.SetActive(false);

        /* ======== Load Swiss Alps ======= */

        dependencyResolver.Resolve();

        yield return SceneManager.LoadSceneAsync("SwissAlps", LoadSceneMode.Additive).WaitUntilDone();
        
        var windManager = Object.FindObjectOfType<WindManager>();
        var windEffectors = Object.FindObjectsOfType<WindEffector>();
        for (int i = 0; i < windEffectors.Length; i++) {
            windEffectors[i].Manager = windManager;
        }

        var evts = Object.FindObjectOfType<AbstractUnityEventSystem>();
        var turrets = Object.FindObjectsOfType<Turret>();
        for (int i = 0; i < turrets.Length; i++) {
            turrets[i].Initialize(evts);
        }

        dependencyResolver.Resolve();

        gameSettingsApplier.ApplySettings(gameSettingsProvider.ActiveSettings, gameSettingsProvider.ActiveVrMode);
        
        var optionsMenu = Object.FindObjectOfType<OptionsMenu>();
        optionsMenu.Initialize();

        var challengeManager = GameObject.FindObjectOfType<ChallengeManager>();
        challengeManager.Initialize();

        //        var networkConfig = GameObject.FindObjectOfType<NetworkConfig>();
        //        if (networkConfig.IsServer) {
        //            var server = GameObject.FindObjectOfType<VoloServer>();
        //            server.Host("Silly host name", networkConfig.ServerPort);
        //        }
        //        else {
        //            var client = GameObject.FindObjectOfType<VoloClient>();
        //            client.Join(new IPEndPoint(IPAddress.Parse("127.0.0.1"), networkConfig.ServerPort));
        //        }

        /* Clean up after ourselves, now's a good time */
        GC.Collect();
    }

    public IEnumerator<WaitCommand> Run() {
        yield return WaitCommand.DontWait;

        var coroutineScheduler = GameObject.FindObjectOfType<UnityCoroutineScheduler>();
        var startupScreen = GameObject.FindObjectOfType<VoloStateMachine>();

        var dependencyResolver = GameObject.Find("_DependencyResolver").GetComponent<UnityDependencyResolver>();

        startupScreen.StartMachine(coroutineScheduler, dependencyResolver.DependencyContainer);
    }

    private static void LoadVrConfig(GameSettingsProvider gameSettingsProvider) {
        var vrMode = DetermineVrMode();

        Debug.Log("Booting in VrMode: " + vrMode);

        // Todo: We don't want to serialize this value, but we do want other systems to use it
        gameSettingsProvider.ActiveVrMode = vrMode;
    }

    public static VrMode DetermineVrMode() {
        var commandLineArgs = Environment.GetCommandLineArgs();
        VrMode? commandlineOverrideVrMode = null;
        for (int i = 0; i < commandLineArgs.Length; i++) {
            var commandLineArg = commandLineArgs[i];
            if (commandLineArg == "-vrmode") {
                commandlineOverrideVrMode = VrModeExtensions.FromCommandlineArgument(commandLineArgs[i + 1]);
            }
        }
        Debug.Log("command-line VR mode: " + (commandlineOverrideVrMode.HasValue ? commandlineOverrideVrMode.Value.ToString() : "'empty'"));
        var vrMode = commandlineOverrideVrMode.HasValue ? commandlineOverrideVrMode.Value : VrMode.None;
        vrMode = ValidateVrMode(vrMode);
        return vrMode;

//        return VrMode.OpenVr;
    }

    private static VrMode ValidateVrMode(VrMode mode) {
        if (mode == VrMode.Oculus && !VRDevice.isPresent) {
            Debug.LogError("Oculus mode selected, but Oculus Rift was not found.");
            mode = VrMode.None;
        }
        else if (mode == VrMode.OpenVr && !OpenVR.IsHmdPresent()) {
            Debug.LogError("OpenVR mode selected, but HTC Vive was not found.");
            mode = VrMode.None;
        }
        return mode;
    }

    private static ICursor CreateCursor(VrMode mode, Camera camera) {
        ICursor cursor;
        switch (mode) {
            case VrMode.None:
                cursor = new MouseCursor(camera);
                break;
            case VrMode.Oculus:
                cursor = NoOpCursor.Default;
                break;
            case VrMode.OpenVr:
                cursor = NoOpCursor.Default;
                break;
            default:
                throw new ArgumentOutOfRangeException("mode", mode, null);
        }
        return cursor;
    }
}
