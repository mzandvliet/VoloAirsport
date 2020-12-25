using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility {

    public class ArrayDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        private readonly IEqualityComparer<TValue> _comparer; 
        private readonly Func<TKey, int> _keyToIndex;
        private readonly Func<int, TKey> _indexToKey;

        private readonly IList<TKey> _keys;
        private readonly IList<TValue> _values;
        private readonly bool[] _isSet;
        private readonly TValue[] _dict;

        public ArrayDictionary(int size, IDictionary<TKey, TValue> existingDict = null,
            IEqualityComparer<TValue> comparer = null) {

            _comparer = comparer ?? EqualityComparer<TValue>.Default;
            _isSet = new bool[size];
            _keys = new List<TKey>(size);
            _values = new List<TValue>(size);
            _dict = new TValue[size];

            var intConverter = TypeConversion.IntConverter<TKey>();
            _keyToIndex = intConverter.ToInt;
            _indexToKey = intConverter.FromInt;

            if (existingDict != null) {
                for (int i = 0; i < existingDict.Count; i++) {
                    var key = _indexToKey(i);
                    TValue value;
                    if (existingDict.TryGetValue(key, out value)) {
                        Add(key, value);    
                    }
                }
            }
        }

        public ArrayDictionary(Func<TKey, int> keyToIndex, Func<int, TKey> indexToKey, int size, 
            IDictionary<TKey, TValue> existingDict = null,
            IEqualityComparer<TValue> comparer = null) {

            _comparer = comparer ?? EqualityComparer<TValue>.Default;
            _keyToIndex = keyToIndex;
            _indexToKey = indexToKey;
            _keys = new List<TKey>(size);
            _values = new List<TValue>(size);
            _isSet = new bool[size];
            _dict = new TValue[size];

            if (existingDict != null) {
                for (int i = 0; i < existingDict.Count; i++) {
                    var key = _indexToKey(i);
                    TValue value;
                    if (existingDict.TryGetValue(key, out value)) {
                        Add(key, value);    
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            for (int i = 0; i < _dict.Length; i++) {
                if (_isSet[i]) {
                    yield return new KeyValuePair<TKey, TValue>(_indexToKey(i), _dict[i]);    
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            _keys.Clear();
            _values.Clear();
            for (int i = 0; i < _dict.Length; i++) {
                _dict[i] = default(TValue);
                _isSet[i] = false;
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            var index = _keyToIndex(item.Key);
            return index < _dict.Length && _isSet[index] && _comparer.Equals(_dict[index], item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (array.Length - arrayIndex < _dict.Length) {
                throw new ArgumentException("array is too small to copy all elements of this dictionary");
            }

            for (int i = 0; i < _dict.Length; i++) {
                if (_isSet[i]) {
                    array[arrayIndex] = new KeyValuePair<TKey, TValue>(_indexToKey(i), _dict[i]);
                    arrayIndex++;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (Contains(item)) {
                return Remove(item.Key);
            }
            return false;
        }

        public int Count {
            get { return _keys.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public void Add(TKey key, TValue value) {
            var index = _keyToIndex(key);
            if (index < _dict.Length) {
                Remove(key);
                _keys.Add(key);
                _values.Add(value);
                _dict[index] = value; 
                _isSet[index] = true; 
            }
        }

        public bool ContainsKey(TKey key) {
            var index = _keyToIndex(key);
            return index < _dict.Length && _isSet[index];
        }

        public bool Remove(TKey key) {
            var index = _keyToIndex(key);
            if (index < _dict.Length && _isSet[index]) {
                _keys.Remove(key);
                _values.Remove(_dict[index]);
                _dict[index] = default(TValue);
                _isSet[index] = false;
                return true;
            } 
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            var index = _keyToIndex(key);
            if (index < _dict.Length && _isSet[index]) {
                value = _dict[index];
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key] {
            get { return _dict[_keyToIndex(key)]; } 
            set { Add(key, value); }
        }

        public ICollection<TKey> Keys {
            get { return _keys; }
        }

        public ICollection<TValue> Values {
            get { return _values; }
        }
    }

    public static class ArrayDictionary {
        public static IDictionary<TKey, TValue> FromValues<TKey, TValue>(params KeyValuePair<TKey, TValue>[] kvPairs) {

            var intConverter = TypeConversion.IntConverter<TKey>();
            var keyToIndex = intConverter.ToInt;
            var indexToKey = intConverter.FromInt;

            int largestIndex = 0;
            for (int i = 0; i < kvPairs.Length; i++) {
                var kvPair = kvPairs[i];
                var index = keyToIndex(kvPair.Key);
                largestIndex = index > largestIndex ? index : largestIndex;
            }

            var dict = new ArrayDictionary<TKey, TValue>(keyToIndex, indexToKey, size: largestIndex + 1);
            for (int i = 0; i < kvPairs.Length; i++) {
                var kvPair = kvPairs[i];
                dict.Add(kvPair.Key, kvPair.Value);
            }
            return dict;
        }

        public static IDictionary<TKey, TValue> EnumDictionary<TKey, TValue>() {
            EnumUtils.AssertTypeIsEnum<TKey>();

            return new ArrayDictionary<TKey, TValue>(
                keyToIndex: EnumUtils.EnumToInt<TKey>(), 
                indexToKey: EnumUtils.IntToEnum<TKey>(), 
                size: EnumUtils.HighestEnumIndex<TKey>() + 1);
        }
    }
}
