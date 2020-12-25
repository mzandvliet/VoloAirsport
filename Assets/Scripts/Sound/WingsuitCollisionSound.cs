using System;
using FMOD.Studio;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Fmod = FMODUnity.RuntimeManager;

public class WingsuitCollisionSound : MonoBehaviour {
    [SerializeField] private CollisionEventSource _collisionSource;
//    [SerializeField] private string _defaultSound;
//    [SerializeField] private TaggedSoundDictionary _taggedSounds;
    [SerializeField] private float _minImpactVelocity = 1f;
    [SerializeField] private float _maxImpactVelocity = 10f;

    private EventInstance _collisionSoundEvent;

    void Start() {
        _collisionSource.OnCollisionEntered += OnCollisionEntered;
        _collisionSoundEvent = Fmod.CreateInstance("event:/wingsuit/Collisions/ground");
    }

    private void OnCollisionEntered(CollisionEventSource collisionEventSource, Collision collision) {
        Vector3 impactVelocity = collision.relativeVelocity -
                                 Vector3.Project(collision.relativeVelocity, collision.contacts[0].normal);
        float relativeSpeed = impactVelocity.magnitude;

        if (relativeSpeed < _minImpactVelocity) {
            return;
        }

//        var asset = _defaultSound;
//        if (_taggedSounds.ContainsKey(collision.collider.tag)) {
//            asset = _taggedSounds[collision.collider.tag];
//        }

        float force = Mathf.Clamp01(relativeSpeed / _maxImpactVelocity);

        _collisionSoundEvent.setParameterValue("force", force);
        _collisionSoundEvent.start();
        //Fmod.PlayOneShot(asset, collision.contacts[0].point);
    }

    [Serializable]
    public class TaggedSoundDictionary : SerializableDictionary<string, string> {}
}
