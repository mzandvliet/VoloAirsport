using UnityEngine;

[ExecuteInEditMode]
public class UIWorldSpaceCanvas : MonoBehaviour {
    [SerializeField] private Vector2 _localScale = Vector3.one;
    [SerializeField] private float _pixelsDensity = 100f;

    private RectTransform _rect;

	void Start () {
	    _rect = GetComponent<RectTransform>();
	}
	
	void Update () {
	    Apply();
	}

    private void Apply() {
        _rect.sizeDelta = new Vector2(_pixelsDensity * _localScale.x, _pixelsDensity * _localScale.y);
        _rect.localScale = new Vector3(1f / _pixelsDensity, 1f / _pixelsDensity, 1f / _pixelsDensity);
    }

    private Vector3 Inverse(Vector3 vector) {
        return new Vector3(
            1f / vector.x,
            1f / vector.y,
            1f / vector.x);
    }
}
