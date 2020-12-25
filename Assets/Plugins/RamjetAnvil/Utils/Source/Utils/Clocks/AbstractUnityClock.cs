using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public abstract class AbstractUnityClock : MonoBehaviour, IClock {
    public abstract float DeltaTime { get; }
    public abstract double CurrentTime { get; }
    public abstract long FrameCount { get; }
    public abstract Routines.DeltaTime PollDeltaTime { get; }
    public abstract double TimeScale { get; set; }
}
