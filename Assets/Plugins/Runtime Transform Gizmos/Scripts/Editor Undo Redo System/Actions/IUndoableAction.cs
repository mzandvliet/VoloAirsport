using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This interface must be implemented by all classes that represent actions
    /// which can be undone by the undo/redo system.
    /// </summary>
    public interface IUndoableAction
    {
        #region Interface Methods
        /// <summary>
        /// Allows the derived classes to specify how the action is undone.
        /// </summary>
        void Undo();
        #endregion
    }
}
