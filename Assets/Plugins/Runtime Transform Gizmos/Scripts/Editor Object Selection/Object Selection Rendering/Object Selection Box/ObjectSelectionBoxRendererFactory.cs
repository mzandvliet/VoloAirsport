using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Factory class which can be used to create object selection box
    /// renderer entities based on a specified object selection box style.
    /// </summary>
    public static class ObjectSelectionBoxRendererFactory
    {
        #region Public Static Functions
        /// <summary>
        /// Creates and returns an object selection box renderer entity based on 
        /// the specified object selection box style.
        /// </summary>
        public static ObjectSelectionBoxRenderer CreateObjectSelectionBoxDrawer(ObjectSelectionBoxStyle objectSelectionBoxStyle)
        {
            switch (objectSelectionBoxStyle)
            {
                case ObjectSelectionBoxStyle.CornerLines:

                    return new CornerLinesObjectSelectionBoxRenderer();

                case ObjectSelectionBoxStyle.WireBox:

                    return new WireObjectSelectionBoxRenderer();

                default:

                    return null;
            }
        }
        #endregion
    }
}
