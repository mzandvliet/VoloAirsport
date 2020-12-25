using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    /// <summary>
    /// Encapsulates a value and makes it mutable.
    /// 
    /// Even though the underlying value may be immutable the ref allows you to
    /// change the value it encapsulates.
    /// </summary>
    public class Ref<T> : IReadonlyRef<T> {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;
 
        public Ref(T initialValue) {
            var value = initialValue;
            _getter = () => value;
            _setter = newValue => value = newValue;
        }

        public Ref(Func<T> value, Action<T> setter) {
            _getter = value;
            _setter = setter;
        }

        public T V {
            get {
                return _getter();
            }
            set {
                _setter(value);
            }
        }
    }

    public static class RefExtensions {
        public static T Deref<T>(this IReadonlyRef<T> r) {
            return r.V;
        }
    }
}
