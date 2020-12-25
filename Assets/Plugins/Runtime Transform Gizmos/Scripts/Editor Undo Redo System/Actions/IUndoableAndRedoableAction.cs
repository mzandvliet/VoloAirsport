using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is an interface which must be implemented by all classes that represent
    /// actions which can be undone and redone by the undo/redo system.
    /// </summary>
    public interface IUndoableAndRedoableAction : IUndoableAction, IRedoableAction
    {
    }
}
