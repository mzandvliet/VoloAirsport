using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds settings which control the way in which an object selection box is rendered.
    /// </summary>
    [Serializable]
    public class ObjectSelectionBoxRenderSettings
    {
        #region Private Variables
        /// <summary>
        /// The selection box style.
        /// </summary>
        [SerializeField]
        private ObjectSelectionBoxStyle _selectionBoxStyle = ObjectSelectionBoxStyle.CornerLines;

        [SerializeField]
        private float _selectionBoxCornerLinePercentage = 0.5f;

        /// <summary>
        /// The color which must be used to render the object selection box lines.
        /// </summary>
        [SerializeField]
        private Color _selectionBoxLineColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        [SerializeField]
        private float _boxSizeAdd = 0.005f;

        [SerializeField]
        private bool _drawBoxes = true;
        #endregion

        #region Public Static Properties
        public static float MinSelectionBoxCornerLinePercentage { get { return 0.01f; } }
        public static float MaxSelectionBoxCornerLinePercentage { get { return 1.0f; } }
        public static float MinSelectionBoxSizeAdd { get { return 0.001f; } }
        #endregion

        #region Public Properties
        public ObjectSelectionBoxStyle SelectionBoxStyle { get { return _selectionBoxStyle; } set { _selectionBoxStyle = value; } }
        public float SelectionBoxCornerLinePercentage { get { return _selectionBoxCornerLinePercentage; } set { _selectionBoxCornerLinePercentage = Mathf.Clamp(value, MinSelectionBoxCornerLinePercentage, MaxSelectionBoxCornerLinePercentage); } }
        public Color SelectionBoxLineColor { get { return _selectionBoxLineColor; } set { _selectionBoxLineColor = value; } }
        public float BoxSizeAdd { get { return _boxSizeAdd; } set { _boxSizeAdd = Mathf.Max(MinSelectionBoxSizeAdd, value); } }
        public bool DrawBoxes { get { return _drawBoxes; } set { _drawBoxes = value; } }
        #endregion
    }
}
