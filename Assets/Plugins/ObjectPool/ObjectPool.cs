using System;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Util {

    public interface IPrefabPool : IDisposable {
        IPooledGameObject Spawn();
        IPooledGameObject Spawn(Vector3 position, Quaternion rotation);
        void DespawnAll();
    }

    public interface IPooledGameObject : IDisposable {
        GameObject GameObject { get; }
    }

    public class PrefabPool : IPrefabPool {

        private static readonly Lazy<GameObject> PoolHolder = new Lazy<GameObject>(() => new GameObject("__Pools"));

        private readonly GameObject _despawnedContainer;
        private readonly Stack<PooledGameObject> _despawned;
        private readonly IList<PooledGameObject> _spawned;
        private readonly Func<GameObject> _objectFactory;
        private readonly int _growthStep;

        public PrefabPool(string poolName, GameObject prefab, int poolSize, int? growthStep = null)
            : this(poolName, () => Object.Instantiate(prefab), poolSize, growthStep) {}

        public PrefabPool(string poolName, Func<GameObject> objectFactory, int poolSize, int? growthStep = null) {
            if (growthStep.HasValue && growthStep < 0) {
                throw new ArgumentException("Growth step cannot be smaller than 0");
            }

            _growthStep = growthStep ?? Math.Max(1, Mathf.RoundToInt(poolSize / 10f));
            _despawnedContainer = new GameObject(poolName);
            _despawnedContainer.SetParent(PoolHolder.Value);
            _despawned = new Stack<PooledGameObject>(poolSize);
            _spawned = new List<PooledGameObject>(poolSize);
            _objectFactory = objectFactory;

            Grow(poolSize);
        }

        public IPooledGameObject Spawn() {
            return Spawn(position: Vector3.zero, rotation: Quaternion.identity);
        }

        public IPooledGameObject Spawn(Vector3 position, Quaternion rotation) {
            if (_despawned.Count == 0 && _growthStep == 0) {
                throw new OverflowException("Pool reached maximum number of spawned instances");
            }
            if (_despawned.Count <= 0) {
                Grow();
            }

            var pooledObject = _despawned.Pop();
            SpawnInstance(pooledObject, position, rotation);
            _spawned.Add(pooledObject);
            return pooledObject;
        }

        private void Despawn(PooledGameObject pooledObject) {
            var isRemovalSuccess = _spawned.Remove(pooledObject);
            if (isRemovalSuccess) {
                DespawnInstance(pooledObject);
                _despawned.Push(pooledObject);
            } else {
                throw new ArgumentException(string.Format("GameObject '{0}' cannot be despawned in pool.", pooledObject.GameObject));
            }
        }

        public void DespawnAll() {
            for (int i = _spawned.Count - 1; i >= 0; i--) {
                _spawned[i].Dispose();
            }
        }

        private void SpawnInstance(PooledGameObject pooledObject, Vector3 position, Quaternion rotation) {
            var gameObject = pooledObject.GameObject;
            if (gameObject == null) {
                throw new NullReferenceException(
                    string.Format("Pooled instance of type '{0}' is null. Did you Destroy() it by accident?",
                        gameObject.name));
            }

            gameObject.transform.SetParent(null, worldPositionStays: false);
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
            gameObject.SetActive(true);

            InvokeSpawnables(pooledObject);
        }

        private void DespawnInstance(PooledGameObject pooledObject) {
            var gameObject = pooledObject.GameObject;
            if (!gameObject.IsDestroyed()) {
                InvokeDespawnables(pooledObject);
                gameObject.SetActive(false);
                if (!_despawnedContainer.IsDestroyed()) {
                    gameObject.transform.SetParent(null, worldPositionStays: false);
                    gameObject.transform.position = Vector3.zero;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.SetParent(_despawnedContainer);
                }
            }
        }

        private void Grow() {
            Grow(_growthStep);
        }

        private void Grow(int growthSize) {
            for (int i = 0; i < growthSize; i++) {
                var instance = _objectFactory();

                instance.SetActive(false);
                instance.transform.SetParent(null, worldPositionStays: false);
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                instance.SetParent(_despawnedContainer);

                _despawned.Push(new PooledGameObject(Despawn, instance));
            }
        }

        public void Dispose() {
            DespawnAll();
            foreach (var despawnedObject in _despawned) {
                if (!despawnedObject.GameObject.IsDestroyed()) {
                    Object.Destroy(despawnedObject.GameObject);
                }
            }
        }

        private static void InvokeSpawnables(PooledGameObject instance) {
            for (int i = 0; i < instance.Spawnables.Count; i++) {
                instance.Spawnables[i].OnSpawn();
            }
        }

        private static void InvokeDespawnables(PooledGameObject instance) {
            for (int i = 0; i < instance.Spawnables.Count; i++) {
                instance.Spawnables[i].OnDespawn();
            }
        }

        private class PooledGameObject : IPooledGameObject {

            private readonly Action<PooledGameObject> _returnToPool;
            private readonly GameObject _gameObject;
            private readonly IList<ISpawnable> _spawnables;

            public PooledGameObject(Action<PooledGameObject> returnToPool, GameObject gameObject) {
                _returnToPool = returnToPool;
                _gameObject = gameObject;
                _spawnables = _gameObject.GetComponentsInChildren<ISpawnable>();
            }

            public IList<ISpawnable> Spawnables {
                get { return _spawnables; }
            }

            public GameObject GameObject {
                get { return _gameObject; }
            }

            public void Dispose() {
                _returnToPool(this);
            }
        }
    }

}
