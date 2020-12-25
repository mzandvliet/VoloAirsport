using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class is responsible for calculating object selection boxes when the
    /// object selection mode is set to 'EntireHierarchy'.
    /// </summary>
    public class EntireHierarchySelectionBoxCalculator : ObjectSelectionBoxCalculator
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

            // We will need to calculate the selection boxes for the hierarchies to which the objects belong,
            // so we will first retrieve all root objects.
            var objectRoots = GameObjectExtensions.GetRootObjectsFromObjectCollection(selectedObjects);

            // Loop through all root objects 
            foreach (GameObject rootObject in objectRoots)
            {
                // Retrieve the hierarchy's model space AABB and if it is valid, create a new
                // object selection box and add it to the output list.
                Box hierarchyModelSpaceBox = rootObject.GetHierarchyModelSpaceBox();
                if (hierarchyModelSpaceBox.IsValid()) objectSelectionBoxes.Add(new ObjectSelectionBox(hierarchyModelSpaceBox, rootObject.transform.localToWorldMatrix));
            }

            return objectSelectionBoxes;
        }
        #endregion
    }
}
