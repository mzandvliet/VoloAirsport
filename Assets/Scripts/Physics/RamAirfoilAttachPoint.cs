using UnityEngine;

public class RamAirfoilAttachPoint : MonoBehaviour {
    [SerializeField] private Airfoil1D _airfoil;
    [SerializeField] private Vector3 _linearAxis = Vector3.forward;
    [SerializeField] private float _linearScale = 0.005f;

    private RamAirModifier _ramAirModifier;
    private Vector3 _baseLocalPosition;
    private float _smoothLinearForce;

    void Awake() {
        _ramAirModifier = _airfoil.GetComponent<RamAirModifier>();
        _baseLocalPosition = transform.localPosition;
    }

	void OnEnable () {
        _airfoil.OnPreFixedUpdate += OnPreFixedUpdate;
	}

    void OnDisable() {
        if (_airfoil) {
            _airfoil.OnPreFixedUpdate -= OnPreFixedUpdate;
        }
    }
	
	void OnPreFixedUpdate (IAerodynamicSurface surface) {
	    Vector3 localLinearForce = transform.InverseTransformDirection(_airfoil.LiftForce + _airfoil.DragForce);
	    float linearForce = Vector3.Project(localLinearForce, _linearAxis).magnitude;
	    linearForce = Mathf.Pow(linearForce, 0.5f);
	    linearForce *= Mathf.Sign(Vector3.Dot(localLinearForce, _linearAxis));
        _smoothLinearForce = Mathf.Lerp(_smoothLinearForce, linearForce, 20f * Time.fixedDeltaTime);
	    float linearScale = _linearScale + _linearScale * 1f * (1f - _ramAirModifier.NormalizedDistance);
        transform.localPosition = _baseLocalPosition + _linearAxis * _smoothLinearForce * linearScale;
	}
}
