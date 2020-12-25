using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Impero.Util;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using ISpawnable = RamjetAnvil.Unity.Utility.ISpawnable;

namespace RamjetAnvil.Impero {

    public class InputMapping<TSourceId, TTargetId> : IEquatable<InputMapping<TSourceId, TTargetId>> {

        public IImmutableDictionary<TTargetId, IImmutableList<TSourceId>> Mappings { get; private set; }

        public InputMapping(IImmutableDictionary<TTargetId, IImmutableList<TSourceId>> mappings) {
            Mappings = mappings;
        }

        public bool Equals(InputMapping<TSourceId, TTargetId> other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Mappings.Count != other.Mappings.Count) return false;

            foreach (var mappingKvPair in Mappings) {
                var key = mappingKvPair.Key;
                var mapping = mappingKvPair.Value;
                IImmutableList<TSourceId> otherMapping;
                other.Mappings.TryGetValue(key, out otherMapping);
                if (!mapping.SequenceEqual(otherMapping)) {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InputMapping<TSourceId, TTargetId>) obj);
        }

        public override int GetHashCode() {
            return (Mappings != null ? Mappings.GetHashCode() : 0);
        }

        public static bool operator ==(InputMapping<TSourceId, TTargetId> left, InputMapping<TSourceId, TTargetId> right) {
            return Equals(left, right);
        }

        public static bool operator !=(InputMapping<TSourceId, TTargetId> left, InputMapping<TSourceId, TTargetId> right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "InputMapping(" + Mappings + ")";
        }
    }

    public class InputMappingBuilder<TSourceId, TTargetId> : ISpawnable {

        private readonly IObjectPool<IList<TSourceId>> _sourceInputPool; 
        private readonly IList<IPooledObject<IList<TSourceId>>> _pooledSourceInputs; 

        private readonly IDictionary<TTargetId, IPooledObject<IList<TSourceId>>> _mappings;

        public InputMappingBuilder() {
            _sourceInputPool = new RamjetAnvil.Unity.Utility.ObjectPool<IList<TSourceId>>(
                factory: () => new List<TSourceId>(), 
                onReturnedToPool: l => l.Clear(),
                growthStep: 10);
            _pooledSourceInputs = new List<IPooledObject<IList<TSourceId>>>();
            _mappings = new Dictionary<TTargetId, IPooledObject<IList<TSourceId>>>();
        }

        public InputMappingBuilder<TSourceId, TTargetId> Map(TSourceId source, TTargetId target) {
            IPooledObject<IList<TSourceId>> sourceInputs;
            if (!_mappings.TryGetValue(target, out sourceInputs)) {
                sourceInputs = _sourceInputPool.Take();
                _mappings[target] = sourceInputs;
            }
            sourceInputs.Instance.Add(source);

            return this;
        }

        public IImmutableDictionary<TTargetId, IImmutableList<TSourceId>> Build() {
            var finalMappings = ImmutableDictionary<TTargetId, IImmutableList<TSourceId>>.Empty;
            foreach (var mapping in _mappings) {
                finalMappings = finalMappings.SetItem(mapping.Key, mapping.Value.Instance.ToImmutableList());
            }
            return finalMappings;
        }

        public void OnSpawn() {
            for (int i = 0; i < _pooledSourceInputs.Count; i++) {
                var pooledSourceInput = _pooledSourceInputs[i];
                pooledSourceInput.Dispose();
            }
            _pooledSourceInputs.Clear();
            _mappings.Clear();
        }

        public void OnDespawn() {
        }
    }

    public static class InputMappingExtensions {

        public static InputMapping<TSource, TTarget> Merge<TTarget, TSource>(this InputMapping<TSource, TTarget> first,
            params InputMapping<TSource, TTarget>[] rest) {
            return Merge(new[] {first}.Concat(rest));
        }

        public static InputMapping<TSource, TTarget> Merge<TTarget, TSource>(
            this IEnumerable<InputMapping<TSource, TTarget>> inputMappings) {
            var mergedMapping = ImmutableDictionary<TTarget, IImmutableList<TSource>>.Empty;
            foreach (var inputMapping in inputMappings) {
                foreach (var mapping in inputMapping.Mappings) {
                    IImmutableList<TSource> mergedSources;
                    if (!mergedMapping.TryGetValue(mapping.Key, out mergedSources)) {
                        mergedSources = ImmutableList<TSource>.Empty;
                    }

                    foreach (var m in mapping.Value) {
                        mergedSources = mergedSources.Add(m);
                    }

                    mergedMapping = mergedMapping.SetItem(mapping.Key, mergedSources);
                }
            }
            return new InputMapping<TSource, TTarget>(mergedMapping);
        }

        public static InputMap<TTarget, TValue> ApplyMapping<TTarget, TSource, TValue>(
            this InputMapping<TSource, TTarget> inputMapping, 
            InputMap<TSource, TValue> inputSource,
            Func<TValue[], TValue> merge) {
            return inputMapping.Mappings
                .Select(kvPair => {
                    var targetId = kvPair.Key;
                    var sourceIds = kvPair.Value;
                    var sources = new List<Func<TValue>>();
                    foreach (var sourceId in sourceIds) {
                        Func<TValue> source;
                        if (inputSource.Source.TryGetValue(sourceId, out source)) {
                            sources.Add(source);
                        }
                    }
                    if (sources.Count > 0) {
                        var mergedSources = ImperoCore.MergePollFns(merge, sources);
                        return new KeyValuePair<TTarget, Func<TValue>>(targetId, mergedSources);
                    }
                    return new KeyValuePair<TTarget, Func<TValue>>(targetId, null);
                })
                .Where(kvPair => kvPair.Value != null)
                .ToDictionary()
                .ToInputMap();
        } 


        public static InputMap<TId, TValue> FillEmptyValues<TId, TValue>(this InputMap<TId, TValue> inputMap, 
            IEnumerable<TId> ids,
            Func<TValue> identityPollFn) {

            var map = inputMap.Source;
            foreach (var id in ids) {
                if (!map.ContainsKey(id)) {
                    //Debug.LogWarning("Filling empty action " + id);
                    map = map.Add(id, identityPollFn);
                }
            }
            return map.ToInputMap();
        }
    }
}