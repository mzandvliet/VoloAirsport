using RamjetAnvil.Unity.Aero;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

/* Todo:
 * 
 * Calculate force distributation ratios automatically
 * 
 * Would an even distrution over the connected masses be ok?
 */

public class AirfoilForceApplicator : MonoBehaviour {
    [SerializeField] private Airfoil _airfoil;
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
            for (int j = 0; j < _airfoil.SectionStates.Length; j++) {
                var section = _airfoil.SectionStates[j];
                var position = _airfoil.transform.TransformPoint(section.LocalPosition);
                var force = (section.Lift + section.Drag) * affectedBody.Share;
                affectedBody.Body.AddForceAtPosition(force, position);

                // Todo: Moment Torque
            } 
	    }
	}

    private void OnDrawGizmos() {
        if (!Application.isPlaying)
            return;

        const float scale = PhysicsUtils.DebugForceScale;

        float mass = CalculateTotalMass();

        for (int i = 0; i < _airfoil.SectionStates.Length; i++) {
            SectionState sectionState = _airfoil.SectionStates[i];
            Vector3 position = transform.TransformPoint(sectionState.LocalPosition);

            //Gizmos.color = Color.white;
            //Gizmos.DrawSphere(position, 0.01f);
            //Gizmos.DrawRay(position, sectionState.RelativeVelocity / 10f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(position, sectionState.Lift / mass * scale);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(position, sectionState.Drag / mass * scale);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(position, sectionState.Moment / mass * scale);
        }
    }

    private float CalculateTotalMass() {
        float total = 0f;
        for (int i = 0; i < _affectedBodies.Length; i++) {
            total += _affectedBodies[i].Body.mass;
        }
        return total;
    }

    [System.Serializable]
    public class AffectedBody {
        public Rigidbody Body;
        public float Share;
    }
}
