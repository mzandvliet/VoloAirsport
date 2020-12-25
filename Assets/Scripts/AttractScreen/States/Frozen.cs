using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.StateMachine;
using UnityEngine;

namespace RamjetAnvil.Volo.States {
    public class Frozen : State {
        [Serializable]
        public class Data {
            [SerializeField] public AbstractUnityClock[] Clocks;
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            //[SerializeField] public 
        }

        private readonly IStateMachine _stateMachine;
        private readonly Data _data;
        private IDisposable _disposable;
        private readonly double[] _clockScaleState;

        public Frozen(IStateMachine machine, Data data) : base(machine) {
            _stateMachine = machine;
            _data = data;
            _clockScaleState = new double[data.Clocks.Length];
            _data.EventSystem.Listen<Events.FreezeGame>(Freeze);
        }

        private void OnEnter() {
            _disposable = _data.EventSystem.Listen<Events.UnfreezeGame>(Unfreeze);
            // TODO We can't do this anymore because of multiplayer
            for (int i = 0; i < _data.Clocks.Length; i++) {
                var clock = _data.Clocks[i];
                _clockScaleState[i] = clock.TimeScale;
                clock.TimeScale = 0;
            }
        }

        private void OnExit() {
            for (int i = 0; i < _data.Clocks.Length; i++) {
                var clock = _data.Clocks[i];
                clock.TimeScale = _clockScaleState[i];
            }
            _disposable.Dispose();
        }

        private void Freeze() {
            _stateMachine.Transition(VoloStateMachine.States.Frozen);
        }

        private void Unfreeze() {
            _stateMachine.TransitionToParent();   
        }
    }
}
