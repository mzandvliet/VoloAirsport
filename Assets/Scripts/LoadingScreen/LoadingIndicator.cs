using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class LoadingIndicator : MonoBehaviour {
    [SerializeField] private float _rotationSpeed = 1f;
    [Dependency, SerializeField] private AbstractUnityClock _clock;

    void Update() {
        var time = (float)Mathd.PingPong(_clock.CurrentTime, _rotationSpeed) / _rotationSpeed;
        transform.localRotation = Quaternion.Euler(new Vector3(0, Mathf.Lerp(-90f, 90f, time), 0));
    }
}
