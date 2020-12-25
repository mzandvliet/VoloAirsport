using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This class is used to hold useful information when performing camera focus operations.
    /// </summary>
    public class EditorCameraFocusOperationInfo
    {
        #region Public Properties
        /// <summary>
        /// When a camera is focused, its position will have to be moved to this destination 
        /// point to achieve the focus effect.
        /// </summary>
        public Vector3 CameraDestinationPosition { get; set; }

        /// <summary>
        /// This value is only used in conjunction with orthographic cameras and it represents the
        /// value of half the vertical volume size that the camera must have to achieve the focus 
        /// effect (i.e. camera.orthographicSize = OrthoCameraHalfVerticalSize).
        /// </summary>
        public float OrthoCameraHalfVerticalSize { get; set; }

        /// <summary>
        /// This is the point on which the camera will be focused after the focus operation is completed.
        /// </summary>
        public Vector3 FocusPoint { get; set; }

        public float NearClipPlane { get; set; }
        #endregion
    }
}
