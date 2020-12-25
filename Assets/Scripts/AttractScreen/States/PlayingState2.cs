//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RamjetAnvil.Coroutine;
//using RamjetAnvil.Impero.StandardInput;
//using RamjetAnvil.Unity.Utility;
//using UnityEngine;
//
//namespace Assets.Scripts.AttractScreen.States
//{
//    public class PlayingState2 {
//
//        // Instead of returning just an IEnumerator we might allow
//        // some room for additional control structures like a suspend operation
//
//        public IEnumerator<WaitCommand> Playing<TConfig, TState>(
//            TConfig initialConfig,
//            Func<TConfig, Maybe<TState>, TState> configToState, 
//            Immerse<TState> immerse, 
//            Edit<TConfig, TState> edit) {
//
//            TConfig config = initialConfig; // Read from disk for example
//            var state = configToState(config, Maybe<TState>.Nothing); // TODO Add initial place to spawn
//            while (true) {
//                var stateAfterImmersion = new AsyncResult<TState>();
//                // TODO While in immersion state we should be able to swap
//                //      between parachute and wingsuit
//                yield return immerse(state, stateAfterImmersion).AsWaitCommand();
//                state = stateAfterImmersion.Result;
//
//                var editingResult = new AsyncResult<Maybe<TConfig>>();
//                yield return edit(config, state, editingResult).AsWaitCommand();
//                if (editingResult.Result.IsJust) {
//                    config = editingResult.Result.Value;
//                    state = configToState(config, Maybe.Just(state)); // Based on the location of the old state
//                }
//            }
//        }
//    }
//
//    /*
//     * REquirements to make it easier for ourselves:
//     * - When editing the world will always pause
//     * - Editor is always running
//     * 
//     * - Options menu should always have the ability to pause and resume the current state,
//     *      - Menu options differ per game state
//     *      - Give menu id to options menu transition
//     *      
//     * - New state machine only for transition from wingsuit to parachute
//     * - Parachute immersion -> parachute editing
//     * 
//     * 
//     */ 
//
//    public interface IImmersion {
//        // Start(state, finalState) -> IEnumerator<WaitCommand>
//        // Stop();
//        // GetState() -> State
//    }
//
//    public delegate IEnumerator<WaitCommand> Immerse<TState>(
//        TState state, AsyncResult<TState> finalState);
//
//    public delegate IEnumerator<WaitCommand> Edit<TConfig, TState>(
//        TConfig config, TState state, AsyncResult<Maybe<TConfig>> result);
//
//    public class ParachuteState2 {
//
//        public Parachute FromConfig(ParachuteConfig config) {
//            return UnityParachuteFactory.Create(config, Vector3.zero, Quaternion.identity);
//        }
//
//        public class Immersion {
//            public IEnumerator<WaitCommand> Immerse(
//                Parachute parachute,
//                AsyncResult<Parachute> parachuteAfterImmersion) {
//            
//                parachuteAfterImmersion.SetResult(parachute);
//                yield return WaitCommand.WaitForNextFrame;
//            }
//        }
//
//        public class Edit {
//
//            public IEnumerator<WaitCommand> Edit(
//                ParachuteConfig config, 
//                Parachute state, 
//                AsyncResult<Parachute> parachuteAfterImmersion) {
//            
//                // TODO Allow user to edit parachute
//                Enter();
//                yield return Update().AsWaitCommand();
//                Exit(parachuteAfterImmersion);
//            }
//
//            void Enter() {
//                _data.ParachuteConfigView.Open(() => _isActive = false);
//                _data.ParachuteEditor.gameObject.SetActive(true);
//                // TODO Build new parachute from existing config
//                //      and apply state
//                RebuildParachute(_parachuteConfig.Get());
//
//                _data.CameraManager.Rig.Mount(_data.EditorCamera);
//                _data.EditorCamera.Center();
//            }
//
//            IEnumerator<WaitCommand> Update() {
//                var actionMap = _data.ParachuteActionMap.V;
//                while (actionMap.ParachuteConfigToggle != ButtonEvent.Down) {
//                    yield return WaitCommand.WaitForNextFrame;
//                }
//            }
//
//            void Exit(AsyncResult<Parachute> parachuteAfterImmersion) {
//                _data.ParachuteEditor.gameObject.SetActive(false);
//            }
//        }
//    }
//
//    public class WingsuitState2 {
//        
//    }
//}
