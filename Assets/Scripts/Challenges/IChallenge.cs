using System;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public enum ChallengeType {
        BulletHell, Rings
    }

    public static class ChallengeTypeExtensions {
        public static string PrettyString(this ChallengeType t) {
            if (t == ChallengeType.BulletHell) {
                return "Bullet Hell";
            } else if (t == ChallengeType.Rings) {
                return "Rings";
            } else {
                throw new ArgumentOutOfRangeException("t", t, null);
            }
        }
    }

    public enum FinishCondition {
        Win, Loss
    }

    public delegate void FinishChallenge(FlightStatistics player, FinishCondition finishCondition);

    public interface IChallenge {
        ChallengeType ChallengeType { get; }
        string Id { get; }
        string Name { get; }

        ImmutableTransform StartTransform { get; }

        // Let the manager know that the player finished
        // challenge
        event FinishChallenge OnFinished;

        // Called by the challenge manager to start the actual challenge
        IEnumerator<WaitCommand> Begin(FlightStatistics player);
    }
}
