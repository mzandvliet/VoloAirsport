using System;
using System.Collections.Generic;
using RamjetAnvil.Volo;
using UnityEngine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Util;
using UnityEngine.Rendering;

/// <summary>
/// Convenient top-Level component used to manage all child visualizers in a gameobject hierarchy.
/// </summary>
public class AerodynamicsVisualizationManager : MonoBehaviour {
    [SerializeField] private float _forceDrawScale = 0.01f;
    [SerializeField] private float _lineWidth = 0.015f;
    private Material _lineMaterial;

    private IList<IAerodynamicSurface> _surfaces;
    private LineSet[] _liftLines;
    private LineSet[] _dragLines;

    public float ForceDrawScale {
        get { return _forceDrawScale; }
        set { _forceDrawScale = value; }
    }

    private void OnEnable() {
        for (int i = 0; i < _surfaces.Count; i++) {
            _liftLines[i].Renderer.gameObject.SetActive(true);
            _dragLines[i].Renderer.gameObject.SetActive(true);
        }
    }

    private void OnDisable() {
        for (int i = 0; i < _surfaces.Count; i++) {
            if (_liftLines[i].Renderer != null) {
                _liftLines[i].Renderer.gameObject.SetActive(false);    
            }
            if (_dragLines[i].Renderer != null) {
                _dragLines[i].Renderer.gameObject.SetActive(false);    
            }
        }
    }

    void Awake () {
        _lineMaterial = Resources.Load<Material>("AerodynamicsVisualizerMaterial");
	    _surfaces = gameObject.GetComponentsOfInterfaceInChildren<IAerodynamicSurface>();
        _liftLines = new LineSet[_surfaces.Count];
        _dragLines = new LineSet[_surfaces.Count];

        Transform lineParent = GetOrCreateLineParent();

        for (int i = 0; i < _surfaces.Count; i++) {
            _liftLines[i] = CreateLineSet("Lift line " + i, Color.green, lineParent);
            _dragLines[i] = CreateLineSet("Drag line " + i, Color.red, lineParent);
        }

        ForceDrawScale = 0.01f;
    }

    LineSet CreateLineSet(string lineName, Color color, Transform lineParent) {
        var lineRenderer = new GameObject(lineName).AddComponent<LineRenderer>();
        lineRenderer.gameObject.transform.SetParent(lineParent);
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;
        lineRenderer.material = _lineMaterial;
        return new LineSet(lineRenderer, new Vector3[2]);
    }

    private void LateUpdate() {
        for (int i = 0; i < _surfaces.Count; i++) {
            var surface = _surfaces[i];

            Vector3 position = surface.transform.TransformPoint(surface.Center);

            var liftLine = _liftLines[i];
            liftLine.Points[0] = position;
            liftLine.Points[1] = position + surface.LiftForce * _forceDrawScale;
            liftLine.Renderer.SetPositions(liftLine.Points);

            var dragLine = _dragLines[i];
            dragLine.Points[0] = position;
            dragLine.Points[1] = position + surface.DragForce * _forceDrawScale;
            dragLine.Renderer.SetPositions(dragLine.Points);
        }
    }
    
    private static Transform _lineParent;
    public static Transform GetOrCreateLineParent() {
        if (_lineParent == null) {
            _lineParent = new GameObject("_LineParent").transform;
        }
        return _lineParent;
    }
}
