using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.States;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public struct SpawnpointLocation {
        public readonly Vector3 Position;
        public readonly float Orientation;

        public SpawnpointLocation(Vector3 position, float orientation) {
            Position = position;
            Orientation = orientation;
        }

        public ImmutableTransform AsTransform {
            get {
                return new ImmutableTransform(
                    position: Position,
                    rotation: Quaternion.Euler(new Vector3(0f, Orientation, 0f)),
                    scale: Vector3.one);
            }
        }

        public static SpawnpointLocation ToLocation(Transform transform) {
            return ToLocation(transform.MakeImmutable());
        }

        public static SpawnpointLocation ToLocation(ImmutableTransform transform) {
            return new SpawnpointLocation(transform.Position, transform.Rotation.eulerAngles.y);
        }
    }

    public static class SpawnpointExtensions {
        public static ImmutableTransform AsWingsuitLocation(this SpawnpointLocation s) {
            return s.AsTransform.Rotate(new Vector3(100f, 0f, 0f));
        }

        public static ImmutableTransform AsParachuteLocation(this SpawnpointLocation s) {
            return s.AsTransform.Rotate(new Vector3(45f, 0f, 0f));
        }
    }

    
}

