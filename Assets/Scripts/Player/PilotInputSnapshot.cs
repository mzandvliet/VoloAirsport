using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public struct PilotInputSnapshot {
        public double Timestamp;
        public float Pitch;
        public float Roll;
        public float Yaw;
        public float Cannonball;
        public float CloseLeftArm;
        public float CloseRightArm;

        public static PilotInputSnapshot operator +(PilotInputSnapshot c1, PilotInputSnapshot c2) {
            return new PilotInputSnapshot {
                Timestamp = c1.Timestamp,
                Pitch = c1.Pitch + c2.Pitch,
                Roll = c1.Roll + c2.Roll,
                Yaw = c1.Yaw + c2.Yaw,
                Cannonball = c1.Cannonball + c2.Cannonball,
                CloseLeftArm = c1.CloseLeftArm + c2.CloseLeftArm,
                CloseRightArm = c1.CloseRightArm + c2.CloseRightArm
            };
        }

        public static PilotInputSnapshot Lerp(PilotInputSnapshot c1, PilotInputSnapshot c2, float lerp) {
            return new PilotInputSnapshot() {
                Timestamp = Mathd.Lerp(c1.Timestamp, c2.Timestamp, lerp),
                Pitch = Mathf.Lerp(c1.Pitch, c2.Pitch, lerp),
                Roll = Mathf.Lerp(c1.Roll, c2.Roll, lerp),
                Yaw = Mathf.Lerp(c1.Yaw, c2.Yaw, lerp),
                Cannonball = Mathf.Lerp(c1.Cannonball, c2.Cannonball, lerp),
                CloseLeftArm = Mathf.Lerp(c1.CloseLeftArm, c2.CloseLeftArm, lerp),
                CloseRightArm = Mathf.Lerp(c1.CloseRightArm, c2.CloseRightArm, lerp),
            };
        }

        public PilotInputSnapshot Merge(PilotInputSnapshot c) {
            return new PilotInputSnapshot {
                Timestamp = c.Timestamp,
                Pitch = Adapters.MergeAxes(Pitch, c.Pitch),
                Roll = Adapters.MergeAxes(Roll, c.Roll),
                Yaw = Adapters.MergeAxes(Yaw, c.Yaw),
                Cannonball = Adapters.MergeAxes(Cannonball, c.Cannonball),
                CloseLeftArm = Adapters.MergeAxes(CloseLeftArm, c.CloseLeftArm),
                CloseRightArm = Adapters.MergeAxes(CloseRightArm, c.CloseRightArm),
            };
        }

        public static readonly PilotInputSnapshot Zero = new PilotInputSnapshot();

        public override string ToString() {
            return string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}, Cannonball: {3}, CloseLeftArm: {4}, CloseRightArm: {5}", Pitch, Roll, Yaw, Cannonball, CloseLeftArm, CloseRightArm);
        }
    }

}
