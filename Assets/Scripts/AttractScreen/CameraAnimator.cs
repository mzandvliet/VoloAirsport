using System;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.TitleScreen {

    public static class CameraAnimator {
        public static IEnumerator<WaitCommand> Animate(Transform mount, ICameraPath path, IClock clock) {
            // Wait at least one frame to prevent the editor from locking up when the duration is 0
            if (path.Duration < TimeSpan.Zero) {
                yield return WaitCommand.WaitForNextFrame;
            }

            yield return Routines.Animate(
                clock.PollDeltaTime,
                path.Duration, 
                lerp => {
                    mount.position = Vector3.Lerp(path.From.Position, path.To.Position, lerp);
                    mount.rotation = Quaternion.Lerp(path.From.Rotation, path.To.Rotation, lerp);
                }, 
                path.Animation).AsWaitCommand();
        }
    }
}