using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class RotationTester : MonoBehaviour
{
    private Quaternion _baseRotation;

    private Vector3 _proposedRotation;

    void Awake()
    {
        _baseRotation = transform.localRotation;

        //transform.localRotation = transform.localRotation * Quaternion.Euler(30f, 0f, 0f);
    }

	private void Update()
	{
        // Elliptical constraint

	    //Quaternion jointRotation = Quaternion.Inverse(_baseRotation) * transform.localRotation;

        //Vector3 proposedForward = jointRotation * Vector3.forward;

        //Vector3 swingAxis = Vector3.Cross(Vector3.forward, proposedForward).normalized;
        //Debug.DrawRay(transform.position, swingAxis * 2f, Color.red);
        //float angle = MathUtils.AngleAroundAxis(Vector3.forward, proposedForward, swingAxis);

        //Debug.Log(angle);

        //angle = Mathf.Clamp(angle, -45f, 45f);

        //transform.localRotation = _baseRotation * Quaternion.AngleAxis(angle, swingAxis);

        
        // Euler Decomposition

        Quaternion jointRotation = Quaternion.Inverse(_baseRotation) * transform.localRotation;

        float angleX = jointRotation.eulerAngles.x;
        float angleY = jointRotation.eulerAngles.y;
        float angleZ = jointRotation.eulerAngles.z;

        angleX = WrapAngle(angleX);
        angleY = WrapAngle(angleY);
        angleZ = WrapAngle(angleZ);

        angleX = Mathf.Clamp(angleX, -20f, 20f);
        angleY = Mathf.Clamp(angleY, -20f, 20f);
        angleZ = Mathf.Clamp(angleZ, -20f, 20f);

        transform.localRotation = _baseRotation * Quaternion.Euler(angleX, angleY, angleZ);
	}

    private float WrapAngle(float angle)
    {
        if (angle < -180)
            return angle + 360f; // Todo float modulus for negative numbers
        if (angle > 180)
            return angle - 360f;
        return angle;
    }
}
