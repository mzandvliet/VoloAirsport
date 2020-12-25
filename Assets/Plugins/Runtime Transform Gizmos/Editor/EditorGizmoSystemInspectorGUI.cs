//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//
//namespace RTEditor
//{
//    /// <summary>
//    /// Custom inspector implementation for the 'EditorGizmoSystem' class.
//    /// </summary>
//    [CustomEditor(typeof(EditorGizmoSystem))]
//    public class EditorGizmoSystemInspectorGUI : Editor
//    {
//        #region Private Variables
//        /// <summary>
//        /// Reference to the currently selected gizmo system object.
//        /// </summary>
//        private EditorGizmoSystem _gizmoSystem;
//        #endregion
//
//        #region Public Methods
//        /// <summary>
//        /// Called when the inspector needs to be rendered.
//        /// </summary>
//        public override void OnInspectorGUI()
//        {
//            // Let the user specify the translation gizmo
//            EditorGUILayout.Separator();
//            TranslationGizmo translationGizmo = EditorGUILayout.ObjectField("Translation Gizmo", _gizmoSystem.TranslationGizmo, typeof(TranslationGizmo), true) as TranslationGizmo;
//            if (translationGizmo != _gizmoSystem.TranslationGizmo)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
//                _gizmoSystem.TranslationGizmo = translationGizmo;
//            }
//
//            // Let the user specify the rotation gizmo
//            RotationGizmo rotationGizmo = EditorGUILayout.ObjectField("Rotation Gizmo", _gizmoSystem.RotationGizmo, typeof(RotationGizmo), true) as RotationGizmo;
//            if (rotationGizmo != _gizmoSystem.RotationGizmo)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
//                _gizmoSystem.RotationGizmo = rotationGizmo;
//            }
//
//            // Let the user specify the scale gizmo
//            ScaleGizmo scaleGizmo = EditorGUILayout.ObjectField("Scale Gizmo", _gizmoSystem.ScaleGizmo, typeof(ScaleGizmo), true) as ScaleGizmo;
//            if (scaleGizmo != _gizmoSystem.ScaleGizmo)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
//                _gizmoSystem.ScaleGizmo = scaleGizmo;
//            }
//
//            // Let the user specify the active gimo type
//            EditorGUILayout.Separator();
//            GizmoType newActiveGizmoType = (GizmoType)EditorGUILayout.EnumPopup("Active Gizmo Type", _gizmoSystem.ActiveGizmoType);
//            if (newActiveGizmoType != _gizmoSystem.ActiveGizmoType)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmoSystem);
//                _gizmoSystem.ActiveGizmoType = newActiveGizmoType;
//            }
//        }
//        #endregion
//
//        #region Private Methods
//        /// <summary>
//        /// Called when the gizmo system object is selected in the scene view.
//        /// </summary>
//        private void OnEnable()
//        {
//            _gizmoSystem = target as EditorGizmoSystem;
//        }
//        #endregion
//    }
//}
//#endif