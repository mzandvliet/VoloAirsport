using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;

namespace RamjetAnvil.Unity.Utility {
    public interface IClock {
        float DeltaTime { get; }
        double CurrentTime { get; }
        long FrameCount { get; }
        Routines.DeltaTime PollDeltaTime { get; }
        double TimeScale { get; set; }
    }

    public static class ClockExtensions {
        public static void Pause(this IClock clock) {
            clock.TimeScale = 0.0;
        }

        public static void Resume(this IClock clock) {
            clock.TimeScale = 1.0;
        }
    }

    public class NoOpClock : IClock {
        public static readonly NoOpClock Default = new NoOpClock();

        private NoOpClock() {}

        public float DeltaTime { get { return 0f; } }
        public double CurrentTime { get { return 0f;  } }
        public long FrameCount { get { return 0;  } }

        public Routines.DeltaTime PollDeltaTime {
            get {
                return () => TimeSpan.Zero;
            }
        }

        public double TargetTimeInterpolationSpeed { get; set; }
        public void SetCurrentTime(double time) {}
        public void SetCurrentTimeInterpolated(double time) {}
        public double TimeScale { get; set; }
    }
}
