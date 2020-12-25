using System;
using System.Collections.Generic;
using FMODUnity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo;
using UnityEngine;

public class Ring : MonoBehaviour, ISpawnable {

    [SerializeField] private Collider _ringMeshCollider;

    public event Action<PlayerIdentifier> OnPlayerContact;

    private IList<Renderer> _renderers;
    private IList<RingAnimator> _animators;
    private IList<Collider> _colliders;
    private IList<StudioEventEmitter> _audioSources;
    private IList<Light> _lights;

    void Awake() {
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        _animators = GetComponentsInChildren<RingAnimator>(includeInactive: true);
        _colliders = GetComponentsInChildren<Collider>(includeInactive: true);
        _audioSources = GetComponentsInChildren<StudioEventEmitter>(includeInactive: true);
        _lights = GetComponentsInChildren<Light>(includeInactive: true);
    }

    public void SetEnabled(bool isRingEnabled) {
        // TODO Maybe we can make rings that are not part of an active course a bit more transparent
        const bool isCollisionOn = false;

        for (int i = 0; i < _renderers.Count; i++) {
            _renderers[i].enabled = isRingEnabled;
        }
        for (int i = 0; i < _animators.Count; i++) {
            _animators[i].enabled = isRingEnabled;
        }
        for (int i = 0; i < _colliders.Count; i++) {
            _colliders[i].enabled = isRingEnabled;
        }
        _ringMeshCollider.enabled = isRingEnabled && isCollisionOn;
        for (int i = 0; i < _audioSources.Count; i++) {
            if (!isRingEnabled) {
                _audioSources[i].Stop();
            }
            _audioSources[i].enabled = isRingEnabled;
            if (isRingEnabled) {
                _audioSources[i].Play();
            }
        }
        for (int i = 0; i < _lights.Count; i++) {
            _lights[i].enabled = isRingEnabled;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (OnPlayerContact != null) {
                OnPlayerContact(other.GetComponent<PlayerIdentifier>());
            }
        }
    }

    public void OnSpawn() {}

    public void OnDespawn() {
        OnPlayerContact = null;
    }
}
