using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Util;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Impero {

    public class InputMap<TInputId, TValue> {
        private TInputId[] _keys; 
        private KeyValuePair<TInputId, Func<TValue>>[] _keyValuePairs; 
        private readonly IImmutableDictionary<TInputId, Func<TValue>> _map;

        public static readonly InputMap<TInputId, TValue> Empty = new InputMap<TInputId, TValue>(
            ImmutableDictionary<TInputId, Func<TValue>>.Empty);

        public InputMap() {
            _map = ImmutableDictionary<TInputId, Func<TValue>>.Empty;
        }

        public InputMap(IImmutableDictionary<TInputId, Func<TValue>> map) {
            _map = map;
        }

        public IImmutableDictionary<TInputId, Func<TValue>> Source {
            get { return _map; }
        }

        public TValue PollOrDefault(TInputId id, TValue @default = default(TValue)) {
            Func<TValue> pollFn;
            if (_map.TryGetValue(id, out pollFn)) {
                return pollFn();
            }
            return @default;
        }

        public TValue Poll(TInputId id) {
            Func<TValue> pollFn;
            if (_map.TryGetValue(id, out pollFn)) {
                return pollFn();
            }
            throw new Exception("Input id '" + id + "' doesn't exist in input map of '" + typeof (TInputId) + "'");
        }

        public TInputId[] Keys {
            get {
                if (_keys == null) {
                    _keys = Source.Keys.ToArray();
                }
                return _keys;
            }
        }

        public KeyValuePair<TInputId, Func<TValue>>[] KeyValuePairs {
            get {
                if (_keyValuePairs == null) {
                    _keyValuePairs = Source.ToArray();
                }
                return _keyValuePairs;
            }
        }
    }
     
    public static class ImperoCore {
        /// <summary>
        ///     Wraps the dictionary into an input map.
        ///     Same as calling the constructor of the InputMap.
        /// </summary>
        /// <param name="source">the heart of the InputMap</param>
        /// <returns>an InputMap</returns>
        public static InputMap<TInputId, TValue> ToInputMap<TInputId, TValue>(
            this IEnumerable<KeyValuePair<TInputId, Func<TValue>>> source) {
            return ToInputMap(source.ToDictionary());
        }

        /// <summary>
        ///     Wraps the dictionary into an input map.
        ///     Same as calling the constructor of the InputMap.
        /// </summary>
        /// <param name="source">the heart of the InputMap</param>
        /// <returns>an InputMap</returns>
        public static InputMap<TInputId, TValue> ToInputMap<TInputId, TValue>(
            this IImmutableDictionary<TInputId, Func<TValue>> source) {
            return new InputMap<TInputId, TValue>(source);
        }

        /// <summary>
        ///     Replaces the dictionary inside the input map with a dictionary that has fast and garbage-less lookup.
        ///     Warning, works only for Enum types.
        /// </summary>
        public static InputMap<TInputId, TValue> Optimize<TInputId, TValue>(this InputMap<TInputId, TValue> inputMap)
            where TInputId : struct, IComparable, IConvertible, IFormattable {

            return new InputMap<TInputId, TValue>(inputMap.Source.ToFastImmutableEnumDictionary());
        }

        /// <summary>
        ///     Applies the adapter to the given poll function returning a new poll function
        ///     that threads the output of the given poll function through the adapter before returning it.
        /// </summary>
        /// <typeparam name="TSource">the type of the source returned by the poll function</typeparam>
        /// <typeparam name="TDest">the type of the adapted poll function as specified by the adapter</typeparam>
        /// <param name="adapter">a function that transforms the value coming from the given poll function to something else</param>
        /// <param name="pollFn">the subject</param>
        /// <returns>a poll function that produces an adapted value</returns>
        public static Func<TDest> Adapt<TSource, TDest>(this Func<TSource> pollFn, Func<TSource, TDest> adapter) {
            return () => adapter(pollFn());
        }

        /// <summary>
        ///     Adapts all poll functions in the InputMap
        /// </summary>
        public static InputMap<TInputId, TDest> Adapt<TInputId, TSource, TDest>(
            this InputMap<TInputId, TSource> inputMap, Func<TInputId, Func<TSource>, Func<TDest>> adaptInput) {
            return inputMap.Source.Select(sourceEntry => {
                TInputId pollKey = sourceEntry.Key;
                Func<TDest> adaptedPollFn = adaptInput(sourceEntry.Key, sourceEntry.Value);
                return new KeyValuePair<TInputId, Func<TDest>>(pollKey, adaptedPollFn);
            }).ToInputMap();
        }

        /// <summary>
        ///     Adapts all poll functions in the InputMap based on a single adapter
        /// </summary>
        public static InputMap<TInputId, TDest> Adapt<TInputId, TSource, TDest>(
            this InputMap<TInputId, TSource> inputMap, Func<TSource, TDest> adapter) {
            return inputMap.Source
                .ChangeValues(pollFn => Adapt(pollFn, adapter))
                .ToInputMap();
        }

        /// <summary>
        ///     Adapts all poll functions in the InputMap providing a new adapter for each poll function.
        ///     Useful if you're adapters are stateful and store state per poll function.
        /// </summary>
        public static InputMap<TInputId, TDest> Adapt<TInputId, TSource, TDest>(
            this InputMap<TInputId, TSource> inputMap, Func<Func<TSource, TDest>> adapterProvider) {
            return inputMap.Source
                .ChangeValues(pollFn => Adapt(pollFn, adapterProvider()))
                .ToInputMap();
        }

        public static InputMap<TInputId, TValue> Adapt<TInputId, TValue>(this InputMap<TInputId, TValue> inputMap,
            TInputId id, Func<TValue, TValue> adapter) {
            Func<TValue> pollFn;
            if (inputMap.Source.TryGetValue(id, out pollFn)) {
                inputMap = inputMap
                    .Source
                    .SetItem(id, pollFn.Adapt(adapter))
                    .ToInputMap();
            }
            return inputMap;
        } 

        public static InputMap<TInputId, TValue> Update<TInputId, TValue>(this InputMap<TInputId, TValue> inputMap, 
            TInputId id, Func<TValue> pollFn) {
            return inputMap.Source.SetItem(id, pollFn).ToInputMap();
        }

        /// <summary>
        ///     Change all the id's inside an InputMap and return a new InputMap with all Id's changed.
        /// </summary>
        public static InputMap<TDestId, TValue> ChangeId<TSourceId, TDestId, TValue>(
            this InputMap<TSourceId, TValue> inputMap, Func<TSourceId, TDestId> idConverter) {
            return inputMap.Source.ChangeKeys(idConverter).ToInputMap();
        }

        /// <summary>
        ///     Creates an input map from a given set of id's and a poll function.
        ///     This function extracts the id from the poll function and stores it
        ///     separately.
        ///     Lookup is now separated from
        ///     polling allowing you to do all sorts of things like mapping another id onto
        ///     the poll function or adapting the poll function without changing or knowing anything
        ///     about the id or lookup functionality.
        /// </summary>
        public static InputMap<TId, TDest> ToInputMap<TId, TDest>(IEnumerable<TId> ids,
            Func<TId, TDest> pollFn) {
            return ids
                .ToDictionary(id => id, id => EncapsulateId(id, pollFn))
                .ToInputMap();
        }

        private static Func<TValue> EncapsulateId<TInputId, TValue>(TInputId id, Func<TInputId, TValue> pollFn) {
            return () => pollFn(id);
        }

        public static Func<T> MergePollFns<T>(Func<T[], T> merge, params Func<T>[] pollFns) {
            return MergePollFns(merge, pollFns as IEnumerable<Func<T>>);
        }

        /// <summary>
        ///     Useful when for example you have multiple buttons that you would like to use to trigger one specific
        ///     in-game action. This function returns one poll function that calls all given poll functions for
        ///     you and merges the outputs with a merge function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="merge"></param>
        /// <param name="initialValue"></param>
        /// <param name="pollFns"></param>
        /// <returns></returns>
        public static Func<T> MergePollFns<T>(Func<T[], T> merge, IEnumerable<Func<T>> pollFns) {
            // Convert to array for speed
            var pollFnList = pollFns.ToArray();

            // Optimization, no overhead when merging is not required.
            if (pollFnList.Length == 1) {
                return pollFnList[0];
            }

            return MergePollFnsInternal(merge, pollFnList);
        }

        private static Func<T> MergePollFnsInternal<T>(Func<T[], T> merge, Func<T>[] pollFns) {
            var buffer = new T[pollFns.Length];
            return () => {
                for (int i = 0; i < pollFns.Length; i++) {
                    buffer[i] = pollFns[i]();
                }
                return merge(buffer);
            };
        }

        public static InputMap<TId, TValue> MergeAll<TId, TValue>(
            this IImmutableDictionary<TId, IEnumerable<Func<TValue>>> inputMap,
            Func<TValue[], TValue> merge) {
            return inputMap.SelectMany(input => {
                if (input.Value.Any()) {
                    return new[] {new KeyValuePair<TId, Func<TValue>>(input.Key, MergePollFns(merge, input.Value))};
                }
                return Enumerable.Empty<KeyValuePair<TId, Func<TValue>>>();
            }).ToInputMap();
        }

        public static InputMap<TId, TValue> MergeAll<TId, TValue>(Func<TValue[], TValue> merge,
            IEnumerable<InputMap<TId, TValue>> inputMaps) {
            return MergeAll(Combine(inputMaps), merge);
        }

        public static InputMap<TId, TValue> MergeAll<TId, TValue>(Func<TValue[], TValue> merge,
            params InputMap<TId, TValue>[] inputMaps) {
            return MergeAll(Combine(inputMaps), merge);
        }

        public static Func<TValue> Merge<TId, TValue>(this InputMap<TId, TValue> inputMap,
            Func<TValue[], TValue> merge,
            params TId[] ids) {
            var pollFns = inputMap.Source
                .Where(pollPair => ids.Contains(pollPair.Key))
                .Select(pollPair => pollPair.Value);
            return MergePollFns(merge, pollFns);
        }

        public static InputMap<TId, TValue> Filter<TId, TValue>(this InputMap<TId, TValue> source,
            Func<TId, bool> perdicate) {
            return source.Source
                .Where(kvPair => perdicate(kvPair.Key))
                .ToInputMap();
        }

        private static IImmutableDictionary<TId, IEnumerable<Func<TValue>>> Combine<TId, TValue>(
            IEnumerable<InputMap<TId, TValue>> allMappedInputs) {
                var result = ImmutableDictionary<TId, IEnumerable<Func<TValue>>>.Empty;
                foreach (var mappedInputs in allMappedInputs) {
                    foreach (var mappedInput in mappedInputs.Source) {
                        IEnumerable<Func<TValue>> pollFns;
                        if (!result.TryGetValue(mappedInput.Key, out pollFns)) {
                            pollFns = ImmutableList<Func<TValue>>.Empty;
                        }
                        result = result.SetItem(mappedInput.Key, ((IImmutableList<Func<TValue>>) pollFns).Add(mappedInput.Value));
                    }
                }
                return result;
        }

        // Generic utility

        /// <summary>
        ///     Caches the output of pollFn every frame. Useful if calling pollFn is an expensive operation
        ///     or when the pollFn may only be called once per frame.
        /// </summary>
        public static Func<T> Cache<T>(this Func<T> pollFn, Func<int> currentFrame) {
            return Adapt<T, T>(pollFn, CacheByFrame<T, T>(Adapters.Identity, currentFrame));
        }

        /// <summary>
        ///     Caches the output of pollFn every frame. Useful if calling pollFn is an expensive operation
        ///     or when the pollFn may only be called once per frame.
        /// </summary>
        public static Func<T> Cache<T>(this Func<T> pollFn, Func<float> currentTime) {
            float lastCachedTimestamp = -1f;
            T cachedValue = default(T);
            return () => {
                if (lastCachedTimestamp < currentTime()) {
                    cachedValue = pollFn();
                    lastCachedTimestamp = currentTime();
                }
                return cachedValue;
            };
        }

        public static Func<TInput, TOutput> CacheByFrame<TInput, TOutput>(Func<TInput, TOutput> adapter, Func<int> currentFrame) {
            TOutput cachedValue = default(TOutput);
            var lastFrame = -1;
            return input => {
                if (currentFrame() > lastFrame) {
                    cachedValue = adapter(input);
                    lastFrame = currentFrame();
                }
                return cachedValue;
            };
        }

        public static class Adapters  {
            public static T Identity<T>(T input) {
                return input;
            }
        }
    }
}