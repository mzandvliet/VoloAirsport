using System.Collections.Generic;
using UnityEngine;
using UnityExecutionOrder;

/* Todo:
 * - On/off toggle from options menu
 * - Fix acceleration value having to be scaled with respect to fixed update frequency (wtf?)
 */

[Run.After(typeof(FlightStatistics))]
public class TrajectoryVisualizer : MonoBehaviour {
    [SerializeField]
    private FlightStatistics _statistics;
    [SerializeField]
    private float _stallSpeedHigh = 52f;
    [SerializeField]
    private float _stallSpeedLow = 42f;
    [SerializeField]
    private float _forceDrawScale = 0.01f;
    [SerializeField]
    private Material _lineMaterial;
    [SerializeField]
    private float _lineWidth = 2f;
    [SerializeField]
    private Transform _collisionIndicator;
    [SerializeField]
    private float _lightIntensity = 2f;
    [SerializeField]
    private float _collisionIndicatorScale = 2f;
    [SerializeField]
    private float _collisionIndicatorLogBase = 2f;
    
    public float ForceDrawScale {
        get { return _forceDrawScale; }
        set { _forceDrawScale = value; }
    }

    // private VectorLine _line; // Todo: Replace Vectrocity
    private Renderer _collisionIndicatorRenderer;
    private Light _collisionIndicatorLight;

    private bool _isVisualizationActive;

    void Awake() {
        if (_statistics == null) {
            _statistics = gameObject.GetComponent<FlightStatistics>();
        }

        _collisionIndicatorRenderer = _collisionIndicator.GetComponent<Renderer>();
        _collisionIndicatorLight = _collisionIndicator.GetComponent<Light>();

        _collisionIndicatorRenderer.gameObject.SetActive(false);

        InitializeLine();

        Transform lineParent = AerodynamicsVisualizationManager.GetOrCreateLineParent();
        // _line.rectTransform.SetParent(lineParent);

        _isVisualizationActive = true;
    }

    public void Activate() {
        _isVisualizationActive = true;
        UpdateRenderer();
    }

    public void Deactivate() {
        _isVisualizationActive = false;
        UpdateRenderer();
    }

    private void OnEnable() {
        UpdateRenderer();
    }

    private void OnDisable() {
        UpdateRenderer();
    }

    private void UpdateRenderer() {
        var isVisible = enabled && _isVisualizationActive;

        InitializeLine();
        // _line.active = isVisible;

        if (_collisionIndicatorRenderer == null) {
            _collisionIndicatorRenderer = _collisionIndicator.GetComponent<Renderer>();    
        }
        _collisionIndicatorRenderer.enabled = isVisible;
    }

    private void InitializeLine() {
//         if (_line == null) {
//             var points = new List<Vector3>(_statistics.TrajectorySegments);

//             _line = new VectorLine("TrajectoryVisualizerLine", points, _lineWidth, LineType.Continuous);
//             _line.material = _lineMaterial;
//             _line.layer = 0;
//         }
    }

    private static readonly Color NormalColor = new Color(0f, .7f, 1f, 0.6f);
    private static readonly Color DangerColor = new Color(1f, 0f, 0f, 0.9f);

    void Update() {
        var trajectory = _statistics.PredictionBuffer;
        float speedLerp = Mathf.Clamp01((_statistics.WorldVelocity.magnitude-10f) * 0.1f);

        for (int i = 0; i < trajectory.Length; i++) {
            var data = trajectory[i];

            // Show stall warning for this segment
            float stallFactor = Mathf.InverseLerp(_stallSpeedHigh, _stallSpeedLow, data.Velocity.magnitude);
            Color color = Color.Lerp(NormalColor, DangerColor, stallFactor);
            color.a = Mathf.Min(1f, i / (float)trajectory.Length * 2f);

            // _line.points3[i] = data.Position + new Vector3(0f, -0.5f * (1f - i / (float)trajectory.Length), 0f);
            // if (i < trajectory.Length - 1) {
            //     _line.SetColor(color, i);
            // }
        }

        if (_statistics.CollisionPredicted) {
            int collisionLineIndex = Mathf.RoundToInt(_statistics.NormalizedCollisionIndex * trajectory.Length);
            HideUnusedLineSegments(collisionLineIndex);

            _collisionIndicatorRenderer.gameObject.SetActive(true);
            float colorLerp = Mathf.Pow((1f - _statistics.NormalizedCollisionIndex), 1.5f);
            Color color = Color.Lerp(NormalColor, DangerColor, colorLerp);
            color.a = speedLerp;
            _collisionIndicatorRenderer.material.SetColor("_Color", color);
            _collisionIndicatorLight.intensity = _lightIntensity * speedLerp;
        } else {
            _collisionIndicatorRenderer.gameObject.SetActive(false);
            _collisionIndicatorLight.intensity = 0f;
        }

        // _line.Draw3D();
    }

    void HideUnusedLineSegments(int startIndex) {
        // for (int i = startIndex; i < _line.points3.Count -1; i++) {
        //     _line.SetColor(Color.clear, i);
        // }
    }

    void LateUpdate() {
        _collisionIndicator.position = _statistics.CollisionPoint + _statistics.CollisionNormal;
        _collisionIndicator.localScale = Vector3.one * (_collisionIndicatorScale * Mathf.Log(1f + Vector3.Distance(transform.position, _collisionIndicator.position), _collisionIndicatorLogBase));
    }
}
