using System;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo {

    public static class CameraTransitions {
        public static IEnumerator<WaitCommand> FadeOut(this IScreenFader fader, IClock clock, FaderSettings faderSettings) {
            return FadeOut(clock, faderSettings, value => fader.Opacity = value);
        }

        public static IEnumerator<WaitCommand> FadeOut(IClock clock, FaderSettings faderSettings, Action<float> fader) {
            return Fade(clock, faderSettings.FadeOutDuration, Routines.EaseInOutAnimation, fader);
        }

        public static IEnumerator<WaitCommand> FadeIn(this IScreenFader fader, IClock clock, FaderSettings faderSettings) {
            return FadeIn(clock, faderSettings, value => fader.Opacity = value);
        }

        public static IEnumerator<WaitCommand> FadeIn(IClock clock, FaderSettings faderSettings, Action<float> fader) {
            return Fade(clock, faderSettings.FadeInDuration, Routines.EaseInOutAnimation.Reverse(), fader);
        }

        private static IEnumerator<WaitCommand> Fade(IClock clock, TimeSpan duration, Routines.Animation animation, Action<float> fader) {
            yield return Routines.Animate(clock.PollDeltaTime, duration, fader, animation).AsWaitCommand();
        }
    }
}
