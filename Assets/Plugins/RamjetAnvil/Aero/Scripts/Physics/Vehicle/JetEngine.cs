using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class JetEngine : MonoBehaviour
{
    [SerializeField] Rigidbody _body;
    [SerializeField] float _maxThrust = 1f;
    [SerializeField] private float _vectoringAngle = 20f;

    float _thrustInput;
    private Vector2 _vectoringInput;
    float _currentThrust;

    public Vector2 VectoringInput {
        get { return _vectoringInput; }
        set { _vectoringInput = value; }
    }

    public float ThrustInput {
        get { return _thrustInput; }
        set { _thrustInput = Mathf.Clamp01(value); }
    }

    public float CurrentThrust
	{
		get { return _currentThrust; }	
	}

    void FixedUpdate() {
        _currentThrust = _thrustInput*_maxThrust;

        transform.localRotation = Quaternion.Euler(_vectoringInput.x * _vectoringAngle, VectoringInput.y * _vectoringAngle, 0f);

        // Apply force to connected body
        Vector3 thrustForce = transform.TransformDirection(Vector3.forward * _currentThrust);
        _body.AddForceAtPosition(thrustForce, transform.position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        if (!_body) {
            return;
        }

		Gizmos.color = Color.green;
		Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * _currentThrust / _body.mass * PhysicsUtils.DebugForceScale);
    }
}