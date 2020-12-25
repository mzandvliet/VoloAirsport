using System;
using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public static class GameObjectUtil
    {
        public static GameObject SetName(this GameObject gameObject, string name) {
            gameObject.name = name;
            return gameObject;
        }

        public static GameObject GetParent(this GameObject subject) {
            if (subject.transform.parent != null) {
                return subject.transform.parent.gameObject;
            }
            return null;
        }

        public static GameObject SetParent(this GameObject subject, GameObject parent) {
            subject.transform.SetParent(parent.transform, worldPositionStays: false);
            return subject;
        }

        public static IEnumerable<GameObject> GetChildren(this GameObject subject) {
            for (int i = 0; i < subject.transform.childCount; i++) {
                var child = subject.transform.GetChild(i);
                yield return child.gameObject;
            }
        }

        public static Func<GameObject> PrefabFactory(GameObject prefab) {
            return () => (GameObject) GameObject.Instantiate(prefab);
        } 
    }
}
