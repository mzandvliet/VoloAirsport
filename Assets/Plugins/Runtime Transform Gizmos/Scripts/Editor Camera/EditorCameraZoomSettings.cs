using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds zoom settings which can be associated with the editor camera.
    /// </summary>
    [Serializable]
    public class EditorCameraZoomSettings
    {
        #region Private Variables
        /// <summary>
        /// The camera zoom mode.
        /// </summary>
        [SerializeField]
        private EditorCameraZoomMode _zoomMode = EditorCameraZoomMode.Standard;

        /// <summary>
        /// Can be used to toggle camera zoom on/off as needed.
        /// </summary>
        [SerializeField]
        private bool _isZoomEnabled = true;

        /// <summary>
        /// The smooth value used when the zoom mode is set to 'Smooth' and the camera works
        /// in orthographic mode.
        /// </summary>
        [SerializeField]
        private float _orthographicSmoothValue = 0.1f;

        /// <summary>
        /// The smooth value used when the zoom mode is set to 'Smooth' and the camera works
        /// in perspective mode.
        /// </summary>
        [SerializeField]
        private float _perspectiveSmoothValue = 0.2f;

        /// <summary>
        /// This is the camera zoom speed when the camera works in orthographic mode and when
        /// the zoom mode is set to 'Standard'.
        /// </summary>
        [SerializeField]
        private float _orthographicStandardZoomSpeed = 10.0f;

        /// <summary>
        /// This is the camera zoom speed when the camera works in perspective mode and when
        /// the zoom mode is set to 'Standard'.
        /// </summary>
        [SerializeField]
        private float _perspectiveStandardZoomSpeed = 400.0f;

        /// <summary>
        /// This is the camera zoom speed when the camera works in orthographic mode and when
        /// the zoom mode is set to 'Smooth'.
        /// </summary>
        [SerializeField]
        private float _orthographicSmoothZoomSpeed = 65.0f;

        /// <summary>
        /// This is the camera zoom speed when the camera works in perspective mode and when
        /// the zoom mode is set to 'Smooth'.
        /// </summary>
        [SerializeField]
        private float _perspectiveSmoothZoomSpeed = 400.0f;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum zoom speed which applies to both orthographic and perspective cameras.
        /// </summary>
        public static float MinZoomSpeed { get { return 0.01f; } }

        /// <summary>
        /// Returns the minimum zoom smooth value.
        /// </summary>
        public static float MinSmoothValue { get { return 1e-5f; } }

        /// <summary>
        /// Returns the maximum zoom smooth value.
        /// </summary>
        public static float MaxSmoothValue { get { return 1.0f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the zoom mode.
        /// </summary>
        public EditorCameraZoomMode ZoomMode { get { return _zoomMode; } set { _zoomMode = value; } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies if camera zoom is enabled.
        /// </summary>
        public bool IsZoomEnabled { get { return _isZoomEnabled; } set { _isZoomEnabled = value; } }

        /// <summary>
        /// Gets/sets the smooth value used when the zoom mode is set to 'Smooth' and the camera works
        /// in orthographic mode. This property can take on values within the [MinSmoothValue, MaxSmoothValue] 
        /// interval. Values outside this interval are clamped accordingly.
        /// </summary>
        public float OrthographicSmoothValue { get { return _orthographicSmoothValue; } set { _orthographicSmoothValue = Mathf.Min(MaxSmoothValue, Mathf.Max(MinSmoothValue, value)); } }

        /// <summary>
        /// Gets/sets the smooth value used when the zoom mode is set to 'Smooth' and the camera works
        /// in perspective mode. This property can take on values within the [MinSmoothValue, MaxSmoothValue] 
        /// interval. Values outside this interval are clamped accordingly.
        /// </summary>
        public float PerspectiveSmoothValue { get { return _perspectiveSmoothValue; } set { _perspectiveSmoothValue = Mathf.Min(MaxSmoothValue, Mathf.Max(MinSmoothValue, value)); } }

        /// <summary>
        /// Gets/sets the camera zoom speed for orthographic mode when the zoom mode is set to
        /// 'Standard'. The minimum zoom speed is given by the 'MinZoomSpeed' property. Values 
        /// smaller than that will be clamped accordingly.
        /// </summary>
        public float OrthographicStandardZoomSpeed { get { return _orthographicStandardZoomSpeed; } set { _orthographicStandardZoomSpeed = Mathf.Max(value, MinZoomSpeed); } }

        /// <summary>
        /// Gets/sets the camera zoom speed for perspective mode when the zoom mode is set to
        /// 'Standard'. The minimum zoom speed is given by the 'MinZoomSpeed' property. Values
        /// smaller than that will be clamped accordingly.
        /// </summary>
        public float PerspectiveStandardZoomSpeed { get { return _perspectiveStandardZoomSpeed; } set { _perspectiveStandardZoomSpeed = Mathf.Max(value, MinZoomSpeed); } }

        /// <summary>
        /// Gets/sets the camera zoom speed for orthographic mode when the zoom mode is set to
        /// 'Smooth'. The minimum zoom speed is given by the 'MinZoomSpeed' property. Values 
        /// smaller than that will be clamped accordingly.
        /// </summary>
        public float OrthographicSmoothZoomSpeed { get { return _orthographicSmoothZoomSpeed; } set { _orthographicSmoothZoomSpeed = Mathf.Max(value, MinZoomSpeed); } }

        /// <summary>
        /// Gets/sets the camera zoom speed for orthographic mode when the zoom mode is set to
        /// 'Smooth'. The minimum zoom speed is given by the 'MinZoomSpeed' property. Values 
        /// smaller than that will be clamped accordingly.
        /// </summary>
        public float PerspectiveSmoothZoomSpeed { get { return _perspectiveSmoothZoomSpeed; } set { _perspectiveSmoothZoomSpeed = Mathf.Max(value, MinZoomSpeed); } }
        #endregion
    }
}