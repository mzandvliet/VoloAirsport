using RamjetAnvil.Unity.Utility;
using UnityEngine;

/* http://answers.unity3d.com/questions/278147/how-to-use-target-rotation-on-a-configurable-joint.html
 * https://gist.github.com/mstevenson/4958837
 * http://forum.unity3d.com/threads/quaternion-wizardry-for-configurablejoint.8919/
 * https://stackoverflow.com/questions/22157435/difference-between-the-two-quaternions 
 */

public class JointTest : MonoBehaviour {
    [SerializeField] private ConfigurableJoint _joint;
    [SerializeField] private float _dampeningFactor = 1f;

    private Transform _transform;
    private Rigidbody _jointBody;

    private Quaternion _jointBaseLocalRotation;
    private Quaternion _lastLocalRotationJointSpace;
    private Quaternion _targetRotation;

    private void Awake() {
        _jointBody = _joint.GetComponent<Rigidbody>();
        _transform = gameObject.GetComponent<Transform>();

        _jointBaseLocalRotation = Quaternion.Inverse(_joint.connectedBody.transform.rotation) * _transform.rotation;
        _lastLocalRotationJointSpace = Quaternion.Inverse(_jointBaseLocalRotation) * _jointBaseLocalRotation;
    }

    void Update() {
        _jointBody.WakeUp();

        Vector3 input = new Vector3(Input.GetAxis("joy_0_0"), 0f, Input.GetAxis("joy_0_1"));
//        input = InputUtilities.CircularizeInput(input);
        
//        Quaternion inputRotation = Quaternion.Euler(input * Time.deltaTime * 90f);

//        transform.rotation = transform.rotation * inputRotation; // Rotate around local
//        transform.rotation = inputRotation * transform.rotation; // Rotate around world

        // Rotate around another transform's local axes, in a parent transform's local space
//        Quaternion localRotation = Quaternion.Inverse(_joint.connectedBody.transform.rotation) * _transform.rotation;
//        localRotation = inputRotation * localRotation;
//        _transform.rotation = _joint.connectedBody.transform.rotation * localRotation;

        _targetRotation = Quaternion.Euler(input * 90f);
    }

    private void FixedUpdate() {
        _joint.targetAngularVelocity = Vector3.zero;

        Quaternion jointSpaceRotation = GetJointSpaceRotation(_joint, _jointBaseLocalRotation, _transform.rotation);

        ApplyTargetTorque(jointSpaceRotation, _targetRotation);
        ApplyAngularDampening(jointSpaceRotation);
    }

    private static Quaternion GetJointSpaceRotation(ConfigurableJoint joint, Quaternion baseLocalRotation, Quaternion worldRotation) {
        var right = joint.axis;
        var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        var up = Vector3.Cross(forward, right).normalized;
        Quaternion jointAxisRotation = Quaternion.LookRotation(forward, up);

        Quaternion localRotation = Quaternion.Inverse(joint.connectedBody.transform.rotation) * worldRotation;
        Quaternion localRotationJointSpace = Quaternion.Inverse(baseLocalRotation) * localRotation;

        localRotationJointSpace = Quaternion.Inverse(jointAxisRotation) * localRotationJointSpace;

        return localRotationJointSpace;
    }

    private void ApplyTargetTorque(Quaternion jointSpaceRotation, Quaternion targetRotation) {
        Quaternion diff = targetRotation * Quaternion.Inverse(jointSpaceRotation);
        
        Vector3 axis;
        float angle;
        diff.ToAngleAxis(out angle, out axis);
        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime);

        _joint.targetAngularVelocity += angularVelocity;
    }

    private void ApplyAngularDampening(Quaternion jointSpaceRotation) {
        Quaternion rotationDelta = jointSpaceRotation * Quaternion.Inverse(_lastLocalRotationJointSpace);
        _lastLocalRotationJointSpace = jointSpaceRotation;

        Vector3 axis;
        float angle;
        rotationDelta.ToAngleAxis(out angle, out axis);

        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad / Time.deltaTime);

        _joint.targetAngularVelocity += -_dampeningFactor * angularVelocity;
    }
}
