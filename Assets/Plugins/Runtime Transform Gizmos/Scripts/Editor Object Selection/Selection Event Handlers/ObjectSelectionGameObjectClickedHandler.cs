using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Abstract class which represents an event handler that is fired by the 
    /// object selection system when a game object is clicked in the scene.
    /// </summary>
    public abstract class ObjectSelectionGameObjectClickedHandler
    {
        #region Public Abstract Methods
        /// <summary>
        /// Must be implemented by all derived classes to handle the object click event. The
        /// method returns true if the object selection has changed after the event was handled
        /// and false otherwise.
        /// </summary>
        public abstract bool Handle(GameObject gameObject);
        #endregion
    }
}
