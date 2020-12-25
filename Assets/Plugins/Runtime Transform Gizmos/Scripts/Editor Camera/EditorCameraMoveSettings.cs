using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds camera move related settings.
    /// </summary>
    [Serializable]
    public class EditorCameraMoveSettings
    {
        #region Private Variables
        /// <summary>
        /// The speed by which the camera can be moved.
        /// </summary>
        [SerializeField]
        private float _moveSpeed = 10.0f;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum move speed.
        /// </summary>
        public static float MinMoveSpeed { get { return 1.0f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the camera move speed. The minimum value is given by the 'MinMoveSpeed'
        /// property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = Mathf.Max(value, MinMoveSpeed); } }
        #endregion
    }
}
