using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class acts as a base abstract class for all classes which handle
    /// object selection box rendering.
    /// </summary>
    public abstract class ObjectSelectionBoxRenderer
    {
        #region Public Abstract Methods
        /// <summary>
        /// Renders the selection boxes for the specified selected game objects. Must
        /// be implemented in all derived classes.
        /// </summary>
        public abstract void RenderObjectSelectionBoxes(HashSet<GameObject> selectedObjects);
        #endregion
    }
}
