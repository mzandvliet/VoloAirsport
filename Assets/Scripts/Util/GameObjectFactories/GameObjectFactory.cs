using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using ObjectFactory = System.Func<UnityEngine.GameObject>;

namespace RamjetAnvil.GameObjectFactories {

    public static class GameObjectFactory {
        public static readonly Lazy<GameObject> EmptyInactivePrefab = new Lazy<GameObject>(() => (GameObject)Resources.Load("EmptyInactivePrefab"));
        public static readonly Lazy<GameObject> EmptyPrefab = new Lazy<GameObject>(() => (GameObject)Resources.Load("EmptyPrefab"));

        public static Lazy<ObjectFactory> EmptyPrefabFactory = new Lazy<ObjectFactory>(() => FromPrefab(EmptyPrefab.Value));
        public static Lazy<ObjectFactory> EmptyInactivePrefabFactory = new Lazy<ObjectFactory>(() => FromPrefab(EmptyInactivePrefab.Value));

        public static ObjectFactory FromPrefab(GameObject prefab, bool turnOff = false) {
            return () => {
                GameObject gameObject;
                if (turnOff) {
                    var prefabState = prefab.activeSelf;
                    prefab.SetActive(false);
                    gameObject = Object.Instantiate(prefab);
                    prefab.SetActive(prefabState);
                } else {
                    gameObject = Object.Instantiate(prefab);
                }
                return gameObject;
            };
        }

        public static ObjectFactory FromPrefab(string resourcePath, bool turnOff = false) {
            return FromPrefab((GameObject)Resources.Load(resourcePath), turnOff);
        }

        public static ObjectFactory Adapt(this ObjectFactory factory, Action<GameObject> adapter) {
            return () => {
                var gameObject = factory();
                adapter(gameObject);
                return gameObject;
            };
        }

        public static ObjectFactory TurnOn(this ObjectFactory factory) {
            return () => {
                var go = factory();
                go.SetActive(true);
                return go;
            };
        }

        public static GameObject Instantiate(this ObjectFactory factory) {
            return factory();
        }

        public static GameObject FindInChildren(this GameObject go, string name, bool includeInactive = true) {
            return (from x in go.GetComponentsInChildren<Transform>(includeInactive)
                    where x.gameObject.name == name
                    select x.gameObject).First();
        }

        public static void ReplaceChild(this GameObject go, string id, GameObject newGo) {
            var oldGo = go.FindInChildren(id).gameObject;
            var parent = oldGo.transform.parent;
            GameObject.Destroy(oldGo);
            newGo.transform.parent = parent;
        }
    }
}
