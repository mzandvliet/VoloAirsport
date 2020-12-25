#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    /// <summary>
    /// This is a static class which offers useful wrappers for Unity's 'Undo' API.
    /// </summary>
    public static class UnityEditorUndoHelper
    {
        #region Public Static Functions
        /// <summary>
        /// This function must be used when the client code needs to record an object
        /// before its properties are modified in the inspector.
        /// </summary>
        public static void RecordObjectForInspectorPropertyChange(UnityEngine.Object objectToRecord)
        {
            // We will only record the object if we are not running in play mode because if we are,
            // the properties will be reset when the user exits play mode anyway.
            if (!Application.isPlaying) Undo.RecordObject(objectToRecord, "Object Property Undo");
        }
        #endregion
    }
}
#endif