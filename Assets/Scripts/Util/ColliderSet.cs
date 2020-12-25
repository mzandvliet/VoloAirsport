using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Util {

    public static class ColliderSet {

        public delegate void CopyCollider<TCollider>(TCollider from, TCollider to) where TCollider : Collider;

        public static readonly CopyCollider<BoxCollider> CopyBoxCollider = (from, to) => {
            to.center = from.center;
            to.size = from.size;
        };
        public static readonly CopyCollider<SphereCollider> CopySphereCollider = (from, to) => {
            to.center = from.center;
            to.radius = from.radius;
        };

        public static Set<BoxCollider> Box(string name, int layer) {
            return new Set<BoxCollider>(name, layer, CopyBoxCollider);
        }

        public class Set<TCollider> where TCollider : Collider {

            private readonly CopyCollider<TCollider> _copyCollider;
            private readonly IObjectPool<TCollider> _colliderPool;
            private readonly GameObject _parent;

            private readonly IList<IPooledObject<TCollider>> _activeColliders;

            public Set(string name, int layer, CopyCollider<TCollider> copyCollider) {
                _activeColliders = new List<IPooledObject<TCollider>>();
                _copyCollider = copyCollider;
                _parent = new GameObject(name);
                _parent.layer = layer;
                _colliderPool = new ObjectPool<TCollider>(() => {
                    var collider = new GameObject("Collider").AddComponent<TCollider>();
                    collider.gameObject.layer = layer;
                    collider.transform.SetParent(_parent.transform);
                    return new ManagedObject<TCollider>(collider, onReturnedToPool: () => collider.gameObject.SetActive(false));
                });
            }

            public GameObject Parent {
                get { return _parent; }
            }

            public void SetSize(int size) {
                var additionalColliderCount = size - _activeColliders.Count;

                for (int i = 0; i < additionalColliderCount; i++) {
                    var collider = _colliderPool.Take();
                    _activeColliders.Add(collider);
                }
                
                for (int i = additionalColliderCount; i < 0; i++) {
                    var lastIndex = _activeColliders.Count - 1;
                    var collider = _activeColliders[lastIndex];
                    collider.Dispose();
                    _activeColliders.RemoveAt(lastIndex);
                }
            }

            public void UpdateCollider(int index, TCollider collider) {
                var colliderCopy = _activeColliders[index];
                _copyCollider(collider, colliderCopy.Instance);
                colliderCopy.Instance.transform.position = collider.transform.position;
                colliderCopy.Instance.transform.rotation = collider.transform.rotation;
                colliderCopy.Instance.transform.localScale = collider.transform.localScale;
                colliderCopy.Instance.gameObject.SetActive(true);
            }
        }
        
    }
}
