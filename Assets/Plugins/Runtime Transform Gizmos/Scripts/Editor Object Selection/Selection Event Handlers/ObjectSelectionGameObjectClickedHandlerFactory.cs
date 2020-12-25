using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Factory class which can be used to create game object clicked handlers
    /// based on a specified object selection mode.
    /// </summary>
    public static class ObjectSelectionGameObjectClickedHandlerFactory
    {
        #region Public Static Functions
        /// <summary>
        /// Creates a game object clicked handler based on the specified object selection mode.
        /// </summary>
        public static ObjectSelectionGameObjectClickedHandler Create(ObjectSelectionMode objectSelectionMode)
        {
            switch (objectSelectionMode)
            {
                case ObjectSelectionMode.IndividualObjects:

                    return new IndividualObjectsSelectionGameObjectClickedHandler();

                case ObjectSelectionMode.EntireHierarchy:

                    return new EntireHierarchyObjectSelectionGameObjectClickedHandler();

                case ObjectSelectionMode.Custom:

                    return EditorObjectSelection.Instance.CustomObjectClickedHandler;

                default:

                    return null;
            }
        }
        #endregion
    }
}
