using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.InputModule {

    public static class GameObjectExtensions {

        public static bool ContainsInHierarchy(this GameObject parent, GameObject potentialChild) {
            if (potentialChild != null) {
                Transform currentChild = potentialChild.transform;
                while (currentChild != null) {
                    if (currentChild.gameObject == parent) {
                        return true;
                    }
                    currentChild = currentChild.parent;
                }
            }
            return false;
        }

        private static readonly List<Transform> _transformCache = new List<Transform>();
        public static void SetLayerRecursively(this Transform go, int layer) {
            _transformCache.Clear();
            go.GetComponentsInChildren(includeInactive: true, result: _transformCache);
            for (int i = 0; i < _transformCache.Count; i++) {
                _transformCache[i].gameObject.layer = layer;
            }
        }
    }
}
