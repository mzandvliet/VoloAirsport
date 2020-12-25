using System.Collections;
using System.Collections.Generic;

namespace RamjetAnvil.RamNet {

    public class ReadonlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> {

        private readonly IDictionary<TKey, TValue> _underlying;

        public ReadonlyDictionary(IDictionary<TKey, TValue> underlying) {
            _underlying = underlying;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return _underlying.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count { get { return _underlying.Count; } }

        public bool ContainsKey(TKey key) {
            return _underlying.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return _underlying.TryGetValue(key, out value);
        }

        public TValue this[TKey key] {
            get { return _underlying[key]; }
        }

        public IEnumerable<TKey> Keys { get { return _underlying.Keys; } }
        public IEnumerable<TValue> Values { get { return _underlying.Values; } }
    }

    public static class ReadonlyDictionary {
        public static IReadOnlyDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict) {
            return new ReadonlyDictionary<TKey, TValue>(dict);
        } 
    }
}
