using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// This class holds settings related to camera focus operations.
    /// </summary>
    [Serializable]
    public class EditorCameraFocusSettings
    {
        #region Private Variables
        /// <summary>
        /// Represents the camera focus mode.
        /// </summary>
        [SerializeField]
        private EditorCameraFocusMode _focusMode = EditorCameraFocusMode.Smooth;

        /// <summary>
        /// This is the camera focus speed expressed in world units/second when the focus mode
        /// is set to 'ConstantSpeed'.
        /// </summary>
        [SerializeField]
        private float _constantFocusSpeed = 10.0f;

        /// <summary>
        /// This is the amount of time it takes the camera to travel to the focus destination point.
        /// </summary>
        [SerializeField]
        private float _smoothFocusTime = 0.3f;

        /// <summary>
        /// When the camera is focused, a move direction will be generated. This value is used to scale
        /// the length of the direction vector. Bigger values move the camera further away than the 
        /// destination point which was calculated originally.
        /// </summary>
        [SerializeField]
        private float _focusDistanceScale = 1.5f;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum focus speed.
        /// </summary>
        public static float MinFocusSpeed { get { return 0.01f; } }

        /// <summary>
        /// Returns the minimum smooth focus time.
        /// </summary>
        public static float MinSmoothFocusTime { get { return 0.01f; } }

        /// <summary>
        /// Returns the minimum focus scale.
        /// </summary>
        public static float MinFocusScale { get { return 1.0f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the camera focus mode.
        /// </summary>
        public EditorCameraFocusMode FocusMode { get { return _focusMode; } set { _focusMode = value; } }

        /// <summary>
        /// Gets/sets the focus speed when the focus mode is set to 'ConstantSpeed'. The minimum value that 
        /// this property can have is given by the 'MinFocusSpeed' property. Values smaller than that will be
        /// clamped accordingly.
        /// </summary>
        public float ConstantFocusSpeed { get { return _constantFocusSpeed; } set { _constantFocusSpeed = Mathf.Max(MinFocusSpeed, value); } }

        /// <summary>
        /// Gets/sets the smooth focus time. The minimum value that this property can have is given by the
        /// 'MinSmoothFocusTime' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float SmoothFocusTime { get { return _smoothFocusTime; } set { _smoothFocusTime = Mathf.Max(MinSmoothFocusTime, value); } }

        /// <summary>
        /// Gets/sets the focus distance scale. The minium value for this property is given by
        /// 'MinFocusScale'. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float FocusDistanceScale { get { return _focusDistanceScale; } set { _focusDistanceScale = Mathf.Max(MinFocusScale, value); } }
        #endregion
    }
}