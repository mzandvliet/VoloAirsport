using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Networking;
using RamjetAnvil.Volo.UI;
using UnityEngine;

namespace RamjetAnvil.Volo.States {

    public class FlyWingsuit : State {

        [Serializable]
        public class Data {
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            [SerializeField] public UnityCoroutineScheduler CoroutineScheduler;
            //[SerializeField] public AbstractUnityClock MenuClock;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public AbstractUnityClock FixedClock;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public ThirdPersonCameraController ThirdPersonCameraMount;
            [SerializeField] public PlayerPilotSpawner PlayerPilotSpawner;
            //[SerializeField] public CourseManager CourseManager;
            [SerializeField] public GameHud GameHud;
            [SerializeField] public PilotActionMapProvider PilotActionMapProvider;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public NotificationList NotificationList;
            [SerializeField] public InputMappingsViewModel InputMappingsViewModel;

            // In-game systems
            [SerializeField] public GameObject PowerUps;

            [SerializeField] public ActiveNetwork ActiveNetwork;

            [SerializeField] public SoundMixer SoundMixer;
            [SerializeField] public FaderSettings FaderSettings;
        }

        private readonly Data _data;
        private PlayingEnvironment _environment;
        private bool _isRespawning;
        private bool _transitToParachuteEditor;

        private string _unfoldParachuteMappingStr;
        private IDisposable _openParachuteNotification;

        public FlyWingsuit(IStateMachine machine, Data data) : base(machine) {
            _data = data;
            
            data.ThirdPersonCameraMount.VrMode = data.CameraManager.VrMode != VrMode.None;

            _data.InputMappingsViewModel.InputMappings.Subscribe(inputMappings => {
                for (int i = 0; i < inputMappings.Length; i++) {
                    var inputMapping = inputMappings[i];
                    if (inputMapping.Id == new InputBindingId(InputBindingGroup.Wingsuit, WingsuitAction.UnfoldParachute)) {
                        _unfoldParachuteMappingStr = inputMapping.Binding;
                    }
                }
            });
        }

        IEnumerator<WaitCommand> OnEnter(PlayingEnvironment environment, Maybe<RespawnRequest> respawnRequest) {
            _environment = environment;
            // TODO Make sure if the player needs to be respawned
            yield return OnResume(respawnRequest, transitToParachuteEditor: false).AsWaitCommand();
        }

        IEnumerator<WaitCommand> OnResume(Maybe<RespawnRequest> respawnRequest, bool transitToParachuteEditor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _data.PowerUps.SetActive(true);

            _transitToParachuteEditor = transitToParachuteEditor;

            if (respawnRequest.IsJust) {
                yield return Respawn().AsWaitCommand();

                _data.ThirdPersonCameraMount.Clear();
            }

            _data.SoundMixer.Unpause(SoundLayer.GameEffects);

            _data.GameClock.Resume();
            _data.FixedClock.Resume();

            SwitchToMount(_environment.SelectedCameraMount);

            if (_data.CameraManager.Rig.Shake) {
                _data.CameraManager.Rig.Shake.enabled = true;
            }
            
            _data.GameHud.Activate();

            _data.PlayerPilotSpawner.ActivePilot.PilotAnimator.SetState(PilotAnimatorState.Wingsuit);
            _data.PlayerPilotSpawner.ActivePilot.EnableWings();

            _data.GameHud.SetTarget(_data.PlayerPilotSpawner.ActivePilot.FlightStatistics, AvatarType.Wingsuit);

            if (!_data.GameSettingsProvider.ActiveSettings.Tutorial.HasUnfoldedParachute) {
                _openParachuteNotification = _data.CoroutineScheduler.Run(NotifyAboutParachute());
            } else {
                _openParachuteNotification = Disposables.Empty;
            }

            //            if (_data.PlayerPilotSpawner.ActivePilot) {
            //                yield return _data.PlayerPilotSpawner.ActivePilot.AlphaManager
            //                    .SetAlphaAsync(1f, 0.5f.Seconds())
            //                    .AsWaitCommand();
            //            }
        }

        IEnumerator<WaitCommand> NotifyAboutParachute() {
            yield return WaitCommand.Wait(30.Seconds());
            
            if (_unfoldParachuteMappingStr != null) {
                _data.NotificationList.AddTimedNotification("Did you know you can press <i>" + _unfoldParachuteMappingStr + "</i> to open your parachute?", 10.Seconds());
                _openParachuteNotification = Disposables.Empty;    
            }

        }

        IEnumerator<WaitCommand> OnSuspend() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_data.CameraManager.Rig.Shake) {
                _data.CameraManager.Rig.Shake.enabled = false;
            }

            _data.PowerUps.SetActive(false);

            _data.SoundMixer.Pause(SoundLayer.GameEffects);

            _data.GameClock.Pause();
            _data.FixedClock.Pause();

            _data.GameHud.Deactivate();

            _openParachuteNotification.Dispose();

            yield return WaitCommand.DontWait;

//            if (_data.PlayerPilotSpawner.ActivePilot && _data.ActiveNetwork.IsSingleplayer) {
//                yield return _data.PlayerPilotSpawner.ActivePilot.AlphaManager
//                    .SetAlphaAsync(0f, 0.5f.Seconds())
//                    .AsWaitCommand();
//            }
        }

        IEnumerator<WaitCommand> OnExit() {
            // TODO We gotta somehow streamline mount switching with target switching
            //      now there are way to many places where we manually have to manage
            //      which target is attached to which mount
            _data.ThirdPersonCameraMount.RemoveTarget();
            yield return OnSuspend().AsWaitCommand();
        }

        IEnumerator<WaitCommand> Respawn() {
            // TODO Respawning is just another state
            _isRespawning = true;
            yield return _data.PlayerPilotSpawner.Respawn(_environment.Spawnpoint).AsWaitCommand();
            _environment = _environment.UpdatePilot(_data.PlayerPilotSpawner.ActivePilot);
            if (_data.CameraManager.Rig.Shake != null) {
                _data.CameraManager.Rig.Shake.SetTarget(_data.PlayerPilotSpawner.ActivePilot);
            }
            _isRespawning = false;
        }

        IEnumerator<WaitCommand> RespawnWithFade() {
            // TODO Respawning is just another state
            _isRespawning = true;
            yield return FadeOut().AsWaitCommand();
            yield return _data.PlayerPilotSpawner.Respawn(_environment.Spawnpoint).AsWaitCommand();
            _environment = _environment.UpdatePilot(_data.PlayerPilotSpawner.ActivePilot);
            _data.ThirdPersonCameraMount.Clear();
            SwitchToMount(_environment.SelectedCameraMount);
            yield return FadeIn().AsWaitCommand();
            _isRespawning = false;
        }

        void Update() {
            var pilotActionMap = _data.PilotActionMapProvider.ActionMap;
            var shouldRespawn = pilotActionMap.PollButtonEvent(WingsuitAction.Respawn) == ButtonEvent.Down;
            var shouldSwitchVehicleType = pilotActionMap.PollButtonEvent(WingsuitAction.UnfoldParachute) ==
                                          ButtonEvent.Down;

            if (!_isRespawning) {
                if (_transitToParachuteEditor) {
                    _transitToParachuteEditor = false;
                    Machine.Transition(Playing.PlayingStates.EditingParachute, _environment);
                } else if (shouldRespawn) {
                    _data.CoroutineScheduler.Run(RespawnWithFade());
                } else if (shouldSwitchVehicleType) {
                    Machine.Transition(Playing.PlayingStates.FlyingParachute, _environment);
                } else if (pilotActionMap.PollButtonEvent(WingsuitAction.ChangeCamera) == ButtonEvent.Down) {
                    _environment = _environment.NextMount();
                    SwitchToMount(_environment.SelectedCameraMount);
                } else if (pilotActionMap.PollButtonEvent(WingsuitAction.ToggleSpectatorView) == ButtonEvent.Down) {
                    Machine.Transition(Playing.PlayingStates.Spectating);
                } 
            }
        }

        private void SwitchToMount(PilotCameraMountId cameraMountId) {
            var wingsuit = _data.PlayerPilotSpawner.ActivePilot.GetComponent<Wingsuit>();

            if (cameraMountId == PilotCameraMountId.FirstPerson) {
                _data.ThirdPersonCameraMount.RemoveTarget();
                _data.CameraManager.SwitchMount(wingsuit.HeadCameraMount);
            } else if (cameraMountId == PilotCameraMountId.Orbit) {
                _data.ThirdPersonCameraMount.SetWingsuitTarget(wingsuit.FlightStatistics);
                _data.CameraManager.SwitchMount(_data.ThirdPersonCameraMount);
            }
        }

        private IEnumerator<WaitCommand> FadeOut() {
            yield return
                CameraTransitions.FadeOut(_data.GameClock, _data.FaderSettings, lerp => {
                    _data.CameraManager.Rig.ScreenFader.Opacity = lerp;
                    _data.SoundMixer.SetVolume(SoundLayer.Effects, 1f - lerp);
                }).AsWaitCommand();
        }

        private IEnumerator<WaitCommand> FadeIn() {
            yield return
                CameraTransitions.FadeIn(_data.GameClock, _data.FaderSettings, lerp => {
                    _data.CameraManager.Rig.ScreenFader.Opacity = lerp;
                    _data.SoundMixer.SetVolume(SoundLayer.Effects, 1f - lerp);
                }).AsWaitCommand();
        }
    }
}
