using UnityEngine;
using System.Collections;

/* This changes the near clipping plane to be as far forward as it can without
 * clipping through any geometry. It does this to make the most of the z-buffering
 * range, limiting z-fighting at large ranges when possible.
 */

public class ClippingPlaneManager : MonoBehaviour {
    [SerializeField] private Camera[] _cameras;
    [SerializeField] private float _minNearClipDistance = 0.01f;
    [SerializeField] private float _maxNearClipDistance = 1f;
    [SerializeField] private string _ignoredLayer = "Head";

    private int _layerMask;
    private bool _lastFrameViewObstructed;

    private void Start() {
        _layerMask = ~LayerMask.GetMask(new[] {_ignoredLayer});
    }

    private void LateUpdate() {
        bool viewObstructed = Physics.CheckSphere(transform.TransformPoint(0f, 0f, 0.0f), 1f, _layerMask);

        if (viewObstructed != _lastFrameViewObstructed) {
            for (int i = 0; i < _cameras.Length; i++) {
                _cameras[i].nearClipPlane = viewObstructed ? _minNearClipDistance : _maxNearClipDistance;
            }
        }
        _lastFrameViewObstructed = viewObstructed;
    }
}
