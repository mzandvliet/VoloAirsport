using UnityEngine;

[ExecuteInEditMode]
public class IgnoreParentTransform : MonoBehaviour {
    private Transform _transform;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }

    private void Update() {
        _transform.rotation = Quaternion.identity;
    }

    private void LateUpdate() {
        _transform.rotation = Quaternion.identity;
    }
}
