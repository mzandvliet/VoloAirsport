using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// This class holds render settings for the object selection rectangle which
    /// is used to select multiple objects in the scene.
    /// </summary>
    [Serializable]
    public class ObjectSelectionRectangleRenderSettings
    {
        #region Private Variables
        /// <summary>
        /// This is the color that must be used when drawing the object selection rectangle border lines.
        /// </summary>
        [SerializeField]
        private Color _borderLineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The object selection rectangle's fill color.
        /// </summary>
        [SerializeField]
        private Color _fillColor = new Color(95.0f / 255.0f, 109.0f / 255.0f, 130.0f / 255.0f, 0.5f);
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the color which must be used when drawing the object selection rectangle border lines.
        /// </summary>
        public Color BorderLineColor { get { return _borderLineColor; } set { _borderLineColor = value; } }

        /// <summary>
        /// Gets/sets the fill color which must be used to render the object selection rectangle.
        /// </summary>
        public Color FillColor { get { return _fillColor; } set { _fillColor = value; } }
        #endregion
    }
}
