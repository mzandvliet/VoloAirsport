using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class is responsible for calculating object selection boxes when the
    /// object selection mode is set to 'IndividualObjects'.
    /// </summary>
    public class IndividualObjectsSelectionBoxCalculator : ObjectSelectionBoxCalculator
    {
        #region Public Methods
        /// <summary>
        /// Calculates and returns the object selection boxes for the specified object selection.
        /// </summary>
        /// <remarks>
        /// The method returns only those selection boxes whose model space box is set to
        /// a valid 'Bounds' instance.
        /// </remarks>
        public override List<ObjectSelectionBox> CalculateForObjectSelection(HashSet<GameObject> selectedObjects)
        {
            var objectSelectionBoxes = new List<ObjectSelectionBox>(selectedObjects.Count);

            // Loop through all selected objects and create the object selection boxes
            foreach (GameObject selectedObject in selectedObjects)
            {
                // Retrieve the object's model space box and if it is valid, create a
                // new selection box and store it in the output list.
                Box modelSpaceBox = selectedObject.GetModelSpaceBox();
                if (modelSpaceBox.IsValid()) objectSelectionBoxes.Add(new ObjectSelectionBox(modelSpaceBox, selectedObject.transform.localToWorldMatrix));
            }

            return objectSelectionBoxes;
        }
        #endregion
    }
}
