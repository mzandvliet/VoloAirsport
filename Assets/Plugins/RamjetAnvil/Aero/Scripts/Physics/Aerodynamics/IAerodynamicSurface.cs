using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Unity.Utils;
using UnityEngine;

public interface IAerodynamicSurface : IComponent, IUpdateEventSource<IAerodynamicSurface> {
    float Area { get; }
    Vector3 Center { get; }

    float AngleOfAttack { get; }
    Vector3 RelativeVelocity { get; }
    float AirSpeed { get; }
    Vector3 LiftForce { get; }
    Vector3 DragForce { get; }
    Vector3 MomentForce { get; }

    void Clear();
}
