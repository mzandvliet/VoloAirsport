using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Base abstract class which must be implemented by all classes that handle
    /// object selection rendering.
    /// </summary>
    public abstract class ObjectSelectionRenderer
    {
        #region Public Abstract Methods
        /// <summary>
        /// Renders the specified object selection. The second parameter holds the object 
        /// selection settings which are needed to perform the rendering operation.
        /// </summary>
        public abstract void RenderObjectSelection(HashSet<GameObject> selectedObjects, ObjectSelectionSettings objectSelectionSettings);
        #endregion
    }
}
