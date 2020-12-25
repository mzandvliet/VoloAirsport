using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    
    public static class IntVectorExtensions {

        public static Vector3 ToVector(this IntVector3 vector, int unitsPerMeter) {
            return new Vector3(
                x: vector.X / (float)unitsPerMeter,
                y: vector.Y / (float)unitsPerMeter,
                z: vector.Z / (float)unitsPerMeter);
        }

        public static IntVector3 ToIntVector(this Vector3 vector, int unitsPerMeter) {
            return new IntVector3(
                x: (int)vector.x * unitsPerMeter,
                y: (int)vector.y * unitsPerMeter,
                z: (int)vector.z * unitsPerMeter);
        }

        public static Vector2 ToVector(this IntVector2 vector, int unitsPerMeter) {
            return new Vector2(
                x: vector.X / (float)unitsPerMeter,
                y: vector.Y / (float)unitsPerMeter);
        }

        public static IntVector2 ToIntVector(this Vector2 vector, int unitsPerMeter) {
            return new IntVector2(
                x: (int)vector.x * unitsPerMeter,
                y: (int)vector.y * unitsPerMeter);
        }

        public static IntVector2 ToIntVector2(this IntVector3 v) {
            return new IntVector2(v.X, v.Y);
        }

        public static IntVector3 ToIntVector3(this IntVector2 v) {
            return new IntVector3(v.X, v.Y, z: 0);
        }
    }
}
