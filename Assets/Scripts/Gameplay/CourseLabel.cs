using RamjetAnvil.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;

public class CourseLabel : MonoBehaviour {
    [Dependency, SerializeField] private GameObject _ring;
    [Dependency("cameraTransform"), SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _visibilityDistance;
    [SerializeField] private Text _label;
    [SerializeField] private CanvasGroup _textOpacity;

    private Transform _transform;
    private string _courseName;
    private Vector3 _labelHeight;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }

    private static float GetRingMeshHeight(GameObject ringMesh) {
        var renderers = ringMesh.GetComponentsInChildren<Renderer>();
        var totalBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            totalBounds.Encapsulate(renderers[i].bounds);
        }
        return totalBounds.size.y;
    }

    /* Todo: Run update logic from a manager */
    void Update() {
        Vector3 pos = _transform.position;
        Vector3 camPos = _cameraTransform.position;

        var camDist = (camPos - pos).magnitude;
        if (camDist < _visibilityDistance) {
            var opacity = 1f - camDist / _visibilityDistance;
            _textOpacity.alpha = opacity;
            _transform.rotation = Quaternion.LookRotation(pos - camPos, _cameraTransform.up); // Inverted look direction; label has to look away from cam
            _transform.localPosition = _labelHeight;
        }
        else {
            _textOpacity.alpha = 0f;
        }
    }

    public string CourseName {
        set {
            _courseName = value;
            _label.text = _courseName;
        }
    }

    public GameObject Ring {
        set {
            _ring = value;
            _labelHeight = Vector3.up * (_ring.transform.localScale.y + 13f + (5f * (_ring.transform.localScale.y / 4f)));
        }
    }

    public Transform CameraTransform {
        set { _cameraTransform = value; }
    }
}
