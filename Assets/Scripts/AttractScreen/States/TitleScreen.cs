using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using Fmod = FMODUnity.RuntimeManager;

namespace RamjetAnvil.Volo.States {
    public class TitleScreen : State {
        [Serializable]
        public class Data {
            [SerializeField] public GameObject CameraMount;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public TitleScreenCameraAnimator Animator;

            [SerializeField] public MusicPlayer MusicPlayer;
            [SerializeField] public NotificationListView NotificationRenderer;
            [SerializeField] public GameObject TitleScreenLogo;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public AbstractUnityClock FixedClock;
            [SerializeField] public FaderSettings FaderSettings;
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            [SerializeField] public VersionChecker VersionChecker;

            [SerializeField] public UnityCoroutineScheduler CoroutineScheduler;

            [SerializeField] public VoloLogo Logo;
            [SerializeField] public float LogoDelayInS;
            [SerializeField] public float LogoFadeInTimeInS;
        }

        private Data _data;
        private ParticleSystemRenderer _logoParticleRenderer;
        private CompositeDisposable _startListener;
        private Func<ButtonEvent> _buttonEvents;
        private IDisposable _logoFadeIn;

        private bool _isRunning;

        public TitleScreen(IStateMachine machine, Data data) : base(machine) {
            _data = data;
            _data.TitleScreenLogo.SetActive(false); // Note: due to obscure crashbug, this has to be active in the serialized scene

            var buttonInput = ImperoCore
                .MergeAll(
                    Adapters.MergeButtons,
                    UnityInputIds.ControllerIds.Select(joystickId => Peripherals.Controller.Buttons(joystickId))
                        .Concat(new [] {Peripherals.Keyboard, Peripherals.Mouse.Buttons}));

            _buttonEvents = ImperoCore
                .MergePollFns(Adapters.MergeButtons, buttonInput.Source.Values)
                .Adapt(Adapters.ButtonEvents(() => Time.frameCount));
        }

        private IEnumerator<WaitCommand> OnEnter() {
            _isRunning = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            FixedClock.ResumePhysics();

            _data.NotificationRenderer.gameObject.SetActive(false);

            _data.MusicPlayer.StopAndFade();
            _data.MusicPlayer.PlayIntro();

            var titlescreenMount = _data.CameraMount.GetComponent<ICameraMount>();
            _data.CameraManager.SwitchMount(titlescreenMount);
            _data.CoroutineScheduler.Run(_data.Animator.Animate(titlescreenMount.transform, () => _isRunning));
            _logoFadeIn = _data.CoroutineScheduler.Run(LogoFadeIn());

            _data.CameraManager.Rig.ScreenFader.Opacity = 1f;
            yield return _data.CameraManager.Rig.ScreenFader.FadeIn(_data.GameClock,  _data.FaderSettings)
                .AsWaitCommand();
        }

        private IEnumerator<WaitCommand> LogoFadeIn() {
            yield return WaitCommand.WaitSeconds(_data.LogoDelayInS);
            _data.Logo.Show(true);
            _logoFadeIn = Disposables.Empty;
        }

        private void Update() {
            if (_buttonEvents() == ButtonEvent.Down) {
                if (_data.Logo.IsVisible) {
                    OnStartPressed();
                    _data.Logo.Show(false);
                } else {
                    // Fully display the logo       
                    Fmod.PlayOneShot("event:/ui/open");
                    _data.Logo.Show(true);
                }
            }
        }

        private void OnExit() {
            _isRunning = false;
            _logoFadeIn.Dispose();
            _data.NotificationRenderer.gameObject.SetActive(true);
            _data.Animator.enabled = false;

            _data.VersionChecker.CheckVersion();
        }

        private void OnStartPressed() {
            Fmod.PlayOneShot("event:/ui/open");
            Machine.Transition(VoloStateMachine.States.MainMenu);
        }
    }
}
