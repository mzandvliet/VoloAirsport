using UnityEngine;

public class ControlSurface : MonoBehaviour {
    [SerializeField] private Airfoil _wing;
    [SerializeField] private int[] _affectedSections;
    [SerializeField] private float _minAngle = -10f;
    [SerializeField] private float _maxAngle = 10f;

    private Quaternion _baseRotation;

    private float _input;

    public float Input {
        get { return _input; }
        set { _input = Mathf.Clamp(value, -1f, 1f); }
    }

    private void Awake() {
        _baseRotation = transform.localRotation;
    }

    private void FixedUpdate() {
        float angle = Mathf.Lerp(_minAngle, _maxAngle, (_input + 1f)*0.5f); // For mesh

        for (int i = 0; i < _affectedSections.Length; i++) {
            var section = _wing.SectionStates[_affectedSections[i]];
            section.Offset = angle;
        }

        transform.localRotation = _baseRotation * Quaternion.Euler(-angle, 0f, 0f);
    }
}
