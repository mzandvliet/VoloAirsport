using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds snap settings for a scale gizmo.
    /// </summary>
    [Serializable]
    public class ScaleGizmoSnapSettings
    {
        #region Private Variables
        /// <summary>
        /// Specifies whether or not snapping is enabled.
        /// </summary>
        [SerializeField]
        private bool _isSnappingEnabled = false;

        /// <summary>
        /// The scale snap step value in world units. When snapping is turned on, objects will be
        /// scaled in increments of this step value. For example, if the scale is applied to a cube
        /// which has a side of 1 world unit length, and the step value is 1, the cube's size will
        /// increase to 2, 3, 4, 5 etc
        /// </summary>
        [SerializeField]
        private float _stepValueInWorldUnits = 1.0f;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum step value.
        /// </summary>
        public static float MinStepValue { get { return 0.1f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not snapping is enabled.
        /// </summary>
        public bool IsSnappingEnabled { get { return _isSnappingEnabled; } set { _isSnappingEnabled = value; } }

        /// <summary>
        /// Gets/sets the step value in world units. The minimum value that this variable can have is given
        /// by the 'MinStepValue' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float StepValueInWorldUnits { get { return _stepValueInWorldUnits; } set { _stepValueInWorldUnits = Mathf.Max(MinStepValue, value); } }
        #endregion
    }
}
