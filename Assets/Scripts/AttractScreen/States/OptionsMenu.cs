using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FMODUnity;
using Fmod = FMODUnity.RuntimeManager;
using RamjetAnvil.Coroutine;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Util;
using UnityEngine;

namespace RamjetAnvil.Volo.States {

    public abstract class Id<TValue> : IEquatable<Id<TValue>> {

        private readonly TValue _value;

        protected Id(TValue value) {
            _value = value;
        }

        public TValue Value {
            get { return _value; }
        }

        public static bool operator ==(Id<TValue> left, Id<TValue> right) {
            return Equals(left, right);
        }

        public static bool operator !=(Id<TValue> left, Id<TValue> right) {
            return !Equals(left, right);
        }

        public bool Equals(Id<TValue> other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TValue>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Id<TValue>) obj);
        }

        public override int GetHashCode() {
            return EqualityComparer<TValue>.Default.GetHashCode(_value);
        }

        public override string ToString() {
            return _value.ToString();
        }
    }


    public class MenuId : Id<string> {
        public static readonly MenuId MainMenu = new MenuId("mainMenu");
        public static readonly MenuId StartSelection = new MenuId("spawnpointMenu");
        public static readonly MenuId Playing = new MenuId("playing");
        public static readonly MenuId CourseEditor = new MenuId("courseEditor");
        public static readonly MenuId SpectatorMode = new MenuId("spectatorMode");

        public MenuId(string value) : base(value) {}
    }

    public class MenuActionId : Id<string> {
        public static readonly MenuActionId Back = new MenuActionId("Back");
        public static readonly MenuActionId Resume = new MenuActionId("Resume");
        public static readonly MenuActionId Restart = new MenuActionId("Respawn");
        public static readonly MenuActionId ChangeParachute = new MenuActionId("ChangeParachute");
        public static readonly MenuActionId StartSelection = new MenuActionId("SpawnpointMenu");
        public static readonly MenuActionId CourseEditor = new MenuActionId("CourseEditor");
        public static readonly MenuActionId TitleScreen = new MenuActionId("TitleScreen");
        public static readonly MenuActionId MainMenu = new MenuActionId("MainMenu");
        public static readonly MenuActionId Quit = new MenuActionId("Quit");

        public MenuActionId(string value) : base(value) { }
    }

    public class OptionsMenuState : State {
        [Serializable]
        public class Data {
            [SerializeField] public Ecology Ecology;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public OptionsMenu OptionsMenu;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public AbstractUnityClock FixedClock;
            [SerializeField] public UnityCoroutineScheduler CoroutineScheduler;
            [SerializeField] public ApplicationQuitter ApplicationQuitter;
            [SerializeField] public SoundMixer SoundMixer;
        }

        private readonly Data _data;

        public OptionsMenuState(IStateMachine machine, Data data) : base(machine) {
            _data = data;
        }

        private IEnumerator<WaitCommand> OnEnter(MenuId menuId) {
            _data.SoundMixer.Pause(SoundLayer.GameEffects);

            Cursor.lockState = CursorLockMode.None;

            // Prevent any input events from another state to leak to
            // the options menu
            yield return WaitCommand.WaitForNextFrame;
            _data.OptionsMenu.Open(menuId, CloseMenu(menuId));
            RuntimeManager.PlayOneShot("event:/ui/open");

            // Update the game settings with the ecology settings
            var currentSettings = _data.GameSettingsProvider.ActiveSettings;
            currentSettings.Gameplay.Time = _data.Ecology.Time;
            currentSettings.Gameplay.Weather = _data.Ecology.Weather;
            _data.GameSettingsProvider.UpdateGameSettings(currentSettings);
        }

        private IEnumerator<WaitCommand> OnExit() {
            // Wait for next frame to prevent any input events from the options menu
            // being interpreted by the next state
            yield return WaitCommand.WaitForNextFrame;
            Fmod.PlayOneShot("event:/ui/back");
        }

        private Action<MenuActionId, Action> CloseMenu(MenuId menuId) {
            // Todo: What if we defer handling this to PlayingState?
            return (actionId, onComplete) => {
                // State transition to
                var transitToParachuteEditor = actionId == MenuActionId.ChangeParachute;
                if (actionId == MenuActionId.Back || actionId == MenuActionId.Resume) {
                    if (menuId == MenuId.Playing) {
                        Machine.TransitionToParent(Maybe<RespawnRequest>.Nothing, transitToParachuteEditor);
                    } else {
                        Machine.TransitionToParent();    
                    }
                } else if (actionId == MenuActionId.ChangeParachute) {
                    Machine.TransitionToParent(Maybe<RespawnRequest>.Nothing, transitToParachuteEditor);
                } else if (actionId == MenuActionId.Restart) {
                    Machine.TransitionToParent(Maybe.Just(new RespawnRequest()), transitToParachuteEditor);
                } else if (actionId == MenuActionId.StartSelection) {
                    Machine.Transition(VoloStateMachine.States.SpawnScreen);
                } else if (actionId == MenuActionId.TitleScreen) {
                    Machine.Transition(VoloStateMachine.States.TitleScreen);
                } else if (actionId == MenuActionId.MainMenu) {
                    Machine.Transition(VoloStateMachine.States.MainMenu);
                } else if (actionId == MenuActionId.Quit) {
                    _data.ApplicationQuitter.RequestQuit();
                } else {
                    throw new ArgumentException("Cannot handle " + actionId);
                }

                if (onComplete != null) {
                    onComplete();
                }
            };
        }
    }
}
