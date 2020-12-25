using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Factory class which can be used to create instances of object selection box calculator
    /// entities based on a specified object selection mode.
    /// </summary>
    public static class ObjectSelectionBoxCalculatorFactory
    {
        #region Public Static Functions
        /// <summary>
        /// Creates an object selection box calculator entity based on the specified
        /// object selection mode.
        /// </summary>
        public static ObjectSelectionBoxCalculator Create(ObjectSelectionMode objectSelectionMode)
        {
            switch(objectSelectionMode)
            {
                case ObjectSelectionMode.IndividualObjects:

                    return new IndividualObjectsSelectionBoxCalculator();

                case ObjectSelectionMode.EntireHierarchy:

                    return new EntireHierarchySelectionBoxCalculator();

                case ObjectSelectionMode.Custom:

                    return EditorObjectSelection.Instance.CustomObjectSelectionBoxCalculator;

                default:

                    return null;
            }
        }
        #endregion
    }
}
