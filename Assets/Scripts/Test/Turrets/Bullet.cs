using System;
using FMOD;
using FMODUnity;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Todo: collision detection, cleanup

public class Bullet : MonoBehaviour, ISpawnable {
    [SerializeField]
    private StudioEventEmitter _explosionSound;
    [SerializeField]
    private StudioEventEmitter _wooshSound;

    public float StartTime;
    public event Action<Bullet, Collision> OnCollision;

    public void OnSpawn() {
        _wooshSound.Play();
    }

    public void OnDespawn() {
        _wooshSound.Stop();
    }

    private void OnCollisionEnter(Collision collision) {
        if (OnCollision != null) {
            OnCollision(this, collision);
        }
    }
}