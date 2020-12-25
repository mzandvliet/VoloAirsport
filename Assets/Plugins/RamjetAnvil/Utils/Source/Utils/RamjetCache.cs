//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//
//namespace RamjetAnvil.Unity.Utility {
//
//    public interface ICache<in TKey, TValue> {
//        void Insert(TKey key, TValue value);
//        void Delete(TKey key);
//        bool TryGetValue(TKey key, out TValue value);
//    }
//
//    public static class CacheExtensions {
//        public static bool Contains<TKey, TValue>(this ICache<TKey, TValue> cache, TKey key) {
//            TValue value;
//            return cache.TryGetValue(key, out value);
//        }
//    }
//
//    public class RamjetCache<TKey, TValue> : ICache<TKey, TValue> {
//
//        private DateTime _timeSinceLastCleanup;
//        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
//        private readonly TimeSpan? _expireAfterAccess;
//        private readonly TimeSpan? _expireAfterWrite;
//        private readonly IList<TKey> _keys; 
//        private readonly IDictionary<TKey, CachedEntry<TValue>> _underlying;
//
//        public RamjetCache(TimeSpan? expireAfterAccess, TimeSpan? expireAfterWrite) : 
//            this(new Dictionary<TKey, CachedEntry<TValue>>(), expireAfterAccess, expireAfterWrite) {}
//
//        public RamjetCache(IDictionary<TKey, CachedEntry<TValue>> underlying, 
//            TimeSpan? expireAfterAccess, TimeSpan? expireAfterWrite) {
//
//            _underlying = underlying;
//            _keys = new List<TKey>();
//            _expireAfterAccess = expireAfterAccess;
//            _expireAfterWrite = expireAfterWrite;
//            _timeSinceLastCleanup = DateTime.Now;
//        }
//
//        public void Insert(TKey key, TValue value) {
//            var now = DateTime.Now;
//
//            CleanUp(now);
//            
//            _underlying[key] = new CachedEntry<TValue>(
//                insertionTime: now,
//                accessTime: now,
//                value: value);
//            _keys.Add(key);
//        }
//
//        public void Delete(TKey key) {
//            RemoveEntry(key);
//            CleanUp(DateTime.Now);
//        }
//
//        public bool TryGetValue(TKey key, out TValue value) {
//            var now = DateTime.Now;
//
//            CleanUp(now);
//
//            CachedEntry<TValue> cachedEntry;
//            bool isRetrievedFromCache;
//            if (_underlying.TryGetValue(key, out cachedEntry)) {
//                if (IsExpired(cachedEntry, now)) {
//                    RemoveEntry(key);
//                    isRetrievedFromCache = false;
//                    value = default (TValue);
//                } else {
//                    _underlying[key] = new CachedEntry<TValue>(cachedEntry.Value, cachedEntry.InsertionTime, accessTime: now);
//                    isRetrievedFromCache = true;
//                    value = cachedEntry.Value;
//                }
//            } else {
//                value = default (TValue);
//                isRetrievedFromCache = false;
//            }
//
//            return isRetrievedFromCache;
//        }
//
//        private void CleanUp(DateTime now) {
//            if (_timeSinceLastCleanup + _cleanupInterval < now) {
//                for (int i = _keys.Count - 1; i >= 0; i--) {
//                    var key = _keys[i];
//                    var cachedEntry = _underlying[key];
//                    if (IsExpired(cachedEntry, now)) {
//                        RemoveEntry(key);
//                    }
//                }
//
//                _timeSinceLastCleanup = now;
//            }
//        }
//
//        private bool IsExpired(CachedEntry<TValue> cachedEntry, DateTime now) {
//            return (_expireAfterAccess.HasValue &&
//                    cachedEntry.AccessTime + _expireAfterAccess.Value < now) ||
//                   (_expireAfterWrite.HasValue && 
//                    cachedEntry.InsertionTime + _expireAfterWrite.Value < now);
//        }
//
//        private void RemoveEntry(TKey key) {
//            _underlying.Remove(key);
//            _keys.Remove(key);
//        }
//    }
//
//    public class CachedEntry<TValue> {
//        public readonly TValue Value;
//        public readonly DateTime InsertionTime;
//        public readonly DateTime AccessTime;
//
//        public CachedEntry(TValue value, DateTime insertionTime, DateTime accessTime) {
//            Value = value;
//            InsertionTime = insertionTime;
//            AccessTime = accessTime;
//        }
//    }
//}
