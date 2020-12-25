using System;
using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.DependencyInjection {

    public class GameObjectInjector : IInjector {
        private static readonly List<MonoBehaviour> MonoBehaviourCache = new List<MonoBehaviour>();

        private readonly bool _traverseHierarchy;

        public GameObjectInjector(bool traverseHierarchy) {
            _traverseHierarchy = traverseHierarchy;
        }

        public void Inject<TContext>(object subject, Injection<TContext> injection, TContext dependencies, bool overrideExisting) {
            var gameObject = subject as GameObject;

            #if UNITY_EDITOR
            var isPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == null &&
                           UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null;
            if (isPrefab) {
                Debug.LogError("Injecting on Prefab '" + gameObject.name + "' is not allowed.", gameObject);
                throw new ArgumentException("Injecting on Prefab '" + gameObject.name + "' is not allowed.");
            }
            #endif

            MonoBehaviourCache.Clear();
            if (_traverseHierarchy) {
                gameObject.GetComponentsInChildren(includeInactive: true, results: MonoBehaviourCache);
            } else {
                gameObject.GetComponents(results: MonoBehaviourCache);
            }

            //Debug.Log("Injection on " + gameObject.name + ", components " + ComponentCache.Join(","));
            for (int i = 0; i < MonoBehaviourCache.Count; i++) {
                var component = MonoBehaviourCache[i];
                if (component != null) {
                    MonoBehaviourInjector.Default.Inject(component, injection, dependencies, overrideExisting);
                } else {
                    Debug.LogWarning("Trying to inject on null component on " + gameObject.name, gameObject);
                }
            }
        }

        public bool IsTypeSupported(Type t) {
            return t.IsAssignableFrom(typeof(GameObject));
        }
    }
}
