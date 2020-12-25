using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.RamNet;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Networking;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.Util;
using UnityEngine;

namespace RamjetAnvil.Volo.States {

    public class Playing : State {

        [Serializable]
        public class Data {
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            [SerializeField] public UnityCoroutineScheduler CoroutineScheduler;
            [SerializeField] public AbstractUnityClock MenuClock;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public AbstractUnityClock FixedClock;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public PlayerPilotSpawner PlayerPilotSpawner;
            [SerializeField] public ParachuteSpawner ParachuteSpawner;
            [SerializeField] public MenuActionMapProvider MenuActionMapProvider;
            [SerializeField] public PilotActionMapProvider PilotActionMapProvider;
            [SerializeField] public JoystickActivator JoystickActivator;
            [SerializeField] public ThirdPersonCameraController ThirdPersonCameraController;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public ChallengeAnnouncerUi ChallengeAnnouncerUi;

            [SerializeField] public ActiveNetwork ActiveNetwork;

            [SerializeField] public FaderSettings FaderSettings;
        }

        [StateEvent("Update")]
        public event Action InternalStateMachineTick;

        private readonly Data _data;

        private readonly IStateMachine _playingStateMachine;
        private readonly ITypedDataCursor<ParachuteConfig> _activeParachuteConfig;
        private bool _isTransitioning;

        public Playing(IStateMachine machine, Data data, FlyWingsuit.Data wingsuitData, ParachuteStates.Data parachuteData, 
            SpectatorMode.Data spectatorData) : base(machine) {
            _data = data;
            _data.PlayerPilotSpawner.ActiveNetwork = data.ActiveNetwork;
            wingsuitData.ActiveNetwork = data.ActiveNetwork;
            wingsuitData.PlayerPilotSpawner.ActiveNetwork = data.ActiveNetwork;

            var parachuteConfigPath = Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "ParachuteConfig_v" + ParachuteConfig.VersionNumber + ".json");
            Debug.Log("parachute config path: " + parachuteConfigPath);

            var defaultParachuteStorage = new ParachuteStorage(ParachuteStorage.DefaultChutesDir.Value,
                parachuteData.InitialConfig, parachuteData.HardCodedAirfoilDefinition, isEditable: false);
            var parachuteStorage = new ParachuteStorage(ParachuteStorage.StorageDir.Value, 
                parachuteData.InitialConfig, parachuteData.HardCodedAirfoilDefinition,
                isEditable: true);
            var allParachutes = defaultParachuteStorage.StoredChutes.Concat(parachuteStorage.StoredChutes).ToList();
            var initialChute = ParachuteStorage.SelectParachute(allParachutes,
                _data.GameSettingsProvider.ActiveSettings.Other.SelectedParachuteId);
            var storageState = TypedDataCursor<ParachuteStorageViewState>.Root(new ParachuteStorageViewState(initialChute, allParachutes,
                ParachuteStorage.StorageDir.Value));
            _activeParachuteConfig = storageState.To(s => s.EditorState).To(s => s.Config);
            _activeParachuteConfig.OnUpdate.Subscribe(selectedParachute => {
                var gameSettings = _data.GameSettingsProvider.ActiveSettings;
                if (gameSettings.Other.SelectedParachuteId != selectedParachute.Id) {
                    gameSettings.Other.SelectedParachuteId = selectedParachute.Id;
                    _data.GameSettingsProvider.UpdateGameSettings(gameSettings);    
                }
            });

            _playingStateMachine = BuildPlayingStateMachine(data.CoroutineScheduler, wingsuitData, parachuteData, parachuteStorage, storageState, spectatorData);
            _playingStateMachine.Transition(PlayingStates.Initial);
        }

        IEnumerator<WaitCommand> OnEnter(SpawnpointLocation spawnpoint) {
            _data.ChallengeAnnouncerUi.enabled = true;
            _data.JoystickActivator.enabled = false;
            _data.ThirdPersonCameraController.enabled = true;

            // TODO Don't spawn the player this is now the responsisiblity of the wingsuit state
            //yield return _data.PlayerPilotSpawner.Respawn(spawnpoint).AsWaitCommand();

            var environment = new PlayingEnvironment(
                spawnpoint: spawnpoint,
                pilot: null,
                parachuteConfig: _activeParachuteConfig.Get());
            _playingStateMachine.Transition(PlayingStates.FlyingWingsuit, environment, Maybe.Just(new RespawnRequest()));
            yield return WaitCommand.WaitRoutine(CameraTransitions.FadeIn(_data.CameraManager.Rig.ScreenFader, _data.MenuClock,  _data.FaderSettings));
        }

        IEnumerator<WaitCommand> OnResume(Maybe<RespawnRequest> respawnRequest, bool transitToParachuteEditor) {
            yield return _playingStateMachine.TransitionToParent(respawnRequest, transitToParachuteEditor).WaitUntilDone();

            _data.ChallengeAnnouncerUi.enabled = true;
            _data.JoystickActivator.enabled = false;
            _data.ThirdPersonCameraController.enabled = true;
        }

        void Update() {
            var menuActionMap = _data.MenuActionMapProvider.ActionMap.V;

            // TODO Only update the flying state if we're actually flying

            if (!_playingStateMachine.IsTransitioning && !_isTransitioning) {
                if (menuActionMap.PollButtonEvent(MenuAction.Pause) == ButtonEvent.Down) {
                    _data.CoroutineScheduler.Run(ToOptionsMenu());
                }
            }

            if (InternalStateMachineTick != null) {
                InternalStateMachineTick();
            }
        }

        IEnumerator<WaitCommand> ToOptionsMenu() {
            _isTransitioning = true;
            yield return _playingStateMachine.Transition(PlayingStates.Suspended).WaitUntilDone();
            Machine.Transition(VoloStateMachine.States.OptionsMenu, MenuId.Playing);
            _isTransitioning = false;
        }
        
        void OnSuspend() {
            _data.ChallengeAnnouncerUi.enabled = false;
            _data.JoystickActivator.enabled = true;
            _data.ThirdPersonCameraController.enabled = false;
        }

        IEnumerator<WaitCommand> OnExit() {
            yield return WaitCommand.WaitRoutine(CameraTransitions.FadeOut(_data.CameraManager.Rig.ScreenFader, _data.MenuClock, _data.FaderSettings));

            _playingStateMachine.Transition(PlayingStates.Initial);

            yield return _data.PlayerPilotSpawner.Despawn().AsWaitCommand();

            _data.ChallengeAnnouncerUi.enabled = false;
            _data.JoystickActivator.enabled = true;
            _data.ThirdPersonCameraController.enabled = false;
        }

        public static class PlayingStates {
            public static readonly StateId Initial = new StateId("Initial");
            public static readonly StateId FlyingWingsuit = new StateId("FlyingWingsuit");
            public static readonly StateId FlyingParachute = new StateId("FlyingParachute");
            public static readonly StateId EditingParachute = new StateId("EditingParachute");
            public static readonly StateId Suspended = new StateId("Suspended");
            public static readonly StateId Spectating = new StateId("Spectating");
        }

        private IStateMachine BuildPlayingStateMachine(
            ICoroutineScheduler scheduler,
            FlyWingsuit.Data wingsuitData,
            ParachuteStates.Data parachuteData,
            ParachuteStorage parachuteStorage,
            ITypedDataCursor<ParachuteStorageViewState> parachuteStorageViewState,
            SpectatorMode.Data spectatorData) {

            var machine = new StateMachine<Playing>(this, scheduler);

            machine.AddState(PlayingStates.Initial, new ParachuteStates.InitialState(machine))
                .Permit(PlayingStates.FlyingWingsuit)
                .PermitChild(PlayingStates.Suspended);
            machine.AddState(PlayingStates.FlyingWingsuit, new FlyWingsuit(machine, wingsuitData))
                .Permit(PlayingStates.EditingParachute)
                .Permit(PlayingStates.FlyingParachute)
                .PermitChild(PlayingStates.Suspended)
                .PermitChild(PlayingStates.Spectating)
                .Permit(PlayingStates.Initial);
            machine.AddState(PlayingStates.FlyingParachute, new ParachuteStates.Flying(machine, parachuteData))
                .Permit(PlayingStates.EditingParachute)
                .Permit(PlayingStates.FlyingWingsuit)
                .PermitChild(PlayingStates.Spectating)
                .PermitChild(PlayingStates.Suspended)
                .Permit(PlayingStates.Initial);
            machine.AddState(PlayingStates.Spectating, new SpectatorMode(machine, spectatorData))
                .PermitChild(PlayingStates.Suspended);
            machine.AddState(PlayingStates.EditingParachute, new ParachuteStates.Editing(machine, parachuteData, parachuteStorage, parachuteStorageViewState))
                .Permit(PlayingStates.FlyingParachute)
                .PermitChild(PlayingStates.Suspended)
                .Permit(PlayingStates.Initial);
            machine.AddState(PlayingStates.Suspended, new ParachuteStates.InitialState(machine))
                .Permit(PlayingStates.Initial);

            return machine;
        }

    }
}
