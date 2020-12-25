using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public abstract class Challenge : MonoBehaviour, IChallenge {
        public abstract ChallengeType ChallengeType { get; }
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract ImmutableTransform StartTransform { get; }
        public abstract event FinishChallenge OnFinished;
        public abstract IEnumerator<WaitCommand> Begin(FlightStatistics player);
    }
}
