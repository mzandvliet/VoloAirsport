using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds pan settings which can be associated with the editor camera.
    /// </summary>
    [Serializable]
    public class EditorCameraPanSettings
    {
        #region Private Variables
        /// <summary>
        /// The camera pan mode.
        /// </summary>
        private EditorCameraPanMode _panMode = EditorCameraPanMode.Standard;

        /// <summary>
        /// This is the smooth value that is used when the pan mode is set to 'Smooth'.
        /// </summary>
        private float _smoothValue = 0.15f;

        /// <summary>
        /// This is the camera pan speed expressed in world units/second when the pan mode
        /// is set to 'Standard'.
        /// </summary>
        [SerializeField]
        private float _standardPanSpeed = 3.0f;

        /// <summary>
        /// This is the camera pan speed expressed in world units/second when the pan mode
        /// is set to 'Smooth'.
        /// </summary>
        [SerializeField]
        private float _smoothPanSpeed = 3.0f;

        /// <summary>
        /// Specifies whether or not the X axis used for panning is inverted.
        /// </summary>
        [SerializeField]
        private bool _invertXAxis = false;

        /// <summary>
        /// Specifies whether or not the Y axis used for panning is inverted.
        /// </summary>
        [SerializeField]
        private bool _invertYAxis = false;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum value that the camera pan speed can have.
        /// </summary>
        public static float MinPanSpeed { get { return 0.01f; } }

        /// <summary>
        /// Returns the minimum pan smooth value.
        /// </summary>
        public static float MinSmoothValue { get { return 1e-5f; } }

        /// <summary>
        /// Returns the maximum pan smooth value.
        /// </summary>
        public static float MaxSmoothValue { get { return 1.0f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the pan mode.
        /// </summary>
        public EditorCameraPanMode PanMode { get { return _panMode; } set { _panMode = value; } }

        /// <summary>
        /// Gets/sets the smooth value that is used when the pan mode is set to 'Smooth'. This
        /// property takes on values in the [MinSmoothValue, MaxSmoothValue] interval. Values 
        /// outside this interval are clamped accordingly.
        /// </summary>
        public float SmoothValue { get { return _smoothValue; } set { _smoothValue = Mathf.Min(MaxSmoothValue, Mathf.Max(MinSmoothValue, value)); } }

        /// <summary>
        /// Gets/sets the camera standard pan speed. The minimum value that the camera pan speed can have is given
        /// by the 'MinPanSpeed' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float StandardPanSpeed { get { return _standardPanSpeed; } set { _standardPanSpeed = Mathf.Max(value, MinPanSpeed); } }

        /// <summary>
        /// Gets/sets the camera smooth pan speed. The minimum value that the camera pan speed can have is given
        /// by the 'MinPanSpeed' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float SmoothPanSpeed { get { return _smoothPanSpeed; } set { _smoothPanSpeed = Mathf.Max(value, MinPanSpeed); } }

        /// <summary>
        /// Gets/sets the boolean which specifies whether or not the X axis must be inverted when panning.
        /// </summary>
        public bool InvertXAxis { get { return _invertXAxis; } set { _invertXAxis = value; } }

        /// <summary>
        /// Gets/sets the boolean which specifies whether or not the Y axis must be inverted when panning.
        /// </summary>
        public bool InvertYAxis { get { return _invertYAxis; } set { _invertYAxis = value; } }
        #endregion
    }
}
