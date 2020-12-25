using UnityEngine;

public class HudElement : MonoBehaviour {
    [SerializeField] private Vector3 _viewportPoint = new Vector3(0.5f, 0.5f, 4f);
    [SerializeField] private Vector3 _scale = Vector3.one;
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;
    [SerializeField] private bool _adjustToFieldOfView = false;

    public Vector3 ViewportPoint {
        get { return _viewportPoint; }
        set { _viewportPoint = value; }
    }

    public Vector3 Scale {
        get { return _scale; }
        set { _scale = value; }
    }

    public Vector3 RotationOffset {
        get { return _rotationOffset; }
        set { _rotationOffset = value; }
    }

    public bool AdjustToFieldOfView {
        get { return _adjustToFieldOfView; }
        set { _adjustToFieldOfView = value; }
    }
}
