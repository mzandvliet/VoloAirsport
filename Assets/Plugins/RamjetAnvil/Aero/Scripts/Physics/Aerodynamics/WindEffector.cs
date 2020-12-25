using UnityEngine;
using UnityNoise;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Todo: Change trail emission count, size, length and radius to match flow regime.

public class WindEffector : MonoBehaviour {
    [SerializeField] private float _windSpeed = 20f;
    [SerializeField] private float _rotationalWindSpeed = 20f;
    [SerializeField] private float _outerRadius = 500f;
    [SerializeField] private float _innerRadius = 250f;

    [SerializeField]
    private WindManager _manager;

    private ParticleSystem _particleSystem;
    private Transform _transform;
    private Vector3 _position;
    private Vector3 _forward;

    public WindManager Manager {
        get { return _manager; }
        set {
            _manager = value;
            OnEnable();
        }
    }

    public float WindSpeed {
        get { return _windSpeed; }
        set { _windSpeed = value; }
    }

    public float RotationalWindSpeed {
        get { return _rotationalWindSpeed; }
    }

    public float OuterRadius {
        get { return _outerRadius; }
        set { _outerRadius = value; }
    }

    public float InnerRadius {
        get { return _innerRadius; }
        set { _innerRadius = value; }
    }

    public Bounds Bounds {
        get { return new Bounds(_transform.position, Vector3.one * (_outerRadius*2f));}
    }

    // Cached transform values, because otherwise 1000s of transform.blah calculations per frame

    public Vector3 Position {
        get { return _position; }
    }

    public Vector3 Forward {
        get { return _forward; }
    }

    void Awake() {
        _transform = gameObject.GetComponent<Transform>();

        _particleSystem = GetComponentInChildren<ParticleSystem>();
        ParticleSystem.ShapeModule shape = _particleSystem.shape;
        shape.radius = _outerRadius;
    }

    private void OnEnable() {
        if (_manager) {
            _manager.AddEffector(this);
        }
    }

    private void OnDisable() {
        if (_manager) {
            _manager.RemoveEffector(this);
        }
    }

    private void Update() {
        _position = _transform.position;
        _forward = _transform.forward;
    }

    /* Todo: Easier selection of windzones by picking, preview wind in editor */
    private static readonly Color GizmoColor = new Color(0f, 0.2f, 0.9f, 0.4f);
    private void OnDrawGizmos() {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, OuterRadius);
    }

#if UNITY_EDITOR
    [DrawGizmo(GizmoType.Pickable)]
    private static void DrawHandle(WindEffector effector, GizmoType gizmosType) {
        Transform transform = effector.transform;

        Handles.color = Color.blue;
        effector.InnerRadius = Handles.RadiusHandle(Quaternion.identity, transform.position, effector.InnerRadius);
        Handles.color = Color.cyan;
        effector.OuterRadius = Handles.RadiusHandle(Quaternion.identity, transform.position, effector.OuterRadius);
        Handles.color = Color.red;
        effector.WindSpeed = Handles.ScaleSlider(effector.WindSpeed, transform.position, transform.forward, transform.rotation, effector.WindSpeed, 0.1f);
        
        if (GUI.changed) {
            EditorUtility.SetDirty(effector);
        }
        //Handles.DrawDottedLine(transform.position, transform.position, Vector3.forward * wind.InnerRadius, );
    }
#endif
}
