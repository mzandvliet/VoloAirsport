using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Util;
using UnityEngine;

namespace RamjetAnvil.Volo.States {
    public class SpectatorMode : State {
        [Serializable]
        public class Data {
            [SerializeField] public SpectatorCamera SpectatorCamera;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public PilotActionMapProvider PilotActionMapProvider;
            [SerializeField] public SoundMixer SoundMixer;
        }

        private readonly Data _data;
        private readonly ICameraMount _spectatorCameraMount;

        private Maybe<RespawnRequest> _respawnRequest;
        private bool _transitToParachuteEditor;

        public SpectatorMode(IStateMachine machine, Data data) : base(machine) {
            _data = data;
            _spectatorCameraMount = _data.SpectatorCamera.GetComponent<ICameraMount>();
        }

        IEnumerator<WaitCommand> OnEnter() {
            FixedClock.PausePhysics();
            _data.SoundMixer.Pause(SoundLayer.GameEffects);

            OnResume(Maybe<RespawnRequest>.Nothing, transitToParachuteEditor: false);
            var cameraTransform = _data.CameraManager.Rig.transform.MakeImmutable();
            _spectatorCameraMount.transform.Set(cameraTransform);
            _data.CameraManager.SwitchMount(_spectatorCameraMount);

            yield return WaitCommand.WaitForNextFrame;
        }

        void OnResume(Maybe<RespawnRequest> respawnRequest, bool transitToParachuteEditor) {
            _data.SoundMixer.Pause(SoundLayer.GameEffects);

            _respawnRequest = respawnRequest;
            _transitToParachuteEditor = transitToParachuteEditor;
            _data.SpectatorCamera.enabled = true;
        }

        void OnSuspend() {
            _data.SpectatorCamera.enabled = false;
        }

        void OnExit() {
            _data.SoundMixer.Unpause(SoundLayer.GameEffects);
            OnSuspend();
        }

        void Update() {
            var pilotActionMap = _data.PilotActionMapProvider.ActionMap;
            var shouldQuitSpectator = 
                pilotActionMap.PollButtonEvent(WingsuitAction.ToggleSpectatorView) == ButtonEvent.Down ||
                _respawnRequest.IsJust ||
                _transitToParachuteEditor;

            if (shouldQuitSpectator) {
                Machine.TransitionToParent(_respawnRequest, _transitToParachuteEditor);
            }
        }

    }
}
