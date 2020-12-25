using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.InputModule;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Util;
using RxUnity.Schedulers;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.States {

    public static class ParachuteStates {

        [Serializable]
        public class Data {
            [SerializeField] public UnityCoroutineScheduler CoroutineScheduler;
            [SerializeField] public FixedUnityCoroutineScheduler FixedCoroutineScheduler;
            
            [SerializeField] public MenuActionMapProvider ActionMapProvider;
            [SerializeField] public ParachuteActionMapProvider ParachuteActionMap;
            [SerializeField] public PilotActionMapProvider PilotActionMapProvider;
            [SerializeField] public ParachuteController ParachuteController;
            [SerializeField] public ApplicationQuitter Quitter;
            [SerializeField] public ParachuteConfig InitialConfig;
            [SerializeField] public AirfoilDefinition HardCodedAirfoilDefinition;
            [SerializeField] public ParachuteSpawner EditorSpawner;
            [SerializeField] public ParachuteSpawner ImmersionSpawner;
            [SerializeField] public ParachuteEditor ParachuteEditor;
            [SerializeField] public ActiveLanguage ActiveLanguage;
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            [SerializeField] public SoundMixer SoundMixer;
            [SerializeField] public InactiveCursorHider CursorHider;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public RamjetInputModule InputModule;

            [SerializeField] public GameHud GameHud;

            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public FaderSettings FaderSettings;
            [SerializeField] public AbstractUnityClock MenuClock;
            [SerializeField] public AbstractUnityClock FixedClock;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public ParachuteEditorCamera EditorCamera;
            
            [SerializeField] public ParachuteSelectionView ParachuteSelectionView;

            [SerializeField] public ThirdPersonCameraController ThirdPersonCameraMount;

            [SerializeField] public float ParachuteDeploymentDuration;
            [SerializeField] public float MountSwitchDuration;
        }

        public class InitialState : State {
            public InitialState(IStateMachine machine) : base(machine) {}
        }
        
        public class Flying : State {

            private readonly Data _data;
            private PlayingEnvironment _environment;
            private Parachute _parachute;
            private bool _isRespawnRequested;
            private bool _transitToEditor;
            
            public Flying(IStateMachine machine, Data data) : base(machine) {
                _data = data;
                _data.ParachuteEditor.gameObject.SetActive(false);
                _data.ThirdPersonCameraMount.PlayerActionMap = _data.PilotActionMapProvider.ActionMapRef;

                _data.ParachuteController.ActionMap = _data.ParachuteActionMap;
            }
            
            private IEnumerator<WaitCommand> OnEnter(PlayingEnvironment environment) {
                _environment = environment;

                _parachute = _data.ImmersionSpawner.Create(environment.ParachuteConfig, "ImmersionParachute");
                _data.ParachuteController.Parachute = _parachute;

                _parachute.AttachToPilot(environment.Pilot, Parachute.InflightUnfoldOrientation, _data.GameSettingsProvider);
                // TODO This is a hack to compensate for the editor 
                //      that misuses the pilot for its own purposes.
                //      We actually need a second pilot for editor purposes

                environment.Pilot.DisableWings();
                environment.Pilot.SetPhysical();
                environment.Pilot.PilotAnimator.SetState(PilotAnimatorState.Parachute);

                SwitchToMount(environment.SelectedCameraMount);
                _data.GameHud.SetTarget(_parachute.Pilot.FlightStatistics, AvatarType.Parachute);
                _data.GameHud.Activate();

                _parachute.Deploy();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (_data.CameraManager.Rig.Shake) {
                    _data.CameraManager.Rig.Shake.enabled = true;
                }

                _data.GameClock.Resume();
                _data.FixedClock.Resume();

                _data.SoundMixer.Unpause(SoundLayer.GameEffects);

                // Indicate that we have unfolded the parachute once, i.e. the player know how to do it
                if (_data.GameSettingsProvider.ActiveSettings.Tutorial.HasUnfoldedParachute != true) {
                    var gameSettings = _data.GameSettingsProvider.ActiveSettings;
                    gameSettings.Tutorial.HasUnfoldedParachute = true;
                    _data.GameSettingsProvider.UpdateGameSettings(gameSettings);
                }

                yield return WaitCommand.DontWait;
            }

            private IEnumerator<WaitCommand> OnResume(Maybe<RespawnRequest> respawnRequest, bool transitToEditor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                _transitToEditor = transitToEditor;

                SwitchToMount(_environment.SelectedCameraMount);
                _data.GameHud.SetTarget(_parachute.Pilot.FlightStatistics, AvatarType.Parachute);

                _data.GameClock.Resume();
                _data.FixedClock.Resume();

                _isRespawnRequested = respawnRequest.IsJust;

                _data.GameHud.Activate();

                if (_data.CameraManager.Rig.Shake) {
                    _data.CameraManager.Rig.Shake.enabled = true;
                }

                _data.SoundMixer.Unpause(SoundLayer.GameEffects);

                yield return WaitCommand.DontWait;
            }

            private void Update() {
                var parachuteActionMap = _data.ParachuteActionMap.V;
                var pilotActionMap = _data.PilotActionMapProvider.ActionMap;
                var shouldRespawn = pilotActionMap.PollButtonEvent(WingsuitAction.Respawn) == ButtonEvent.Down;
                var shouldSwitchVehicleType = pilotActionMap.PollButtonEvent(WingsuitAction.UnfoldParachute) == ButtonEvent.Down;

                _data.GameHud.SetParachuteInput(_data.ParachuteController.Input);

                if (shouldRespawn || _isRespawnRequested) {
                    _isRespawnRequested = false;
                    Machine.Transition(Playing.PlayingStates.FlyingWingsuit, _environment, Maybe.Just(new RespawnRequest()));
                } else {
                    if (_transitToEditor) {
                        _transitToEditor = false;
                        Machine.Transition(Playing.PlayingStates.EditingParachute, _environment);
                    } else if (parachuteActionMap.ParachuteConfigToggle == ButtonEvent.Down) {
                        TransitToEditingState();
                    } else if (shouldSwitchVehicleType) {
                        Machine.Transition(Playing.PlayingStates.FlyingWingsuit, _environment, Maybe.Nothing<RespawnRequest>());
                    } else if (pilotActionMap.PollButtonEvent(WingsuitAction.ChangeCamera) == ButtonEvent.Down) {
                        _environment = _environment.NextMount();
                        SwitchToMount(_environment.SelectedCameraMount);
                    } else if (pilotActionMap.PollButtonEvent(WingsuitAction.ToggleSpectatorView) == ButtonEvent.Down) {
                        Machine.Transition(Playing.PlayingStates.Spectating);
                    }
                }
            }

            private void SwitchToMount(PilotCameraMountId cameraMountId) {
                var parachuteFlightStats = _parachute.Pilot.FlightStatistics;
                if (cameraMountId == PilotCameraMountId.Orbit) {
                    _data.ThirdPersonCameraMount.SetParachuteTarget(parachuteFlightStats, _parachute);
                    _data.CameraManager.SwitchMount(_data.ThirdPersonCameraMount);    
                } else if (cameraMountId == PilotCameraMountId.FirstPerson) {
                    _data.ThirdPersonCameraMount.RemoveTarget();
                    _data.CameraManager.SwitchMount(_parachute.Pilot.HeadCameraMount);
                } else {
                    throw new ArgumentOutOfRangeException("Unable to switch to mount " + cameraMountId + " because it does not exist");
                }
            }

            private void TransitToEditingState() {
                _parachute.DetachPilot();
                _parachute.Destroy(0f.Seconds());   
                Machine.Transition(Playing.PlayingStates.EditingParachute, _environment);
            }

            private void OnSuspend() {
                _data.GameClock.Pause();
                _data.FixedClock.Pause();

                _data.GameHud.Deactivate();

                _data.SoundMixer.Pause(SoundLayer.GameEffects);

                if (_data.CameraManager.Rig.Shake) {
                    _data.CameraManager.Rig.Shake.enabled = false;
                }
            }

            private void OnExit() {
                OnSuspend();

                _data.GameHud.Deactivate();

                // Despawn parachute
                _data.ParachuteController.Parachute = null;

                // Ehhhhhh, weird if check, we actually need to distinguish between
                // exit to editing state and exit to wingsuit state
                if (_parachute != null) {
                    _parachute.DetachPilot();
                    _parachute.Destroy(3f.Seconds());   
                }

                if (_data.CameraManager.Rig.Shake) {
                    _data.CameraManager.Rig.Shake.enabled = false;
                }

                _data.ThirdPersonCameraMount.RemoveTarget();
            }
        }

        public class Editing : State {

            private readonly Data _data;
            private readonly ITypedDataCursor<ParachuteEditor.EditorState> _editorState;
            private readonly ITypedDataCursor<ParachuteConfig> _activeParachuteConfig;

            private PlayingEnvironment _environment;
            private IDisposable _disposeParachuteBuilder;
            private Parachute _editorParachute;
            private ParachuteConfig _currentConfig;
            private bool _isConfigUpdated;
            private readonly ISubject<Parachute> _editorParachuteChanges;

            public Editing(IStateMachine machine, Data data, 
                ParachuteStorage parachuteStorage, ITypedDataCursor<ParachuteStorageViewState> storageState) : base(machine) {

                _editorParachuteChanges = new ReplaySubject<Parachute>(1);

                _data = data;

                _editorState = storageState.To(c => c.EditorState);
                _activeParachuteConfig = _editorState.To(editorState => editorState.Config);

                // Toggle editor camera off when a gizmo interaction starts to avoid feedback loops
                //_data._cameraRig.Initialize();
                
                storageState.OnUpdate
                    // Store parachute every time it is changed (by sampling or throttling to reduce disk I/O)
                    // Store parachute every time the use selects a different parachute
                    .Throttle(TimeSpan.FromSeconds(2), Scheduler.ThreadPool)
                    .ObserveOn(UnityThreadScheduler.MainThread)
                    .Select(state => {
                        return state.AvailableParachutes
                            .Where(p => p.IsEditable)
                            .Select(p => {
                                var config = GameObject.Instantiate(p);
                                var json = JsonUtility.ToJson(config, prettyPrint: true);
                                return new { config, json };
                            })
                            .ToList();
                    }) // Copy object to prevent thread-unsafe editing
                    .ObserveOn(Schedulers.FileWriterScheduler)
                    .Subscribe(parachutes => {
                        parachuteStorage.DeleteAllStoredParachutes();
                        for (int i = 0; i < parachutes.Count; i++) {
                            var parachute = parachutes[i];
                            parachuteStorage.StoreParachute(parachute.config, parachute.json);
                        }
                    });

                data.ParachuteSelectionView.Initialize(storageState, data.GameSettingsProvider.IsVrActive);
                data.ParachuteSelectionView.BackToFlight += TransitToFlyingState;
            }

            void OnEnter(PlayingEnvironment environment) {
                _data.ParachuteEditor.Initialize(_editorState, _editorParachuteChanges);

                _environment = environment;
                var editLocation = _environment.Spawnpoint.AsParachuteLocation();
                var pilot = environment.Pilot;
                pilot.TrajectoryVisualizer.Deactivate();
                _data.ParachuteEditor.transform.Set(editLocation);
                _data.ParachuteEditor.gameObject.SetActive(true);

                _isConfigUpdated = false;
                _currentConfig = _activeParachuteConfig.Get();
                CreateEditorChute(_activeParachuteConfig.Get(), editLocation);
                _disposeParachuteBuilder = _activeParachuteConfig.OnUpdate
                    .Subscribe(config => {
                        _currentConfig = config;
                        _isConfigUpdated = true;
                    });
                
                _data.CameraManager.SwitchMount(_data.EditorCamera);
                _data.EditorCamera.Center();

                OnResume(Maybe<RespawnRequest>.Nothing, transitToParachuteEditor: false);
            }

            void CreateEditorChute(ParachuteConfig config, ImmutableTransform transform) {
                if (_editorParachute != null) {
                    _editorParachute.DetachPilot();
                    GameObject.Destroy(_editorParachute.Root.gameObject);
                }

                _editorParachute = _data.EditorSpawner.Create(config, transform, "EditorParachute");

                _environment.Pilot.SetKinematic();
                _environment.Pilot.OnDespawn();
                _environment.Pilot.transform.position = transform.Position;
                _environment.Pilot.transform.rotation = transform.Rotation;
                _environment.Pilot.OnSpawn();
                _environment.Pilot.SetKinematic();

                _editorParachute.AttachToPilot(_environment.Pilot, Parachute.DefaultUnfoldOrientation, _data.GameSettingsProvider);
                UnityParachuteFactory.SetKinematic(_editorParachute);
                _data.EditorCamera.SetTarget(_editorParachute);

                _editorParachuteChanges.OnNext(_editorParachute);
            }

            void OnResume(Maybe<RespawnRequest> respawnRequest, bool transitToParachuteEditor) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                _data.CameraManager.SwitchMount(_data.EditorCamera);

                _data.ParachuteEditor.OnMouseIsUnavailable += OnMouseIsUnavailable;
                _data.ParachuteEditor.OnMouseIsAvailable += OnMouseIsAvailable;
                _data.EditorCamera.enabled = true;

                _data.CursorHider.enabled = false;

                _data.InputModule.ReContextualize(_data.ParachuteSelectionView.FirstObject);
            }

            void Update() {
                var actionMap = _data.ParachuteActionMap.V;
                if (actionMap.ParachuteConfigToggle == ButtonEvent.Down) {
                    TransitToFlyingState();
                }
                if (_isConfigUpdated) {
                    CreateEditorChute(_currentConfig, _environment.Spawnpoint.AsParachuteLocation());
                    _isConfigUpdated = false;
                }
            }

            void TransitToFlyingState() {
                Machine.Transition(Playing.PlayingStates.FlyingParachute, _environment.UpdateParachuteConfig(_currentConfig));
            }

            void OnSuspend() {
                _data.ParachuteEditor.OnMouseIsUnavailable -= OnMouseIsUnavailable;
                _data.ParachuteEditor.OnMouseIsAvailable -= OnMouseIsAvailable;
                _data.EditorCamera.enabled = false;

                _data.CursorHider.enabled = true;
            }

            void OnExit() {
                OnSuspend();
                
                _environment.Pilot.TrajectoryVisualizer.Activate();
                _disposeParachuteBuilder.Dispose();

                _editorParachute.DetachPilot();
                GameObject.Destroy(_editorParachute.Root.gameObject);

                _data.ParachuteEditor.gameObject.SetActive(false);
            }

            private void OnMouseIsUnavailable() {
                _data.EditorCamera.enabled = false;
            }

            private void OnMouseIsAvailable() {
                _data.EditorCamera.enabled = true;
            }
        }
    }

}



