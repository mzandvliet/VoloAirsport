using System;
using System.ComponentModel;
using UnityEngine;

namespace RamjetAnvil.InputModule {

    [Flags]
    public enum Axis {
        X = 1,
        Y = 2,
        Z = 4
    }

    public static class AxisExtensions {

        public static Axis FromIndex(int index) {
            switch (index) {
                case 0: return Axis.X;
                case 1: return Axis.Y;
                case 2: return Axis.Z;
                default: throw new InvalidEnumArgumentException("Invalid index: " + index);
            }
        }

        public static Vector3 LimitScale(this Vector3 vector, Axis axes) {
            return new Vector3 {
                x = axes.HasX() ? vector.x : 1f,
                y = axes.HasY() ? vector.y : 1f,
                z = axes.HasZ() ? vector.z : 1f
            };
        }

        public static bool Has(this Axis axis, Axis other) {
            return (axis & other) == other;
        }

        public static bool Has(this Axis axis, int index) {
            return axis.Has(FromIndex(index));
        }

        public static bool HasX(this Axis axis) {
            return (axis & Axis.X) == Axis.X;
        }

        public static bool HasY(this Axis axis) {
            return (axis & Axis.Y) == Axis.Y;
        }

        public static bool HasZ(this Axis axis) {
            return (axis & Axis.Z) == Axis.Z;
        }
    }
}
