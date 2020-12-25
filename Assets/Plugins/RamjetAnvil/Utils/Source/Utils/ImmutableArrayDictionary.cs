using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RamjetAnvil.Unity.Utility {

    public class ImmutableArrayDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue> {

        private readonly IEqualityComparer<TValue> _comparer; 
        private readonly Func<TKey, int> _keyToIndex;
        private readonly Func<int, TKey> _indexToKey;
        private readonly bool[] _isSet;
        private readonly TValue[] _dict;
        private readonly int _count;

        public ImmutableArrayDictionary(int size, IEqualityComparer<TValue> comparer = null) {
            var intConverter = TypeConversion.IntConverter<TKey>();
            _keyToIndex = intConverter.ToInt;
            _indexToKey = intConverter.FromInt;
            _isSet = new bool[size];
            _dict = new TValue[size];
            _comparer = comparer;
            _count = 0;
        }

        public ImmutableArrayDictionary(Func<TKey, int> keyToIndex, Func<int, TKey> indexToKey, int size, IEqualityComparer<TValue> comparer = null) 
            : this(keyToIndex, indexToKey, new bool[size], new TValue[size], comparer ?? EqualityComparer<TValue>.Default) {}

        private ImmutableArrayDictionary(Func<TKey, int> keyToIndex, Func<int, TKey> indexToKey, bool[] isSet, TValue[] dict, 
            IEqualityComparer<TValue> comparer) {

            _keyToIndex = keyToIndex;
            _indexToKey = indexToKey;
            _isSet = isSet;
            _dict = dict;
            _comparer = comparer;
            _count = CalculateCount(isSet);
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

        public int Count {
            get { return _count; }
        }

        public bool ContainsKey(TKey key) {
            return _isSet[_keyToIndex(key)];
        }

        public bool TryGetValue(TKey key, out TValue value) {
            var index = _keyToIndex(key);
            if (_isSet[index]) {
                value = _dict[index];
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key] {
            get { return _dict[_keyToIndex(key)]; }
        }

        public IEnumerable<TKey> Keys {
            get {
                var keys = new List<TKey>();
                for (int i = 0; i < _dict.Length; i++) {
                    if (_isSet[i]) {
                        keys.Add(_indexToKey(i));
                    }
                }
                return keys;
            }
        }

        public IEnumerable<TValue> Values {
            get {
                var values = new List<TValue>();
                for (int i = 0; i < _dict.Length; i++) {
                    if (_isSet[i]) {
                        values.Add(_dict[i]);
                    }
                }
                return values;
            }
        }

        public IImmutableDictionary<TKey, TValue> Clear() {
            return new ImmutableArrayDictionary<TKey, TValue>(
                _keyToIndex, 
                _indexToKey, 
                isSet: new bool[_isSet.Length], 
                dict: new TValue[_dict.Length],
                comparer: _comparer);
        }

        public IImmutableDictionary<TKey, TValue> Add(TKey key, TValue value) {
            return AddRange(new []{new KeyValuePair<TKey, TValue>(key, value)});
        }

        public IImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs) {
            var newDict = new TValue[_dict.Length];
            Array.Copy(_dict, newDict, _dict.Length);
            var newIsSet = new bool[_isSet.Length];
            Array.Copy(_isSet, newIsSet, _isSet.Length);

            // TODO Throw exception if it exists
            foreach (var keyValuePair in pairs) {
                var index = _keyToIndex(keyValuePair.Key);
                newDict[index] = keyValuePair.Value;
                newIsSet[index] = true;
            }

            return new ImmutableArrayDictionary<TKey, TValue>(_keyToIndex, _indexToKey, newIsSet, newDict, _comparer);
        }

        public IImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value) {
            return SetItems(new[] {new KeyValuePair<TKey, TValue>(key, value)});
        }

        public IImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) {
            var newDict = new TValue[_dict.Length];
            Array.Copy(_dict, newDict, _dict.Length);
            var newIsSet = new bool[_isSet.Length];
            Array.Copy(_isSet, newIsSet, _isSet.Length);

            foreach (var keyValuePair in items) {
                var index = _keyToIndex(keyValuePair.Key);
                newDict[index] = keyValuePair.Value;
                newIsSet[index] = true;
            }

            return new ImmutableArrayDictionary<TKey, TValue>(_keyToIndex, _indexToKey, newIsSet, newDict, _comparer);
        }

        public IImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys) {
            var newDict = new TValue[_dict.Length];
            Array.Copy(_dict, newDict, _dict.Length);
            var newIsSet = new bool[_isSet.Length];
            Array.Copy(_isSet, newIsSet, _isSet.Length);

            foreach (var key in keys) {
                var index = _keyToIndex(key);
                newDict[index] = default(TValue);
                newIsSet[index] = false;
            }

            return new ImmutableArrayDictionary<TKey, TValue>(_keyToIndex, _indexToKey, newIsSet, newDict, _comparer);
        }

        public IImmutableDictionary<TKey, TValue> Remove(TKey key) {
            return RemoveRange(new[] {key});
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            var index = _keyToIndex(item.Key);
            var existingValue = _dict[index];
            return _isSet[index] && _comparer.Equals(existingValue, item.Value);
        }

        public bool TryGetKey(TKey equalKey, out TKey actualKey) {
            actualKey = equalKey;
            return true;
        }

        private int CalculateCount(bool[] isSet) {
            var count = 0;
            for (int i = 0; i < isSet.Length; i++) {
                count += isSet[i] ? 1 : 0;
            }
            return count;
        }
    }

    public static class ImmutableArrayDictionary {
        public static IImmutableDictionary<TKey, TValue> ToFastImmutableEnumDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> dict) {
            EnumUtils.AssertTypeIsEnum<TKey>();

            return new ImmutableArrayDictionary<TKey, TValue>(keyToIndex: EnumUtils.EnumToInt<TKey>(), 
                                                              indexToKey: EnumUtils.IntToEnum<TKey>(), 
                                                              size: EnumUtils.HighestEnumIndex<TKey>() + 1)
                .AddRange(dict);
        }
    }
}
