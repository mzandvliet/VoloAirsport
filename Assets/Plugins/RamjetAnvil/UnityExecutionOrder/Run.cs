using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityExecutionOrder {

    /// <summary>
    /// Signifies an execution order dependency to another type.
    /// This type has two concrete implementations: Run.Before and Run.After
    /// </summary>
    public abstract class Run : Attribute, IEquatable<Run> {

        /// <summary>
        /// Signifies that the class that has this attribute needs to be executed before
        /// the given type.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class Before : Run {
            public Before(Type type) : base(type) {}

            public override string ToString() {
                return "Run.Before(" + _type + ")";
            }
        }

        /// <summary>
        /// Signifies that the class that has this attribute needs to be executed after
        /// the given type.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class After : Run {
            public After(Type type) : base(type) {}

            public override string ToString() {
                return "Run.After(" + _type + ")";
            }
        }

        private readonly Type _type;

        private Run(Type type) {
            _type = type;
        }

        public Type Type {
            get { return _type; }
        }

        public bool Equals(Run other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(_type, other._type);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Run) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (base.GetHashCode() * 397) ^ (_type != null ? _type.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Run left, Run right) {
            return Equals(left, right);
        }

        public static bool operator !=(Run left, Run right) {
            return !Equals(left, right);
        }
    }
}
