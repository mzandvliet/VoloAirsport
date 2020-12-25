using System;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {

    [Serializable]
    public class Guid {
        public static readonly Guid Empty = new Guid();

        [SerializeField] public byte[] Value;

        public Guid() {
            Value = new byte[16];
        }

        public Guid(byte[] value) {
            Value = value;
        }

        public int Length {
            get { return Value.Length; }
        }

        public void CopyFrom(Guid other) {
            Buffer.BlockCopy(other.Value, 0, Value, 0, Value.Length);
        }

        protected bool Equals(Guid other) {
            return CollectionUtil.StructuralEquals(this.Value, other.Value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Guid) obj);
        }

        public override int GetHashCode() {
            return (Value != null ? CollectionUtil.ArrayHashCode(Value) : 0);
        }

        public override string ToString() {
            return new System.Guid(Value).ToString();
        }

        public static Guid RandomGuid() {
            return new Guid(System.Guid.NewGuid().ToByteArray());
        }
    }
}
