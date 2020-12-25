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

using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class CenterOfMassVisualizer : MonoBehaviour, IAerodynamicsVisualizer
{
    [SerializeField] private bool _updateContinuously = true;
    [SerializeField] private Transform _root;

    private Rigidbody[] _bodies;
    private Vector3 _centerOfMass = Vector3.zero;
    private float _totalMass;

    public float ForceDrawScale { get; set; }

    public Vector3 CenterOfMass { get { return _centerOfMass; } }
    public float TotalMass { get { return _totalMass; } }

    private void Start()
    {
        Initialize();
        Calculate();
    }

    private void Initialize() {
        _bodies = _root.gameObject.GetComponentsInChildren<Rigidbody>();
    }

    private void Update()
    {
        if (_bodies != null && _updateContinuously)
            Calculate();
    }

    private void Calculate()
    {
		Vector3 centerOfMass = Vector3.zero;
		_totalMass = 0f;

        for (int i = 0; i < _bodies.Length; i++) {
            Rigidbody part = _bodies[i];
            centerOfMass += part.worldCenterOfMass * part.mass;
            _totalMass += part.mass;
        }

        centerOfMass /= _totalMass;
		_centerOfMass = centerOfMass;    }

    private void OnDrawGizmos()
    {
        if (_bodies == null) {
            Initialize();
        }

        if (!Application.isPlaying) {
            Calculate();
        }

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(_centerOfMass, 0.05f);

        //Debug.Log(_totalMass);
    }

#if UNITY_EDITOR
    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    private static void DrawHandle(CenterOfMassVisualizer viz, GizmoType gizmosType) {
        Handles.Label(viz.transform.position, "Mass: " + viz.TotalMass, GUI.skin.box);
    }
#endif
}