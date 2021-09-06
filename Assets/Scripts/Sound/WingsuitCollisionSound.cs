using System;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class WingsuitCollisionSound : MonoBehaviour {
    [SerializeField] private CollisionEventSource _collisionSource;
    [SerializeField] private float _minImpactVelocity = 1f;
    [SerializeField] private float _maxImpactVelocity = 10f;

    void Start() {
        _collisionSource.OnCollisionEntered += OnCollisionEntered;
    }

    private void OnCollisionEntered(CollisionEventSource collisionEventSource, Collision collision) {
        Vector3 impactVelocity = collision.relativeVelocity -
                                 Vector3.Project(collision.relativeVelocity, collision.contacts[0].normal);
        float relativeSpeed = impactVelocity.magnitude;

        if (relativeSpeed < _minImpactVelocity) {
            return;
        }

        float force = Mathf.Clamp01(relativeSpeed / _maxImpactVelocity);
    }

    [Serializable]
    public class TaggedSoundDictionary : SerializableDictionary<string, string> {}
}
