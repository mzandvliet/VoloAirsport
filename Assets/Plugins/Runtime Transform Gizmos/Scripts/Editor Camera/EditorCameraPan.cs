using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that can be used to pan a camera.
    /// </summary>
    public static class EditorCameraPan
    {
        #region Public Static Functions
        /// <summary>
        /// Pans the specified camera using the specified pan amounts.
        /// </summary>
        /// <param name="camera">
        /// The camera which must be panned.
        /// </param>
        /// <param name="panAmountRight">
        /// The amount to pan to the right in world units/second.
        /// </param>
        /// <param name="panAmountUp">
        /// The amount to pan to upwards in world units/second.
        /// </param>
        public static void PanCamera(Camera camera, float panAmountRight, float panAmountUp)
        {
            // Use the specified pan amounts to pan along the camera right and up axes
            Transform cameraTransform = camera.transform;
            cameraTransform.position += (cameraTransform.right * panAmountRight + cameraTransform.up * panAmountUp);
        }
        #endregion
    }
}
