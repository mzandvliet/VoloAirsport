#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorUndoRedoSystem' class.
    /// </summary>
    [CustomEditor(typeof(EditorUndoRedoSystem))]
    public class EditorUndoRedoSystemInspectorGUI : Editor
    {
        #region Private Variables
        /// <summary>
        /// Reference to the currently selected undo/redo system.
        /// </summary>
        private EditorUndoRedoSystem _editorUndoRedoSystem;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Let the user specify the maximum number of actions which can be registered with the Undo/redo system
            int newInt = EditorGUILayout.IntField("Action Limit", _editorUndoRedoSystem.ActionLimit);
            if(newInt != _editorUndoRedoSystem.ActionLimit)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorUndoRedoSystem);
                _editorUndoRedoSystem.ActionLimit = newInt;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the undo/redo system object is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _editorUndoRedoSystem = target as EditorUndoRedoSystem;
        }
        #endregion
    }
}
#endif