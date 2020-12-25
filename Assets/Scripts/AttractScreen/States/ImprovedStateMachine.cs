using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;

namespace RamjetAnvil.Volo {

    public delegate IEnumerator<WaitCommand> Enter<T>(T input);
    public delegate IEnumerator<WaitCommand> Exit();
    public delegate IEnumerator<WaitCommand> Update();
    public delegate IEnumerator<WaitCommand> SM<T>(T input);

    public static class ImprovedStateMachine {

        private static IEnumerator<WaitCommand> EmptyExit() {
            yield return WaitCommand.DontWait;
        }
        private static IEnumerator<WaitCommand> EmptyUpdate() {
            yield return WaitCommand.DontWait;
        }
        private static IEnumerator<WaitCommand> EmptyEnter<T>(T input) {
            yield return WaitCommand.DontWait;
        }

        public static SM<T> Create<T>(
            Enter<T> enter = null, 
            Exit onExit = null, 
            Update update = null) {

            enter = enter ?? EmptyEnter;
            onExit = onExit ?? EmptyExit;
            update = update ?? EmptyUpdate;

            return (input) => Create(enter, onExit, update, input);
        }

        private static IEnumerator<WaitCommand> Create<T>(
            Enter<T> enter,
            Exit exit,
            Update update,
            T input) {

            yield return enter(input).AsWaitCommand();
            // Update should itself decide when to stop
            yield return update().AsWaitCommand();
            yield return exit().AsWaitCommand();
        }

        public static SM<T> Loop<T>(SM<T> s, params SM<T>[] others) {
            return input => LoopInternal(input, s, others);
        }
        private static IEnumerator<WaitCommand> LoopInternal<T>(T input, SM<T> s, params SM<T>[] others) {
            while (true) {
                yield return s(input).AsWaitCommand();
                for (int i = 0; i < others.Length; i++) {
                    yield return others[i](input).AsWaitCommand();
                }
            }
        }

//        public static SM<T> Loop<T>(this SM<T> stateMachine, Func<bool> isPaused) {
//            return input => Loop(stateMachine, isPaused, input);
//        }
//        private static IEnumerator<WaitCommand> Loop<T>(SM<T> stateMachine, Func<bool> isPaused, T input) {
//            while (true) {
//                yield return stateMachine(input).AsWaitCommand();
//                if (isPaused()) {
//                    yield return WaitCommand.WaitForNextFrame;
//                }
//            }
//        }

    }
}
