using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Bounds' extension methods.
    /// </summary>
    public static class BoundsExtensions
    {
        #region Public Static Functions
        /// <summary>
        /// Returns an instance of the 'Bounds' class which is invalid. An invalid 'Bounds' instance 
        /// is one which has its size vector set to 'float.MaxValue' for all 3 components. The center
        /// of an invalid bounds instance is the zero vector.
        /// </summary>
        public static Bounds GetInvalidBoundsInstance()
        {
            return new Bounds(Vector3.zero, GetInvalidBoundsSize());
        }

        /// <summary>
        /// Checks if the specified bounds instance is valid. A valid 'Bounds' instance is
        /// one whose size vector does not have all 3 components set to 'float.MaxValue'.
        /// </summary>
        public static bool IsValid(this Bounds bounds)
        {
            return bounds.size != GetInvalidBoundsSize();
        }

        /// <summary>
        /// Transforms 'bounds' using the specified transform matrix.
        /// </summary>
        /// <remarks>
        /// Transforming a 'Bounds' instance means that the function will construct a new 'Bounds' 
        /// instance which has its center translated using the translation information stored in
        /// the specified matrix and its size adjusted to account for rotation and scale. The size
        /// of the new 'Bounds' instance will be calculated in such a way that it will contain the
        /// old 'Bounds'.
        /// </remarks>
        /// <param name="bounds">
        /// The 'Bounds' instance which must be transformed.
        /// </param>
        /// <param name="transformMatrix">
        /// The specified 'Bounds' instance will be transformed using this transform matrix. The function
        /// assumes that the matrix doesn't contain any projection or skew transformation.
        /// </param>
        /// <returns>
        /// The transformed 'Bounds' instance.
        /// </returns>
        public static Bounds Transform(this Bounds bounds, Matrix4x4 transformMatrix)
        {
            // We will need access to the right, up and look vector which are encoded inside the transform matrix
            Vector3 rightAxis = transformMatrix.GetColumn(0);
            Vector3 upAxis = transformMatrix.GetColumn(1);
            Vector3 lookAxis = transformMatrix.GetColumn(2);

            // We will 'imagine' that we want to rotate the bounds' extents vector using the rotation information
            // stored inside the specified transform matrix. We will need these when calculating the new size if
            // the transformed bounds.
            Vector3 rotatedExtentsRight = rightAxis * bounds.extents.x;
            Vector3 rotatedExtentsUp = upAxis * bounds.extents.y;
            Vector3 rotatedExtentsLook = lookAxis * bounds.extents.z;

            // Calculate the new bounds size along each axis. The size on each axis is calculated by summing up the 
            // corresponding vector component values of the rotated extents vectors. We multiply by 2 because we want
            // to get a size and curently we are working with extents which represent half the size.
            float newSizeX = (Mathf.Abs(rotatedExtentsRight.x) + Mathf.Abs(rotatedExtentsUp.x) + Mathf.Abs(rotatedExtentsLook.x)) * 2.0f;
            float newSizeY = (Mathf.Abs(rotatedExtentsRight.y) + Mathf.Abs(rotatedExtentsUp.y) + Mathf.Abs(rotatedExtentsLook.y)) * 2.0f;
            float newSizeZ = (Mathf.Abs(rotatedExtentsRight.z) + Mathf.Abs(rotatedExtentsUp.z) + Mathf.Abs(rotatedExtentsLook.z)) * 2.0f;

            // Construct the transformed 'Bounds' instance
            var transformedBounds = new Bounds();
            transformedBounds.center = transformMatrix.MultiplyPoint(bounds.center);
            transformedBounds.size = new Vector3(newSizeX, newSizeY, newSizeZ);

            // Return the instance to the caller
            return transformedBounds;
        }

        /// <summary>
        /// Returns the screen space corner points of the specified 'Bounds' instance.
        /// </summary>
        /// <param name="camera">
        /// The camera used for rendering to the screen. This is needed to perform the
        /// transformation to screen space.
        /// </param>
        public static Vector2[] GetScreenSpaceCornerPoints(this Bounds bounds, Camera camera)
        {
            Vector3 aabbCenter = bounds.center;
            Vector3 aabbExtents = bounds.extents;

            //  Return the screen space point array
            return new Vector2[]
        {
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x - aabbExtents.x, aabbCenter.y - aabbExtents.y, aabbCenter.z - aabbExtents.z)),
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x + aabbExtents.x, aabbCenter.y - aabbExtents.y, aabbCenter.z - aabbExtents.z)),
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x + aabbExtents.x, aabbCenter.y + aabbExtents.y, aabbCenter.z - aabbExtents.z)),
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x - aabbExtents.x, aabbCenter.y + aabbExtents.y, aabbCenter.z - aabbExtents.z)),

            camera.WorldToScreenPoint(new Vector3(aabbCenter.x - aabbExtents.x, aabbCenter.y - aabbExtents.y, aabbCenter.z + aabbExtents.z)),
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x + aabbExtents.x, aabbCenter.y - aabbExtents.y, aabbCenter.z + aabbExtents.z)),
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x + aabbExtents.x, aabbCenter.y + aabbExtents.y, aabbCenter.z + aabbExtents.z)),
            camera.WorldToScreenPoint(new Vector3(aabbCenter.x - aabbExtents.x, aabbCenter.y + aabbExtents.y, aabbCenter.z + aabbExtents.z))
        };
        }

        /// <summary>
        /// Returns the rectangle which encloses the specifies 'Bounds' instance in screen space.
        /// </summary>
        public static Rect GetScreenRectangle(this Bounds bounds, Camera camera)
        {
            // Retrieve the bounds' corner points in screen space
            Vector2[] screenSpaceCornerPoints = bounds.GetScreenSpaceCornerPoints(camera);

            // Identify the minimum and maximum points in the array
            Vector3 minScreenPoint = screenSpaceCornerPoints[0], maxScreenPoint = screenSpaceCornerPoints[0];
            for (int screenPointIndex = 1; screenPointIndex < screenSpaceCornerPoints.Length; ++screenPointIndex)
            {
                minScreenPoint = Vector3.Min(minScreenPoint, screenSpaceCornerPoints[screenPointIndex]);
                maxScreenPoint = Vector3.Max(maxScreenPoint, screenSpaceCornerPoints[screenPointIndex]);
            }

            // Return the screen space rectangle
            return new Rect(minScreenPoint.x, minScreenPoint.y, maxScreenPoint.x - minScreenPoint.x, maxScreenPoint.y - minScreenPoint.y);
        }

        public static Bounds FromPointCloud(List<Vector3> pointCloud)
        {
            if (pointCloud.Count == 0) return GetInvalidBoundsInstance();

            Vector3 min = pointCloud[0];
            Vector3 max = pointCloud[0];
            for(int ptIndex = 1; ptIndex < pointCloud.Count; ++ptIndex)
            {
                Vector3 point = pointCloud[ptIndex];
                min = Vector3.Min(point, min);
                max = Vector3.Max(point, max);
            }

            Vector3 center = (min + max) * 0.5f;
            Vector3 size = (max - min);
            return new Bounds(center, size);
        }
        #endregion

        #region Private Static Functions
        /// <summary>
        /// Returns the vector which is used to represent and invalid bounds size.
        /// </summary>
        private static Vector3 GetInvalidBoundsSize()
        {
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }
        #endregion
    }
}
