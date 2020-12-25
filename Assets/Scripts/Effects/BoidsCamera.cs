using UnityEngine;
using System.Collections;

public class BoidsCamera : MonoBehaviour {
    [SerializeField] private Boids _boids;

    private Transform _transform;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }

    void Update() {
        var targetPosition = _boids.AveragePosition;
        _transform.LookAt(targetPosition);
    }
}
