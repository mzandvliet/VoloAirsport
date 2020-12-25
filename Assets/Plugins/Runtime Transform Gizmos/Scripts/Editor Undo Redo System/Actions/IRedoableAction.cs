using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This interface must be implemented by all classes that represent actions
    /// which can be redone by the undo/redo system.
    /// </summary>
    public interface IRedoableAction
    {
        #region Interface Methods
        /// <summary>
        /// Allows the derived classes to specify how the action is redone.
        /// </summary>
        void Redo();
        #endregion
    }
}