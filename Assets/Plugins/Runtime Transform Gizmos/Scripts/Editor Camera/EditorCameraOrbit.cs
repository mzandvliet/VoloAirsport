using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class which can be used to perform camera orbiting operations.
    /// </summary>
    public static class EditorCameraOrbit
    {
        #region Public Static Functions
        /// <summary>
        /// Orbits 'camera' around 'orbitPoint' by rotating around the camera's right vector
        /// by 'degreesCameraRight' degrees and around the world up vector by 'degreesGlobalUp'
        /// degrees.
        /// </summary>
        public static void OrbitCamera(Camera camera, float degreesCameraRight, float degreesGlobalUp, Vector3 orbitPoint)
        {
            // If the camera is too close to the orbit point, there is nothing to do
            Transform cameraTransform = camera.transform;
            if ((cameraTransform.position - orbitPoint).magnitude < 1e-5f) return;

            // Rotate the camera position and orientation
            cameraTransform.RotateAround(orbitPoint, Vector3.up, degreesGlobalUp);
            cameraTransform.RotateAround(orbitPoint, cameraTransform.right, degreesCameraRight);

            // Make sure the camera looks at the orbit point
            cameraTransform.LookAt(orbitPoint, cameraTransform.up);
        }
        #endregion
    }
}
