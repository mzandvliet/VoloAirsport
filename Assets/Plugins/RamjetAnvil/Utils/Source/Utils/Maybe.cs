using System;
using System.Collections.Generic;
using System.Linq;

namespace RamjetAnvil.Unity.Utility {

    public static class Maybe {
        public static Maybe<T> Of<T>(T elem) {
            if (elem == null) {
                return Nothing<T>();
            }
            return Just(elem);
        }

        public static Maybe<TB> Of<TA, TB>(TA input, Func<TA, TB> elemConverter) {
            if (input == null) {
                return Nothing<TB>();
            }
            return Of(elemConverter(input));
        }

        public static Maybe<T> Just<T>(T elem) {
            return new Maybe<T>(isJust: true, value: elem);
        }

        public static Maybe<T> Nothing<T>() {
            return Maybe<T>.Nothing;
        }

        public static void Do<T>(this Maybe<T> maybe, Action<T> work) {
            if (maybe.IsJust) {
                work(maybe.Value);
            }
        }

        public static T GetOrDefault<T>(this Maybe<T> maybe) {
            if (maybe.IsJust) {
                return maybe.Value;
            }
            return default(T);
        }

        public static T GetOrElse<T>(this Maybe<T> maybe, Func<T> defaultValue) {
            if (maybe.IsJust) {
                return maybe.Value;
            }
            return defaultValue();
        }

        public static TOutput GetOrElse<T, TOutput>(this Maybe<T> maybe, Func<T, TOutput> converter,
            Func<TOutput> defaultValue) {
            if (maybe.IsJust) {
                return converter(maybe.Value);
            }
            return defaultValue();
        }

        public static Maybe<TOutput> Select<TInput, TOutput>(this Maybe<TInput> maybe, Func<TInput, TOutput> selector) {
            if (maybe.IsJust) {
                return Just(selector(maybe.Value));
            }
            return Nothing<TOutput>();
        }

        public static Maybe<T> Find<T>(this IEnumerable<T> coll, Func<T, bool> predicate) {
            try {
                return Just(coll.First(predicate));
            } catch (InvalidOperationException) {
                return Nothing<T>();
            }
        }
    }

    /// <summary>
    ///     The Maybe type encapsulates an optional value. A value of type Maybe a either contains a value of type a
    ///     (represented as Just a), or it is empty (represented as Nothing). Using Maybe is a good way to deal with errors or
    ///     exceptional cases without resorting to drastic measures such as error.
    /// </summary>
    public struct Maybe<T> : IEquatable<Maybe<T>> {

        public static readonly Maybe<T> Nothing = new Maybe<T>(isJust: false, value: default(T));

        private readonly bool _isJust;
        private readonly T _value;

        public Maybe(bool isJust, T value) {
            _isJust = isJust;
            _value = value;
        }

        public T Value {
            get {
                if (!_isJust) {
                    throw new Exception("Cannot retrieve value from nothing of type " + typeof(T));
                }
                return _value;
            }
        }

        public bool IsJust {
            get {
                return _isJust;
            }
        }

        public bool IsNothing {
            get {
                return !_isJust;
            }
        }

        public bool Equals(Maybe<T> other) {
            return _isJust.Equals(other._isJust) && EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Maybe<T> && Equals((Maybe<T>)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (_isJust.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(_value);
            }
        }

        public static bool operator ==(Maybe<T> left, Maybe<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Maybe<T> left, Maybe<T> right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            if (_isJust) {
                return "Just(" + _value + ")";
            }
            return "Nothing";
        }
    }
}