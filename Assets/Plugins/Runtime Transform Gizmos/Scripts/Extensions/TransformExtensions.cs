using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Transform' extension methods.
    /// </summary>
    public static class TransformExtensions
    {
        #region Public Static Functions
        /// <summary>
        /// Returns a relative transform matrix which describes the transfrom information of
        /// 'transform' relative to 'referenceTransform'.
        /// </summary>
        public static Matrix4x4 GetRelativeTransform(this Transform transform, Transform referenceTransform)
        {
            return referenceTransform.localToWorldMatrix.inverse * transform.localToWorldMatrix;
        }

        public static Matrix4x4 GetWorldMatrix(this Transform transform)
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        }

        #endregion
    }
}
