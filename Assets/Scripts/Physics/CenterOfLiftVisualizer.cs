/**
 * Created by Martijn Zandvliet, 2014
 * 
 * Use of the source code in this document is governed by the terms
 * and conditions described in the Ramjet Anvil End-User License Agreement.
 * 
 * A copy of the Ramjet Anvil EULA should have been provided in the purchase
 * of the license to this source code. If you do not have a copy, please
 * contact me.
 * 
 * For any inquiries, contact: martijn@ramjetanvil.com
 */

using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class CenterOfLiftVisualizer : MonoBehaviour, IAerodynamicsVisualizer {
    [SerializeField] private bool _updateContinuously = true;
    [SerializeField] private Transform _root;
    [SerializeField] private CenterOfMassVisualizer _comVisualizer;
    [SerializeField] private Material _lineMaterial;

    public float ForceDrawScale { get; set; }

    private IList<IAerodynamicSurface> _surfaces;
    private Vector3 _centerOfLift = Vector3.zero;
    private Vector3 _centerOfDrag = Vector3.zero;

    public Vector3 CenterOfLift { get { return _centerOfLift; } }

    // private VectorLine _line; // Todo: Replace Vectrocity
    // private VectorLine _points;

    private void Awake() {
        _surfaces = _root.gameObject.GetComponentsOfInterfaceInChildren<IAerodynamicSurface>();

        // var lines = new List<Vector3>();
        // var colors = new List<Color32> { Color.green, Color.red };
        // _line = new VectorLine("CenterOfLiftVisualizerLine", lines, 1f, LineType.Discrete);
        // _line.SetColors(colors);

        // var points = new List<Vector3>();
        // const float pointSize = 10f;
        // _points = new VectorLine("CenterOfLiftVisualizerPoints", points, pointSize, LineType.Points);
        // _points.SetColors(colors);

        // Transform lineParent = AerodynamicsVisualizationManager.GetOrCreateLineParent();
        // _line.rectTransform.SetParent(lineParent);
        // _points.rectTransform.SetParent(lineParent);
    }

    private void OnEnable() {
        // _line.active = true;
        // _points.active = true;

        Debug.Log("Todo: replace Vectrocity library");
    }

    private void OnDisable() {
        // _line.active = false;
        // _points.active = false;
    }

    private void LateUpdate() {
        if (_surfaces != null && _updateContinuously)
            Calculate();

        if (enabled)
            UpdateLine();
    }

    private void Calculate() {
		Vector3 centerOfLift = Vector3.zero;
        Vector3 centerOfDrag = Vector3.zero;
        float liftScale = 0f;
        float dragScale = 0f;

        for (int i = 0; i < _surfaces.Count; i++) {
            var part = _surfaces[i];
            var partCenter = part.transform.TransformPoint(part.Center);
            var lift = part.LiftForce.magnitude;
            var drag = part.DragForce.magnitude;
            centerOfLift += partCenter * lift;
            centerOfDrag += partCenter * drag;
            liftScale += lift;
            dragScale += drag;
        }

        // Todo: approaches infinity when speed gets low, of course

        centerOfLift /= liftScale;
        centerOfDrag /= dragScale;

		_centerOfLift = centerOfLift;
        _centerOfDrag = centerOfDrag;
    }

    private void UpdateLine() {
        // _line.points3[0] = _comVisualizer.CenterOfMass;
        // _line.points3[1] = _centerOfLift;
        // _line.points3[2] = _comVisualizer.CenterOfMass;
        // _line.points3[3] = _centerOfDrag;
        // _line.Draw3D();

        // _points.points3[0] = _centerOfLift;
        // _points.points3[1] = _centerOfDrag;
        // _points.Draw3D();
    }
}