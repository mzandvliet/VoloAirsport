using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Volo.States {

    public class InCourseEditor : State {

        [Serializable]
        public class Data {
            [SerializeField] public CourseEditor CourseEditor;
            [SerializeField] public AbstractUnityClock Clock;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public FaderSettings FaderSettings;
            [SerializeField] public EditorOrbitCamera Mount;
            [SerializeField] public SpectatorCamera SpectatorCamera;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public AbstractUnityClock MenuClock;
        }

        private Data _data;
        private IDisposable _closeListener;

        public InCourseEditor(IStateMachine machine, Data data) : base(machine) {
            _data = data;
//            var inputSettings = _data.GameSettingsProvider.SettingChanges
//                .Select(settings => InputSettings.FromGameSettings(settings.Input));
//            _data.SpectatorCamera.ActionMap = SpectatorInput.ActionMap.Create(
//                new ActionMapConfig<SpectatorAction> {
//                    InputMapping = SpectatorInput.Bindings.DefaultMapping.Value,
//                    InputSettings = inputSettings,
//                    ControllerId = null
//                },
//                _data.MenuClock);
        }

        // When exiting always transition to spawnpoint selection
        // Allow options menu to be enabled

        IEnumerator<WaitCommand> OnEnter() {
            _data.CourseEditor.enabled = true;

            var currentCameraTransform = _data.CameraManager.Rig.transform.MakeImmutable();
            _data.Mount.transform.Set(currentCameraTransform);
            _data.CameraManager.SwitchMount(_data.Mount);

            yield return WaitCommand.WaitRoutine(CameraTransitions.FadeIn(_data.CameraManager.Rig.ScreenFader, _data.Clock, _data.FaderSettings));

            _closeListener = _data.CourseEditor.CloseEditor.Subscribe(_ => {
                Machine.Transition(VoloStateMachine.States.SpawnScreen);
            });
        }

        IEnumerator<WaitCommand> OnExit() {
            _closeListener.Dispose();
            yield return WaitCommand.WaitRoutine(CameraTransitions.FadeOut(_data.CameraManager.Rig.ScreenFader, _data.Clock, _data.FaderSettings));

            _data.CourseEditor.enabled = false;
        }
    }
}
