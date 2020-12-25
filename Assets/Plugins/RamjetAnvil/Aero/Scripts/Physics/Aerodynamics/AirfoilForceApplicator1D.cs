using UnityEngine;

/* Todo:
 * 
 * Calculate force distributation ratios automatically
 * 
 * Would an even distrution over the connected masses be ok?
 */

public class AirfoilForceApplicator1D : MonoBehaviour {
    [SerializeField] private Airfoil1D _airfoil;
    [SerializeField] private AffectedBody[] _affectedBodies;

    private void OnEnable() {
        _airfoil.OnPostFixedUpdate += ApplyForce;
    }

    private void OnDisable() {
        if (_airfoil) {
            _airfoil.OnPostFixedUpdate -= ApplyForce;
        }
    }

	private void ApplyForce(IAerodynamicSurface surface) {
	    for (int i = 0; i < _affectedBodies.Length; i++) {
	        AffectedBody affectedBody = _affectedBodies[i];
            var position = _airfoil.transform.TransformPoint(_airfoil.Center);
            var linearForce = (_airfoil.LiftForce + _airfoil.DragForce) * affectedBody.Share;
            affectedBody.Body.AddForceAtPosition(linearForce, position);

            // Todo: Moment torque
	    }
	}

    [System.Serializable]
    public class AffectedBody {
        public Rigidbody Body;
        public float Share;
    }
}
