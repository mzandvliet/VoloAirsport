using UnityEngine;
using System.Collections;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that can be used to rotate a camera.
    /// </summary>
    public static class EditorCameraRotation
    {
        #region Public Static Functions
        /// <summary>
        /// Rotates the specified camera using the specified rotation amounts.
        /// </summary>
        /// <remarks>
        /// The function will rotate around the camera right axis first and then around
        /// the global Y axis.
        /// </remarks>
        /// <param name="camera">
        /// The camera which must be rotated.
        /// </param>
        /// <param name="degreesCameraRight">
        /// The amount of rotation in degrees which must be applied around the camera right axis.
        /// </param>
        /// <param name="degreesGlobalUp">
        /// The amount of rotation in degrees which must be applied around the global Y axis.
        /// </param>
        public static void RotateCamera(Camera camera, float degreesCameraRight, float degreesGlobalUp)
        {
            // Rotate around the camera right axis and the global Y axis respectively
            Transform cameraTransform = camera.transform;
            cameraTransform.Rotate(cameraTransform.right, degreesCameraRight, Space.World);
            cameraTransform.Rotate(Vector3.up, degreesGlobalUp, Space.World);
        }
        #endregion
    }
}
