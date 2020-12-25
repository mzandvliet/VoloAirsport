using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.RamNet {

    public struct ObjectType : IEquatable<ObjectType> {

        public readonly uint Value;

        public ObjectType(uint value) {
            Value = value;
        }

        public bool Equals(ObjectType other) {
            return Value == other.Value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ObjectType && Equals((ObjectType) obj);
        }

        public override int GetHashCode() {
            return (int) Value;
        }

        public static bool operator ==(ObjectType left, ObjectType right) {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectType left, ObjectType right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("ObjectType({0})", Value);
        }

        public static explicit operator uint(ObjectType objectType) {
            return objectType.Value;
        }
        public static explicit operator ObjectType(uint value) {
            return new ObjectType(value);
        }
    }
}
