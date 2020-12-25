using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public static class LoadUtil {

        public static IEnumerator<WaitCommand> WaitUntilDoneInternal(AsyncOperation operation) {
            while (!operation.isDone) {
                yield return WaitCommand.WaitForNextFrame;
            }
            // Apparently we need to wait one extra frame for everything
            // in the scene to be properly initialized.
            yield return WaitCommand.WaitForNextFrame;
        }

        public static WaitCommand WaitUntilDone(this AsyncOperation operation) {
            return WaitUntilDoneInternal(operation).AsWaitCommand();
        }

    }
}
