using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RamjetAnvil.RamNet {
    public interface INatPunchClient {
        NatPunchId Punch(IPEndPoint remoteEndpoint,
            OnNatPunchSuccess onSuccess = null,
            OnNatPunchFailure onFailure = null);
    }

    public delegate void OnNatPunchSuccess(NatPunchId punchId, IPEndPoint endPoint);
    public delegate void OnNatPunchFailure(NatPunchId punchId);

    public struct NatPunchId : IEquatable<NatPunchId> {
        public readonly string Value;

        public NatPunchId(string punchId) {
            Value = punchId;
        }

        public bool Equals(NatPunchId other) {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NatPunchId && Equals((NatPunchId) obj);
        }

        public override int GetHashCode() {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(NatPunchId left, NatPunchId right) {
            return left.Equals(right);
        }

        public static bool operator !=(NatPunchId left, NatPunchId right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("PunchId({0})", Value);
        }
    }
}
