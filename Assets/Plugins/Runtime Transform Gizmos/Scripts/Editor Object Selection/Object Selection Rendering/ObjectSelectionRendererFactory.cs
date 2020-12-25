using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Factory class which can be used to create instances of object selection rendering
    /// entities based on a specified object selection render mode.
    /// </summary>
    public static class ObjectSelectionRendererFactory
    {
        #region Public Static Functions
        /// <summary>
        /// Creates an instance of the correct object selection renderer type based on the 
        /// specified object selection render mode.
        /// </summary>
        public static ObjectSelectionRenderer Create(ObjectSelectionRenderMode objectSelectionRenderMode)
        {
            switch (objectSelectionRenderMode)
            {
                case ObjectSelectionRenderMode.SelectionBoxes:

                    return new SelectionBoxObjectSelectionRenderer();

                default:

                    return null;
            }
        }
        #endregion
    }
}
