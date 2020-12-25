using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This interface must be implemented by all classes that represent actions which
    /// can be executed.
    /// </summary>
    public interface IAction
    {
        #region Interface Methods
        /// <summary>
        /// Allows the derived classes to define how an action is executed.
        /// </summary>
        void Execute();
        #endregion
    }
}
