using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Abstract class which represents an event handler that is fired by the 
    /// object selection system when a group of objects have entered the area
    /// of the multi-object selection shape.
    /// </summary>
    public abstract class ObjectSelectionGameObjectsEnteredSelectionShapeHandler
    {
        #region Public Abstract Methods
        /// <summary>
        /// Must be implemented by all derived classes to handle the objects entered
        /// selection shape event. The method returns true if the object selection has 
        /// changed after the event was handled and false otherwise.
        /// </summary>
        public abstract bool Handle(List<GameObject> gameObjects);
        #endregion
    }
}
