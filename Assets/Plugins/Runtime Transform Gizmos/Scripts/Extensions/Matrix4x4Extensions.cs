using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Matrix4x4' extension methods.
    /// </summary>
    public static class Matrix4x4Extensions
    {
        #region Public Static Functions
        public static Vector3[] GetAllAxes(this Matrix4x4 matrix)
        {
            return new Vector3[]
            {
                matrix.GetAxis(0), matrix.GetAxis(1), matrix.GetAxis(2)
            };
        }

        /// <summary>
        /// Returns the axis with the specified index from the coordinate system 
        /// encoded by 'matrix.
        /// </summary>
        /// <remarks>
        /// The function does not perform any validation for the specified axis index.
        /// </remarks>
        /// <param name="matrix">
        /// The matrix whose coordinate system will be used to retrieve the axis
        /// with the specified index.
        /// </param>
        /// <param name="axisIndex">
        /// The index of the axis which must be retrieved.
        /// </param>
        /// <returns>
        /// A normalized vector which represents the axis with the specified index.
        /// </returns>
        public static Vector3 GetAxis(this Matrix4x4 matrix, int axisIndex)
        {
            // Retrieve the axis and normalize it
            Vector3 axis = matrix.GetColumn(axisIndex);
            axis.Normalize();

            // Return the axis to the caller
            return axis;
        }

        public static Vector3 GetTranslation(this Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }

        public static Matrix4x4 SetTranslation(this Matrix4x4 matrix, Vector3 translation)
        {
            matrix.SetColumn(3, new Vector4(translation.x, translation.y, translation.z, 1.0f));
            return matrix;
        }

        public static Quaternion GetRotation(this Matrix4x4 matrix)
        {
            Vector3 upAxis = matrix.GetAxis(1);
            Vector3 lookAxis = matrix.GetAxis(2);

            return Quaternion.LookRotation(lookAxis, upAxis);
        }

        public static Matrix4x4 SetRotation(this Matrix4x4 matrix, Quaternion rotation)
        {
            Vector3 translation = matrix.GetTranslation();
            Vector3 scale = matrix.GetXYZScale();

            return Matrix4x4.TRS(translation, rotation, scale);
        }

        // Note: Works only with scale matrices (no rotation data).
        public static Vector3 GetSignedXYZScale(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.GetColumn(0)[0], matrix.GetColumn(1)[1], matrix.GetColumn(2)[2]);
        }

        // Note: Returns only positive scale values even if the matrix contains negative scale.
        public static Vector3 GetXYZScale(this Matrix4x4 matrix)
        {
            // The scale vector is calculated by calculating the magnitude of each
            // columm vector inside the specified matrix.
            return new Vector3(matrix.GetColumn(0).magnitude,
                               matrix.GetColumn(1).magnitude,
                               matrix.GetColumn(2).magnitude);
        }

        public static Matrix4x4 SetXYZScale(this Matrix4x4 matrix, Vector3 scale)
        {
            Vector3 translation = matrix.GetTranslation();
            Quaternion rotation = matrix.GetRotation();

            return Matrix4x4.TRS(translation, rotation, scale);
        }

        public static Matrix4x4 SetXYZScale(this Matrix4x4 matrix, float scale)
        {
            return matrix.SetXYZScale(new Vector3(scale, scale, scale));
        }

        /// <summary>
        /// Given a specified matrix, the function will make sure that the encoded scale
        /// transform is set to one for all axes and returns the updated matrix.
        /// </summary>
        /// <param name="matrix">
        /// The matrix whose scale transform must be set to 1 for all axes.
        /// </param>
        /// <returns>
        /// Returns the updated matrix.
        /// </returns>
        public static Matrix4x4 SetScaleToOneOnAllAxes(this Matrix4x4 matrix)
        {
            // Loop through each axis
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                // Store the axis for easy access
                Vector4 axis = matrix.GetColumn(axisIndex);

                // Normalize the axis to make it unit length. This eliminates any scale information for the axis.
                Vector3 normalizedAxis = axis;
                normalizedAxis.Normalize();

                // Adjust the matrix column.
                // Note: Although, not really necessary, we will preserve the old 'W' component of the
                //       matrix which is stored in the 'axis' variable.
                matrix.SetColumn(axisIndex, new Vector4(normalizedAxis.x, normalizedAxis.y, normalizedAxis.z, axis.w));
            }

            // Return the updated matrix
            return matrix;
        }
        #endregion
    }
}
