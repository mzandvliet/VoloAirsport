using System;
using System.Collections.Generic;
using FMODUnity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;

public class Accelerator : MonoBehaviour {

    [SerializeField] private float _velocityChange = 30f;

    void Awake() {

    }

  
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Vector3 velocity = transform.up * _velocityChange;

            var wingsuit = other.GetComponent<PlayerIdentifier>();
            if (!wingsuit) {
                Debug.LogWarning("Oops: " + other.name);
                return;
            }
            for (int i = 0; i < wingsuit.Root.Rigidbodies.Count; i++) {
                wingsuit.Root.Rigidbodies[i].AddForce(velocity, ForceMode.VelocityChange);
            }
        }
    }
}
