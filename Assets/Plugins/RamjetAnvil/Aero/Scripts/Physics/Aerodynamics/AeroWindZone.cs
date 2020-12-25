using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AeroWindZone : MonoBehaviour {
    [SerializeField] private float _windSpeed = 20f;
    [SerializeField] private float _outerRadius = 500f;
    [SerializeField] private float _innerRadius = 250f;
    public float WindSpeed {
        get { return _windSpeed; }
        set { _windSpeed = value; }
    }

    public float OuterRadius {
        get { return _outerRadius; }
        set { _outerRadius = value; }
    }

    public float InnerRadius {
        get { return _innerRadius; }
        set { _innerRadius = value; }
    }

    public Vector3 Position
    {
        get { return _position; }
    }

    public Vector3 Forward
    {
        get { return _forward; }
    }

    private Vector3 _position;
    private Vector3 _forward;

    void Update() {
        // Bug: if we have moving windzones, this could be framelagging
        _position = transform.position;
        _forward = transform.forward;
    }

#if UNITY_EDITOR
    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    private static void DrawHandle(AeroWindZone zone, GizmoType gizmosType) {
        Transform transform = zone.transform;

        Handles.color = Color.blue;
        zone.InnerRadius = Handles.RadiusHandle(Quaternion.identity, transform.position, zone.InnerRadius);
        Handles.color = Color.cyan;
        zone.OuterRadius = Handles.RadiusHandle(Quaternion.identity, transform.position, zone.OuterRadius);
        Handles.color = Color.red;
        zone.WindSpeed = Handles.ScaleSlider(zone.WindSpeed, transform.position, transform.forward, transform.rotation, zone.WindSpeed, 0.1f);
        
        if (GUI.changed) {
            EditorUtility.SetDirty(zone);
        }
        //Handles.DrawDottedLine(transform.position, transform.position, Vector3.forward * wind.InnerRadius, );
    }
#endif
}
