using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    
    public static class PhysicsUtils
    {
        public const float DebugForceScale = .005f;

        public static Rigidbody GetRigidBodyInParents(Transform transform)
        {
            if (transform == null) {
                return null;
            }

            var body = transform.GetComponent<Rigidbody>();
            if (body) {
                return body;
            }

            if (transform.parent) {
                return GetRigidBodyInParents(transform.parent);
            }

            return null;
        }

        public static void ReorientRigidbody(Rigidbody rigidbody, Vector3 position, Quaternion rotation)
        {
            bool wasKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = true;
            rigidbody.transform.position = position;
            rigidbody.transform.rotation = rotation;
            rigidbody.isKinematic = wasKinematic;
        }

        public static void TranslateRigidbody(Rigidbody rigidbody, Vector3 delta, Space space)
        {
            bool wasKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = true;
            rigidbody.transform.Translate(delta, space);
            rigidbody.isKinematic = wasKinematic;
        }

        public static void AddTorqueAtPosition(this Rigidbody me, Vector3 torque, Vector3 position, ForceMode forceMode) {
            Vector3 torqueAxis = torque.normalized;
            Vector3 hinge = Vector3.right;
            if ((torqueAxis - hinge).sqrMagnitude < float.Epsilon) { // Chosen hinge cannot be the same as torque axis
                hinge = Vector3.up;
            }
            Vector3.OrthoNormalize(ref torqueAxis, ref hinge); // Make hinge orthogonal to torque axis
            AddTorqueAtPosition(me, torque, hinge, position, forceMode);
        }

        public static void AddTorqueAtPosition(this Rigidbody me, Vector3 torque, Vector3 hinge, Vector3 position, ForceMode forceMode) {
            Vector3 force = Vector3.Cross(0.5f * torque, hinge);
            me.AddForceAtPosition(force, position + hinge, forceMode);
            me.AddForceAtPosition(-force, position - hinge, forceMode);
//            Debug.DrawLine(position + hinge, position - hinge, Color.white);
//            Debug.DrawRay(position + hinge, force, Color.green);
//            Debug.DrawRay(position - hinge, -force, Color.green);
        }

        /* Todo:
         * Treat torque as if it's an infinite cylinder
         * - Find relation of body CoM to torque point
         * - Project this onto plane of rotation
         * - Apply linear and angular force based on this
         */

        public static void AddAccelerationAroundPosition(this Rigidbody me, Vector3 axis, float angularSpeed, Vector3 position, ForceMode forceMode) {
            Vector3 delta = me.transform.position - position;
            //Vector3 projectedDelta = delta - Vector3.Project(delta, axis);
            //Vector3 normal = projectedDelta.normalized;
            Vector3 force = Vector3.Cross(axis, delta) * angularSpeed;
            me.AddForce(force, forceMode);
        }
    }

}
