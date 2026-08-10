using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.Coroutine.Unity {

    public static class Unity {

        public delegate UnityEngine.Coroutine StartRoutine(IEnumerator routine);

        public static IEnumerator ToUnity(this IEnumerator<WaitCommand> routine, StartRoutine startRoutine) {
            return routine.AsWaitCommand().ToUnity(startRoutine);
        }

        public static IEnumerator ToUnity(this WaitCommand waitCommand, StartRoutine startRoutine) {
            if (waitCommand.IsRoutine) {
                var runningRoutines = new UnityEngine.Coroutine[waitCommand.Routines.Length];
                for (int i = 0; i < waitCommand.Routines.Length; i++) {
                    var routine = waitCommand.Routines[i].ToUnity(startRoutine);
                    var runningRoutine = startRoutine(routine);
                    runningRoutines[i] = runningRoutine;
                }

                for (int i = 0; i < runningRoutines.Length; i++) {
                    var runningRoutine = runningRoutines[i];
                    yield return runningRoutine;
                }
            } else {
                if (waitCommand.Duration.Value.Seconds > 0f) {
                    yield return new WaitForSeconds(waitCommand.Duration.Value.Seconds);
                } else {
                    for (int i = 0; i < waitCommand.Duration.Value.FrameCount; i++) {
                        yield return null;
                    }
                }
            }
        }

    }
}
