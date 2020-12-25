using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a factory class which can be used to create game objects entered selection
    /// shape event handlers based on a specified object selection mode.
    /// </summary>
    public static class ObjectSelectionGameObjectsEnteredSelectionShapeHandlerFactory
    {
        #region Public Static Functions
        /// <summary>
        /// Creates a game objects entered selection shape handler based on the specified object selection mode.
        /// </summary>
        public static ObjectSelectionGameObjectsEnteredSelectionShapeHandler Create(ObjectSelectionMode objectSelectionMode)
        {
            switch (objectSelectionMode)
            {
                case ObjectSelectionMode.IndividualObjects:

                    return new IndividualObjectsSelectionGameObjectsEnteredSelectionShapeHandler();

                case ObjectSelectionMode.EntireHierarchy:

                    return new EntireHierarchyObjectSelectionGameObjectsEnteredSelectionShapeHandler();

                case ObjectSelectionMode.Custom:

                    return EditorObjectSelection.Instance.CustomObjectsEnteredSelectionShapeHandler;

                default:

                    return null;
            }
        }
        #endregion
    }
}
