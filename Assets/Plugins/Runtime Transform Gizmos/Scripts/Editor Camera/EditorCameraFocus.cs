using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class which is useful for performing camera focus operations. The class
    /// does not perform any focus operations on cameras, but it can be used to retrieve useful
    /// info which can be used by the client code to perform these operations.
    /// </summary>
    public static class EditorCameraFocus
    {
        #region Public Static Functions
        /// <summary>
        /// Returns an instance of the 'EditorCameraFocusOperationInfo' which holds data that can be
        /// used to perform a camera focus operation.
        /// </summary>
        /// <param name="camera">
        /// The camera which must be involved in the focus operation.
        /// </param>
        /// <param name="focusSettings">
        /// All calculations will be performed based on these settings.
        /// </param>
        public static EditorCameraFocusOperationInfo GetFocusOperationInfo(Camera camera, EditorCameraFocusSettings focusSettings)
        {
            // Retrieve the selection world space AABB
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            Bounds selectionWorldAABB = objectSelection.GetWorldBox().ToBounds();

            // We will establish the camera destination position by moving the camera along the reverse of its look vector
            // starting from the center of the world AABB by a distance equal to the maximum AABB size component.
            float maxAABBComponent = selectionWorldAABB.size.x;
            if (maxAABBComponent < selectionWorldAABB.size.y) maxAABBComponent = selectionWorldAABB.size.y;
            if (maxAABBComponent < selectionWorldAABB.size.z) maxAABBComponent = selectionWorldAABB.size.z;

            // Construct the focus operation info and return it to the caller
            EditorCameraFocusOperationInfo focusOpInfo = new EditorCameraFocusOperationInfo();
            focusOpInfo.CameraDestinationPosition = selectionWorldAABB.center - camera.transform.forward * maxAABBComponent * focusSettings.FocusDistanceScale;
            focusOpInfo.FocusPoint = selectionWorldAABB.center;

            // Now we need to calculate the ortho size that the camera should have to achieve the focus effect.
            // We will calculate the size in such a way that a 1 unit cube fits inside a volume of height = 1.
            // In this case our cube side length is equal to 'maxAABBComponent'. So we will have to make sure
            // that fits.
            // Note: We multiply by 'focusSettings.FocusDistanceScale' because the further away from the focus
            //       point the camera is, the bigger the view volume.
            focusOpInfo.OrthoCameraHalfVerticalSize = maxAABBComponent * 0.5f * focusSettings.FocusDistanceScale;

            if (camera.orthographic) focusOpInfo.NearClipPlane = 0.0f;
            else focusOpInfo.NearClipPlane = camera.nearClipPlane;

            return focusOpInfo;
        }
        #endregion
    }
}
