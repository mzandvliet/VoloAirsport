using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Abstract base class which must be implemented by all classes that are responsible
    /// for calculating object selection boxes in a specific manner.
    /// </summary>
    public abstract class ObjectSelectionBoxCalculator
    {
        #region Public Abstract Methods
        /// <summary>
        /// Abstract method which calculates and returns a list of the object selection boxes 
        /// for the specified object selection.
        /// </summary>
        public abstract List<ObjectSelectionBox> CalculateForObjectSelection(HashSet<GameObject> selectedObjects);
        #endregion
    }
}
