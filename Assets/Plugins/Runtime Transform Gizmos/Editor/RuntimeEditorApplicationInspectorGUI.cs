using UnityEngine;
using UnityEditor;
using System;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorApplication' class.
    /// </summary>
    [CustomEditor(typeof(RuntimeEditorApplication))]
    public class RuntimeEditorApplicationInspectorGUI : Editor
    {
        #region Private Variables
        /// <summary>
        /// Reference to the currently selected editor application object.
        /// </summary>
        private RuntimeEditorApplication _editorApplication;
        private static bool _xzGridSetingsAreVisible = true;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            Vector3 newVec3 = EditorGUILayout.Vector3Field("Light Object Volume Size", _editorApplication.VolumeSizeForLightObjects);
            if(newVec3 != _editorApplication.VolumeSizeForLightObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                _editorApplication.VolumeSizeForLightObjects = newVec3;
            }

            newVec3 = EditorGUILayout.Vector3Field("Particle System Object Volume Size", _editorApplication.VolumeSizeForParticleSystemObjects);
            if (newVec3 != _editorApplication.VolumeSizeForParticleSystemObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                _editorApplication.VolumeSizeForParticleSystemObjects = newVec3;
            }

            newVec3 = EditorGUILayout.Vector3Field("Empty Object Volume Size", _editorApplication.VolumeSizeForEmptyObjects);
            if (newVec3 != _editorApplication.VolumeSizeForEmptyObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorApplication);
                _editorApplication.VolumeSizeForEmptyObjects = newVec3;
            }

            _xzGridSetingsAreVisible = EditorGUILayout.Foldout(_xzGridSetingsAreVisible, "XZ Grid Settings");
            if (_xzGridSetingsAreVisible)
            {
                EditorGUI.indentLevel += 1;
                _editorApplication.XZGrid.RenderView(_editorApplication);
                EditorGUI.indentLevel -= 1;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the editor application object is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _editorApplication = target as RuntimeEditorApplication;
        }
        #endregion
    }
}
