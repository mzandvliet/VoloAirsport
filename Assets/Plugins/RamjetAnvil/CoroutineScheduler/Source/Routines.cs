using System;
using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.Coroutine
{
    public static class Routines {

        public delegate TimeSpan DeltaTime();
        public delegate float Animation(float lerp);

        public static readonly Animation LinearAnimation = value => value;
        public static readonly Animation EaseInOutAnimation = t => {
            t = Mathf.Clamp01(t);
            t *= 2f;
            if (t < 1f) return 0.5f * t * t * t * t;
            t -= 2f;
            return -0.5f * (t * t * t * t - 2f);
        };

        public static IEnumerator<WaitCommand> Animate(DeltaTime deltaTime, TimeSpan duration, Action<float> animator) {
            return Animate(deltaTime, duration, animator, LinearAnimation);
        }

        public static IEnumerator<WaitCommand> Animate(DeltaTime deltaTime, TimeSpan duration, Action<float> animator, Animation animation) {
            var time = TimeSpan.Zero;
            while (time < duration) {
                var lerp = (float)(time.TotalSeconds / duration.TotalSeconds);
                animator(animation(lerp));
                time += deltaTime();
                yield return WaitCommand.WaitForNextFrame;
            }
            animator(animation(1f));
        }

        public static IEnumerator<WaitCommand> Rotate(DeltaTime deltaTime, Transform transform, Quaternion target, TimeSpan duration, Animation animation = null) {
            animation = animation ?? EaseInOutAnimation;

            var startRotation = transform.rotation;
            return Animate(deltaTime, duration, lerp => transform.rotation = Quaternion.Slerp(startRotation, target, lerp), animation);
        }

        public static IEnumerator<WaitCommand> Translate(DeltaTime deltaTime, Transform transform, Vector3 newPosition, TimeSpan duration, Animation animation = null) {
            animation = animation ?? EaseInOutAnimation;

            var startPositon = transform.position;
            return Animate(deltaTime, duration, lerp => transform.position = Vector3.Lerp(startPositon, newPosition, lerp), animation);
        }

    }

    public static class AnimationExtensions {
        public static Routines.Animation Reverse(this Routines.Animation animation) {
            return t => animation(1f - t);
        }
    }
}
