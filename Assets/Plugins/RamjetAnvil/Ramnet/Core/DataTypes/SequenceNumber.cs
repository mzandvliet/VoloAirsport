using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    /// <summary>
    /// Taken from: http://algorithmicassertions.com/math/2014/03/11/Ordering-Cyclic-Sequence-Numbers.html
    /// </summary>
    public struct SequenceNumber : IEquatable<SequenceNumber> {
        public readonly ushort Value;

        public SequenceNumber(ushort value) {
            Value = value;
        }

        public bool Equals(SequenceNumber other) {
            return Value == other.Value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SequenceNumber && Equals((SequenceNumber) obj);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public static bool operator <=(SequenceNumber left, SequenceNumber right) {
            return left < right || left == right;
        }

        public static bool operator >=(SequenceNumber left, SequenceNumber right) {
            return left > right || left == right;
        }

        public static bool operator <(SequenceNumber left, SequenceNumber right) {
            return right > left;
        }

        public static bool operator >(SequenceNumber left, SequenceNumber right) {
            var diff = (short) (left.Value - right.Value);
            return diff > 0;
        }

        public static bool operator ==(SequenceNumber left, SequenceNumber right) {
            return left.Equals(right);
        }

        public static bool operator !=(SequenceNumber left, SequenceNumber right) {
            return !left.Equals(right);
        }

        public SequenceNumber Increment() {
            return new SequenceNumber((ushort) (Value + 1));
        }

        public override string ToString() {
            return string.Format("SequenceNumber({0})", Value);
        }

        public static explicit operator ushort(SequenceNumber sequenceNumber) {
            return sequenceNumber.Value;
        }
        public static explicit operator SequenceNumber(ushort value) {
            return new SequenceNumber(value);
        }
    }
}
