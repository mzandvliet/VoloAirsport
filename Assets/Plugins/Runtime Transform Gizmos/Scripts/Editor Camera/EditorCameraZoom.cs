using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that can be used to apply zoom to a camera.
    /// </summary>
    public static class EditorCameraZoom
    {
        #region Public Static Functions
        /// <summary>
        /// Zooms the specified camera by the specified amount.
        /// </summary>
        /// <param name="camera">
        /// The camera which must be zoomed.
        /// </param>
        /// <param name="zoomAmount">
        /// The zoom amount. Positive values will zoom in and negative values will zoom out.
        /// </param>
        public static void ZoomCamera(Camera camera, float zoomAmount)
        {
            // We will call 'ZoomOrthoCameraViewVolume' even if the camera is not orthographic because
            // that will ensure correct behaviour when switching from perspective to ortho.
            float zoomAmountScale = 1.0f;
            zoomAmountScale = ZoomOrthoCameraViewVolume(camera, zoomAmount);

            // If the camera is not orthographc, we will not limit the amount of zoom
            if (!camera.orthographic) zoomAmountScale = 1.0f;
            else camera.nearClipPlane += zoomAmount;

            // Regardless of the type of camera we are using, we will also move the position of the camera. It may appear
            // strange that we are doing this for an orthographic camera also but this is actually necessary because a camera
            // has a near and a far clip plane. If we don't move the camera along its look vector, objects may get clipped
            // by the near or far clip plane regardless of the zoom factor that is applied to the camera.
            // Note: We make sure to scale the zoom amount by the 'zoomAmountScale' variable.
            Transform cameraTransform = camera.transform;
            cameraTransform.position += cameraTransform.forward * zoomAmount * zoomAmountScale;
        }

        /// <summary>
        /// This function can be called to apply a zoom effect to an ortho camera's view volume.
        /// </summary>
        /// <returns>
        /// A scale factor which can be used to scale the zoom amount that is applied to
        /// the camera to achieve the zoom effect. The position of the camera must be adjusted
        /// by taking this value into account (e.g. pos = pos + look * zoomAmount * zoomScale).
        /// </returns>
        public static float ZoomOrthoCameraViewVolume(Camera camera, float zoomAmount)
        {
            // Start with a zoom scale of 1. We will modify this if needed.
            float zoomAmountScale = 1.0f;

            // We will use a minimum value for the orthographic size. This is because if we allow the size
            // to become < than 0, the scene will be inverted. Having it set to 0, is also not good because
            // exceptions will be thrown.
            const float minOrthoSize = 0.001f;

            // Calculate the new ortho size 
            float newOrthoSize = camera.orthographicSize - zoomAmount;

            // Is the new ortho size < than the allowed minimum?
            // Note: If it is, what we would normally have to do is to just clamp the size to the
            //       minimum value. However, we must calculate the the zoom ammount scale factor
            //       so that we can correctly return it from the function.
            if (newOrthoSize < minOrthoSize)
            {
                float delta = minOrthoSize - newOrthoSize;                  // Holds the amount which must be subtracted from the zoom
                float percentageOfRemovedZoom = delta / zoomAmount;         // Holds the percentage of zoom which was removed

                // Clamp the new ortho size to the allowed minimum
                newOrthoSize = minOrthoSize;

                // Calculate the zoom scale factor. We start from 1 and subtract the
                // percentage which was removed earlier.
                zoomAmountScale = 1.0f - percentageOfRemovedZoom;
            }

            // Set the new ortho size
            camera.orthographicSize = Mathf.Max(minOrthoSize, newOrthoSize);

            // Return the established zoom amount scale
            return zoomAmountScale;
        }
        #endregion
    }
}
