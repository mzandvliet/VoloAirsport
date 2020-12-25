using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RamjetAnvil.Unity.Utility
{
    public static class CollectionUtil {
        // Taken from: http://techmikael.blogspot.nl/2009/01/fast-byte-array-comparison-in-c.html
        public static bool StructuralEquals(byte[] strA, byte[] strB) {
            int length = strA.Length;
            if (length != strB.Length) {
                return false;
            }
            for (int i = 0; i < length; i++) {
                if (strA[i] != strB[i]) return false;
            }
            return true;
        }

        public static int ArrayHashCode(byte[] array) {
            unchecked {
                if (array == null) {
                    return 0;
                }
                int hash = 17;
                for (int i = 0; i < array.Length; i++) {
                    hash = hash * 31 + array[i];
                }
                return hash;
            }
        }

        public static void CopyListInto<T>(this IList<T> source, ICollection<T> destination) {
            for (int i = 0; i < source.Count; i++) {
                destination.Add(source[i]);
            }
        }

        // ---------------

        public static IDictionary<TValue, TKey> Flip<TKey, TValue>(this IDictionary<TKey, TValue> source) {
            var dest = new Dictionary<TValue, TKey>();
            foreach (var kvPair in source) {
                dest[kvPair.Value] = kvPair.Key;
            }
            return dest;
        }

        public static Maybe<TValue> Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return Maybe.Just(value);
            }
            return Maybe.Nothing<TValue>();
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue) {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }
            return defaultValue;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dict, TKey key, TValue defaultValue) {
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }
            return defaultValue;
        }

        public static T RandomElement<T>(this IList<T> l) {
            var index = UnityEngine.Random.Range(0, l.Count);
            return l[index];
        }

        public static bool HasValue<TElem, TComparable>(this IList<TElem> l,
            Func<TElem, TComparable, bool> predicate,
            TComparable comparable) where TElem : struct {
            for (int i = 0; i < l.Count; i++) {
                var elem = l[i];
                if (predicate(elem, comparable)) {
                    return true;
                }
            }
            return false;
        }

        public static void RemoveAt<TElem, TComparable>(this IList<TElem> l, Func<TElem, TComparable, bool> predicate, TComparable comparable) {
            for (int i = l.Count - 1; i >= 0; i--) {
                var elem = l[i];
                if (predicate(elem, comparable)) {
                    l.RemoveAt(i);
                }
            }
        }

        public static void UpdateAt<TElem, TComparable>(this IList<TElem> l, Func<TElem, TComparable, bool> predicate,
            TComparable comparable, TElem newValue) {
            for (int i = 0; i < l.Count; i++) {
                var elem = l[i];
                if (predicate(elem, comparable)) {
                    l[i] = newValue;
                }
            }
        }

        public static TElem? FindElement<TElem, TComparable>(this IList<TElem> l, 
            Func<TElem, TComparable, bool> predicate, 
            TComparable comparable) where TElem : struct {
            for (int i = 0; i < l.Count; i++) {
                var elem = l[i];
                if (predicate(elem, comparable)) {
                    return elem;
                }
            }
            return null;
        }

        public static T GetNextClamp<T>(this IList<T> l, T item) {
            var newIndex = Math.Min(l.IndexOf(item) + 1, l.Count - 1);
            return l[newIndex];
        }

        public static T GetPreviousClamp<T>(this IList<T> l, T item) {
            var newIndex = Math.Max(l.IndexOf(item) - 1, 0);
            return l[newIndex];
        }

        public static T GetNext<T>(this IList<T> l, T item) {
            var newIndex = (l.IndexOf(item) + 1) % l.Count;
            return l[newIndex];
        }

        public static T GetPrevious<T>(this IList<T> l, T item) {
            var newIndex = (l.IndexOf(item) - 1).PositiveModulo(l.Count);
            return l[newIndex];
        }

        public static T GetNext<T>(this IImmutableList<T> l, T item) {
            GetNext(l as IList<T>, item);
            var newIndex = (l.IndexOf(item) + 1) % l.Count;
            return l[newIndex];
        }

        public static T GetPrevious<T>(this IImmutableList<T> l, T item) {
            var newIndex = (l.IndexOf(item) - 1).PositiveModulo(l.Count);
            return l[newIndex];
        }

        public static IEnumerator<T> LoopingEnumerator<T>(IList<T> list) {
            int currentIndex = 0;
            while (true) {
                yield return list[currentIndex];
                currentIndex = (currentIndex + 1) % list.Count;
            }
        }

        public static IList<T> ToListOptimized<T>(this IEnumerable<T> enumerable) {
            return enumerable as List<T> ?? enumerable as IList<T> ?? enumerable.ToList();
        } 

    }
}
