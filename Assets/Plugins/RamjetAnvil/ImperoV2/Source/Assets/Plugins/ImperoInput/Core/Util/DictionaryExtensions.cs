using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Impero.Util
{
    public static class DictionaryExtensions
    {
        public static IImmutableDictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs) {
            return ImmutableDictionary<TKey, TValue>.Empty.AddRange(keyValuePairs);
        }

        public static IImmutableDictionary<TKey, TDestValue> ChangeValues<TKey, TSourceValue, TDestValue>(
            this IImmutableDictionary<TKey, TSourceValue> d, Func<TSourceValue, TDestValue> selector) {
            return d.Aggregate(ImmutableDictionary<TKey, TDestValue>.Empty, (current, keyValuePair) => 
                current.SetItem(keyValuePair.Key, selector(keyValuePair.Value)));
        }

        public static IImmutableDictionary<TDestKey, TValue> ChangeKeys<TSourceKey, TDestKey, TValue>(
            this IImmutableDictionary<TSourceKey, TValue> d, Func<TSourceKey, TDestKey> selector)
        {
            return d.Aggregate(ImmutableDictionary<TDestKey, TValue>.Empty, (current, keyValuePair) => 
                current.SetItem(selector(keyValuePair.Key), keyValuePair.Value));
        }

        public static TValue TryGet<TSourceKey, TValue>(this IImmutableDictionary<TSourceKey, TValue> d, TSourceKey key) {
            TValue value;
            if (d.TryGetValue(key, out value)) {
                return value;
            }
            return default(TValue);
        }

        public static Maybe<TValue> TryGetValue<TKey, TValue>(this IImmutableDictionary<TKey, TValue> d, TKey key) {
            TValue value;
            if (d.TryGetValue(key, out value)) {
                return Maybe.Just(value);
            }
            return Maybe.Nothing<TValue>();
        }

        public static Maybe<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key) {
            TValue value;
            if (d.TryGetValue(key, out value)) {
                return Maybe.Just(value);
            }
            return Maybe.Nothing<TValue>();
        }

        public static IImmutableDictionary<TKey, TValue> Merge<TKey, TValue>(this IEnumerable<IImmutableDictionary<TKey, TValue>> dictionaries) {
            return dictionaries.SelectMany(dictionary => dictionary)
                .Aggregate(ImmutableDictionary<TKey, TValue>.Empty, (current, keyValuePair) => 
                    current.SetItem(keyValuePair.Key, keyValuePair.Value));
        }
    }

}