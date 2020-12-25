using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo {
    [Serializable]
    public struct PilotInputConfig {
        [SerializeField] public float InputSpeedScaling;
        [SerializeField] public float StallLimiterStrength;
        [SerializeField] public float RollLimiterStrength;
        [SerializeField] public float PitchAttitude;

        public PilotInputConfig(float inputSpeedScaling, float stallLimiterStrength, float rollLimiterStrength, float pitchAttitude) {
            InputSpeedScaling = inputSpeedScaling;
            StallLimiterStrength = stallLimiterStrength;
            RollLimiterStrength = rollLimiterStrength;
            PitchAttitude = pitchAttitude;
        }

        public override string ToString() {
            return string.Format("InputSpeedScaling: {0}, StallLimiterStrength: {1}, RollLimiterStrength: {2}", InputSpeedScaling, StallLimiterStrength, RollLimiterStrength);
        }

        public static readonly PilotInputConfig Default = new PilotInputConfig {
            InputSpeedScaling = 0.95f,
            StallLimiterStrength = 0.5f,
            RollLimiterStrength = 0.5f,
            PitchAttitude = 0.5f
        };
    }
}
