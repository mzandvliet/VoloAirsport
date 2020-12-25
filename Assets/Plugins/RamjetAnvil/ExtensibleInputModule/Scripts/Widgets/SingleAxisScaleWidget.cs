using System;
using RamjetAnvil.InputModule;
using UnityEngine;

public class SingleAxisScaleWidget : MonoBehaviour {

    [SerializeField] private float _dragSpeed = 0.5f;
    [SerializeField] private float _lengthMultiplier = 0.6f;

    [SerializeField] private SurfaceDragHandler _dragHandler;
    [SerializeField] private Transform _widgetTransform;
    [SerializeField] private Transform _stok;
    [SerializeField] private Transform _box;

    public event Action<float> OnSizeChanged;

	void Awake() {
		_dragHandler.Dragging += DragHandlerOnDragging;
	}

    public void SetSize(float length) {
        length *= _lengthMultiplier;

        var stokScale = _stok.localScale;
        stokScale.y = length;
        _stok.localScale = stokScale;

        var stokPosition = _stok.localPosition;
        stokPosition.z = length;
        _stok.localPosition = stokPosition;

        var boxPosition = _box.localPosition;
        boxPosition.z = length * 2;
        _box.localPosition = boxPosition;
    }

    private void DragHandlerOnDragging(Transform camTransform, Vector3 diff) {
        diff *= _dragSpeed;

        var forward = camTransform.InverseTransformDirection(_widgetTransform.forward);
        var projectedDiff = Vector3.Project(-diff, forward);
        projectedDiff = camTransform.TransformDirection(projectedDiff);

        var localDisplacement = Quaternion.Inverse(_widgetTransform.rotation) * projectedDiff;
        var newScale = (_stok.localScale.y / _lengthMultiplier) + localDisplacement.z;

        if (OnSizeChanged != null) {
            OnSizeChanged(newScale);
        }
    }

    public float DragSpeed {
        get { return _dragSpeed; }
        set { _dragSpeed = value; }
    }
}
