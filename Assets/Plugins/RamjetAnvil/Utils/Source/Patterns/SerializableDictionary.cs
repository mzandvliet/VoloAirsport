using System;
using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    /// <summary>
    /// A partial dictionary implementation that can be serialized by Unity, and provides fast iteration.
    /// 
    /// To use it: Subclass it and make a concrete type. Otherwise Unity still cannot serialize it.
    /// </summary>
    /// <example>
    /// [System.Serializable]
    /// public class SerializedStringVector3Dictionary : SerializedDictionary&lt;string, Vector3&gt;
    /// </example>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> _keys;
        [SerializeField] private List<TValue> _values;

        public SerializableDictionary()
        {
            if (_keys == null)
                _keys = new List<TKey>();
            if (_values == null)
                _values = new List<TValue>();
        }

        public int Count
        {
            get { return _keys.Count; }
        }

        public void Add(TKey key, TValue value)
        {
            if (_keys.Contains(key))
                throw new Exception("Dictionary already contains entry for item");

            _keys.Add(key);
            _values.Add(value);
        }

        public bool ContainsKey(TKey key)
        {
            return _keys.Contains(key);
        }

        public bool Remove(TKey key)
        {
            int index = _keys.IndexOf(key);
            _values.RemoveAt(index);
            return _keys.Remove(key);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_keys.Contains(key))
                    throw new Exception("Dictionary does not contain entry for item");

                int index = _keys.IndexOf(key);
                return _values[index];
            }
            set
            {
                if (!_keys.Contains(key))
                    throw new Exception("Dictionary does not contain entry for item");

                int index = _keys.IndexOf(key);
                _values[index] = value;
            }
        }

        public IList<TValue> Values
        {
            get { return _values; }
        }

        public IList<TKey> Keys
        {
            get { return _keys; }
        }
    }
}