using System;
using UnityEngine;

namespace Assets.Scripts.Ui.GenericComponents {

    public class GLCircle : MonoBehaviour {

        [SerializeField] private float radius = 1f;
        [SerializeField] private LineRenderer _lineRenderer;

        private bool _isInitialized;

        const float StepSize = 0.3f;
        private Vector3[] _circlePoints;

        void Awake() {
            Initialize();
        }

        void Initialize() {
            if (!_isInitialized) {
                _isInitialized = true;

                var steps = Mathf.CeilToInt((2f * Mathf.PI) / StepSize);
                steps += 1; // One extra step to make the last point wrap around to the first one
                _circlePoints = new Vector3[steps];
                _lineRenderer.numPositions = _circlePoints.Length;

                Radius = radius;
            }
        }

        public float Radius {
            set {
                Initialize();

                if (Math.Abs(radius - value) > float.Epsilon) {
                    radius = value;
                    for (int i = 0; i < _circlePoints.Length; i++) {
                        var theta = i * StepSize;
                        var point = new Vector3(
                            x: Mathf.Cos(theta) * radius,
                            z: Mathf.Sin(theta) * radius,
                            y: 0f);
                        _circlePoints[i] = point;
                    }
                    _lineRenderer.SetPositions(_circlePoints);
                }
            }
        }
    }
}
