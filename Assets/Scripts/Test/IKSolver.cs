using System.Collections.Generic;
using UnityEngine;
using MathUtils = RamjetAnvil.Unity.Utility.MathUtils;

namespace RamjetAnvil.Unity.IK
{
    public static class IKSolver
    {
        private const float MaxError = 0.01f;
        private const int MaxIterations = 100;

        public static IList<MuscleJoint> GetHierarchy(MuscleJoint root)
        {
            IList<MuscleJoint> joints = new List<MuscleJoint>();

            MuscleJoint joint = root;
            while (joint != null)
            {
                joints.Add(joint);
                joint = joint.transform.parent ? joint.transform.parent.GetComponent<MuscleJoint>() : null;
            }

            IList<MuscleJoint> orderedJoints = new List<MuscleJoint>();
            for (int i = 0; i < joints.Count; i++)
            {
                orderedJoints.Add(joints[joints.Count - 1 - i]);
            }

            return orderedJoints;
        }

        public static IList<MuscleJoint> CopyHierarchy(IList<MuscleJoint> joints)
        {
            IList<MuscleJoint> mirrorJoints = new MuscleJoint[joints.Count];
            for (int i = 0; i < joints.Count; i++)
            {
                MuscleJoint originalJoint = joints[i];
                Transform mirrorTransform = new GameObject(joints[i].name + "_IK").transform;
                mirrorTransform.position = originalJoint.transform.position;
                mirrorTransform.rotation = originalJoint.transform.rotation;

                MuscleJoint mirrorJoint = mirrorTransform.gameObject.AddComponent<MuscleJoint>();
                mirrorJoint.Initialize(originalJoint);

                GameObject.Destroy(mirrorJoint.GetComponent<SphereCollider>()); // Todo: superhacky

                if (i > 0)
                {
                    mirrorTransform.parent = mirrorJoints[i - 1].transform;
                }

                mirrorJoints[i] = mirrorJoint;
            }
            return mirrorJoints;
        }

        public static void MirrorHierarchy(IList<MuscleJoint> source, IList<MuscleJoint> target)
        {
            for (int i = 0; i < source.Count; i++)
            {
                MuscleJoint originalJoint = source[i];
                MuscleJoint mirrorTransform = target[i];
                mirrorTransform.transform.position = originalJoint.transform.position;
                mirrorTransform.transform.rotation = originalJoint.transform.rotation;
            }
        }

        // Cyclic Coordinate Descent
        public static void Evaluate(IList<MuscleJoint> joints, Transform target)
        {
            MuscleJoint endEffector = joints[joints.Count - 1];

            float error = (target.position - endEffector.transform.position).magnitude;
            if (error <= MaxError)
                return;

            bool done = false;
            int numIterations = 0;

            while (!done && numIterations < MaxIterations)
            {
                // From tip to root, skipping end effector
                for (int i = joints.Count - 2; i >= 0; i--)
                {
                    MuscleJoint joint = joints[i];

                    Vector3 localJointNormal = joints[i + 1].transform.localPosition.normalized;

                    // Find rotation to target
                    Vector3 targetDirection = target.position - joint.transform.position;
                    Vector3 localTargetDirection = joint.transform.parent
                                                       ? joint.transform.parent.InverseTransformDirection(
                                                           targetDirection)
                                                       : targetDirection;
                    Quaternion targetDelta = Quaternion.FromToRotation(joint.transform.localRotation*localJointNormal,
                                                                       localTargetDirection);

                    // Find rotation to end effector
                    Vector3 endDirection = endEffector.transform.position - joint.transform.position;
                    Vector3 localEndDirection = joint.transform.parent
                                                    ? joint.transform.parent.InverseTransformDirection(endDirection)
                                                    : endDirection;
                    Quaternion endDelta = Quaternion.FromToRotation(joint.transform.localRotation*localJointNormal,
                                                                    localEndDirection);

                    // Construct a rotation that puts end effector on the line to (e.g. closest to) the target
                    Quaternion proposedLocalRotation = targetDelta*Quaternion.Inverse(endDelta)*
                                                       joint.transform.localRotation;

                    // Enforce joint limits

                    // Ensure bone rotation stays within limits

                    if (!IsWithinLimits(joint, proposedLocalRotation))
                    {
                        proposedLocalRotation = ClampToLimits(joint, proposedLocalRotation);
                        //RandomizeChain(joint, 0.01f);
                    }

                    joint.transform.localRotation = proposedLocalRotation;
                }

                // Early out if we've reached a local minimum
                float newError = (target.position - endEffector.transform.position).magnitude;
                if (newError <= MaxError || error - newError < 0.01f)
                    done = true;
                error = newError;

                numIterations++;
            }

            UnityEngine.Debug.Log(numIterations);
        }

        public static float CalculateLength(IList<MuscleJoint> joints)
        {
            float length = 0f;
            for (int i = 0; i < joints.Count - 1; i++)
            {
                length += (joints[i].transform.position - joints[i + 1].transform.position).magnitude;
            }
            return length;
        }

        private static bool IsWithinLimits(MuscleJoint joint, Quaternion proposedRotation)
        {
            Quaternion jointRotation = Quaternion.Inverse(joint.BaseLocalRotation)*proposedRotation;

            float angleX = jointRotation.eulerAngles.x;
            float angleY = jointRotation.eulerAngles.y;
            float angleZ = jointRotation.eulerAngles.z;

            angleX = WrapAngle(angleX);
            angleY = WrapAngle(angleY);
            angleZ = WrapAngle(angleZ);

            if (angleX > joint.Limits.X.Min || angleX < joint.Limits.X.Max ||
                angleY > joint.Limits.Y.Min || angleY < joint.Limits.Y.Max ||
                angleZ > joint.Limits.Z.Min || angleZ < joint.Limits.Z.Max)
                return false;

            return false;
        }

        private static Quaternion ClampToLimits(MuscleJoint joint, Quaternion proposedRotation)
        {
            return ClampLimitsEuler(joint, proposedRotation);
        }

        private static Quaternion ClampLimitsEllipse(MuscleJoint joint, Quaternion proposedRotation)
        {
            Quaternion jointRotation = Quaternion.Inverse(joint.BaseLocalRotation)*proposedRotation;

            Vector3 proposedForward = jointRotation*Vector3.forward;

            Vector3 swingAxis = Vector3.Cross(Vector3.forward, proposedForward).normalized;
            float angle = MathUtils.AngleAroundAxis(Vector3.forward, proposedForward, swingAxis);

            angle = Mathf.Clamp(angle, -45f, 45f);

            return joint.BaseLocalRotation*Quaternion.AngleAxis(angle, swingAxis);
        }

        private static Quaternion ClampLimitsEuler(MuscleJoint joint, Quaternion proposedRotation)
        {
            Quaternion jointRotation = Quaternion.Inverse(joint.BaseLocalRotation)*proposedRotation;

            float angleX = jointRotation.eulerAngles.x;
            float angleY = jointRotation.eulerAngles.y;
            float angleZ = jointRotation.eulerAngles.z;

            angleX = WrapAngle(angleX);
            angleY = WrapAngle(angleY);
            angleZ = WrapAngle(angleZ);

            angleX = Mathf.Clamp(angleX, joint.Limits.X.Min, joint.Limits.X.Max);
            angleY = Mathf.Clamp(angleY, joint.Limits.Y.Min, joint.Limits.Y.Max);
            angleZ = Mathf.Clamp(angleZ, joint.Limits.Z.Min, joint.Limits.Z.Max);

            return joint.BaseLocalRotation*Quaternion.Euler(angleX, angleY, angleZ);
        }


        // This is broken due to angle-axis issues involving projected angles
        //private static Quaternion ClampLimitsAngleAxis(MuscleJoint joint, Quaternion proposedRotation)
        //{
        //    Quaternion jointRotation = Quaternion.Inverse(joint.BaseLocalRotation) * proposedRotation;

        //    Vector3 proposedX = jointRotation * Vector3.right;
        //    Vector3 proposedY = jointRotation * Vector3.up;
        //    Vector3 proposedZ = jointRotation * Vector3.forward;

        //    float angleX = MathUtils.AngleAroundAxis(Vector3.right, proposedZ, Vector3.right);
        //    float angleY = MathUtils.AngleAroundAxis(Vector3.up, proposedX, Vector3.up);
        //    float angleZ = MathUtils.AngleAroundAxis(Vector3.forward, proposedY, Vector3.forward);

        //    angleX = Mathf.Clamp(angleX, joint.Limits.X.Min, joint.Limits.X.Max);
        //    angleY = Mathf.Clamp(angleY, joint.Limits.Y.Min, joint.Limits.Y.Max);
        //    angleZ = Mathf.Clamp(angleZ, joint.Limits.Z.Min, joint.Limits.Z.Max);

        //    return joint.BaseLocalRotation *
        //            Quaternion.Euler(0f, 0f, angleZ) *
        //            Quaternion.Euler(angleX, 0f, 0f) *
        //            Quaternion.Euler(0f, angleY, 0f);
        //}

        private static float WrapAngle(float angle)
        {
            if (angle < -180)
                return angle + 360f; // Todo float modulus for negative numbers
            if (angle > 180)
                return angle - 360f;
            return angle;
        }

        private static void RandomizeChain(MuscleJoint joint, float amount)
        {
            if (!joint.transform.parent)
                return;

            joint.transform.parent.localRotation = Quaternion.Slerp(
                joint.transform.parent.localRotation,
                Random.rotationUniform,
                amount);
        }

        public static void Slerp(IList<MuscleJoint> sources, IList<MuscleJoint> targets, float lerp)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                Slerp(sources[i], targets[i], lerp);
            }
        }

        public static void Slerp(MuscleJoint source, MuscleJoint target, float lerp)
        {
            target.transform.localRotation = Quaternion.Slerp(target.transform.localRotation,
                                                              source.transform.localRotation, lerp);
        }

        public static void DrawGizmos(IList<MuscleJoint> joints, Transform target)
        {
            const float debugAxisScale = 0.25f;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(target.position, 0.05f);

            for (int i = 0; i < joints.Count; i++)
            {
                Transform jointA = joints[i].transform;

                if (i < joints.Count - 1)
                {
                    Transform jointB = joints[i + 1].transform;

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(jointA.position, jointB.position);
                }

                Gizmos.color = Color.red;
                Gizmos.DrawRay(jointA.position, jointA.right*debugAxisScale);

                Gizmos.color = Color.green;
                Gizmos.DrawRay(jointA.position, jointA.up*debugAxisScale);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(jointA.position, jointA.forward*debugAxisScale);

                Gizmos.DrawSphere(jointA.position, 0.025f);
            }
        }
    }
}