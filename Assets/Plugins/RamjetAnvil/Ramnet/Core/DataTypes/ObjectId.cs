using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.RamNet {

    public struct ObjectId : IEquatable<ObjectId> {
        public readonly uint Value;

        public ObjectId(uint value) {
            Value = value;
        }

        public bool Equals(ObjectId other) {
            return Value == other.Value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ObjectId && Equals((ObjectId) obj);
        }

        public override int GetHashCode() {
            return (int) Value;
        }

        public static bool operator ==(ObjectId left, ObjectId right) {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectId left, ObjectId right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("ObjectId({0})", Value);
        }

        public static explicit operator uint(ObjectId objectType) {
            return objectType.Value;
        }
        public static explicit operator ObjectId(uint value) {
            return new ObjectId(value);
        }
    }
}
