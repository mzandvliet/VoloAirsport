using System;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletManager : MonoBehaviour {
    [SerializeField] private Bullet _bulletPrefab;

    private UnityPool<Bullet> _bulletPool;
    private List<Bullet> _activeBullets;

    private void Awake() {
        _bulletPool = new UnityPool<Bullet>("Bullets", 512, _bulletPrefab, null);
        _activeBullets = new List<Bullet>(512);
    }

    private void Update() {
        for (int i = _activeBullets.Count - 1; i >= 0; i--) {
            var b = _activeBullets[i];
            if (b.StartTime < Time.time - 5f) {
                Recycle(b);
            }
        }
    }

    private void Recycle(Bullet b) {
        _activeBullets.Remove(b);
        b.OnCollision -= OnBulletCollision;
        _bulletPool.Despawn(b);
    }

    public void Fire(Vector3 position, Quaternion rotation, Vector3 force) {
        var bullet = _bulletPool.Spawn(position, rotation);
        bullet.OnCollision += OnBulletCollision;
        bullet.StartTime = Time.time;
        _activeBullets.Add(bullet);

        Rigidbody body = bullet.GetComponent<Rigidbody>();
        body.AddForce(force, ForceMode.Impulse);
    }

    private void OnBulletCollision(Bullet b, Collision c) {
        // Todo: play some effect
        Recycle(b);
    }

    public class UnityPool<T> where T : Component, ISpawnable {
        private readonly Queue<T> _items;
        private readonly T _prefab;
        private readonly Transform _root;

        public UnityPool(string name, int capacity, T prefab, Action<T> recycle) {
            _items = new Queue<T>(capacity);
            _prefab = prefab;
            _root = new GameObject(name).transform;

            for (int i = 0; i < capacity; i++) {
                T item = Instantiate(_prefab);
                Despawn(item);
            }
        }

        public T Spawn(Vector3 position, Quaternion rotation) {
            if (_items.Count == 0) {
                throw new Exception("Pool is empty");
            }

            var item = _items.Dequeue();
            item.transform.position = position;
            item.transform.rotation = rotation;
            item.gameObject.SetActive(true);
            item.OnSpawn();
            item.transform.parent = null;

            return item;
        }

        public void Despawn(T item) {
            item.OnDespawn();
            item.gameObject.SetActive(false);
            _items.Enqueue(item);
            item.transform.parent = _root;
        }
    }
}
