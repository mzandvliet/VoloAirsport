//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;
//
//namespace RTEditor
//{
//    /// <summary>
//    /// Custom inspector implementation for the 'TranslationGizmo' class.
//    /// </summary>
//    [CustomEditor(typeof(TranslationGizmo))]
//    public class TranslationGizmoInspectorGUI : GizmoInspectorGUIBase
//    {
//        #region Private Variables
//        /// <summary>
//        /// Reference to the currently selected translation gizmo.
//        /// </summary>
//        private TranslationGizmo _translationGizmo;
//        #endregion
//
//        #region Public Methods
//        /// <summary>
//        /// Called when the inspector needs to be rendered.
//        /// </summary>
//        public override void OnInspectorGUI()
//        {
//            // Draw the common gizmo properties
//            base.OnInspectorGUI();
//
//            // Let the user control the gizmo axis length
//            EditorGUILayout.Separator();
//            float newFloatValue = EditorGUILayout.FloatField("Axis Length", _translationGizmo.AxisLength);
//            if (newFloatValue != _translationGizmo.AxisLength)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.AxisLength = newFloatValue;
//            }
//
//            // Let the user control the radius of the arrow cones which sit at the tip of each axis
//            newFloatValue = EditorGUILayout.FloatField("Arrow Cone Radius", _translationGizmo.ArrowConeRadius);
//            if (newFloatValue != _translationGizmo.ArrowConeRadius)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.ArrowConeRadius = newFloatValue;
//            }
//
//            // Let the user control the length of the arrow cones which sit at the tip of each axis
//            newFloatValue = EditorGUILayout.FloatField("Arrow Cone Length", _translationGizmo.ArrowConeLength);
//            if (newFloatValue != _translationGizmo.ArrowConeLength)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.ArrowConeLength = newFloatValue;
//            }
//
//            // Let the user specify whether or not the arrow cones must be lit
//            bool newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Are Arrow Cones Lit", _translationGizmo.AreArrowConesLit);
//            if (newBoolValue != _translationGizmo.AreArrowConesLit)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.AreArrowConesLit = newBoolValue;
//            }
//
//            // Let the user change the color of the multi-axis squares
//            EditorGUILayout.Separator();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Construct the multi-axis label text
//                string multiAxisLabelText = ((MultiAxisSquare)multiAxisIndex).ToString() + "Multi Axis Square Color";
//
//                // Let the user change the multi-axis square color
//                Color currentMultiAxisColor = _translationGizmo.GetMultiAxisSquareColor((MultiAxisSquare)multiAxisIndex);
//                Color newMultiAxisColor = EditorGUILayout.ColorField(multiAxisLabelText, currentMultiAxisColor);
//                if (newMultiAxisColor != currentMultiAxisColor)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                    _translationGizmo.SetMultiAxisSquareColor((MultiAxisSquare)multiAxisIndex, newMultiAxisColor);
//                }
//            }
//
//            // Let the user change the color of the selected multi-axis square
//            Color newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Selected Multi Axis Square Color", _translationGizmo.SelectedMultiAxisSquareColor);
//            if (newColorValue != _translationGizmo.SelectedMultiAxisSquareColor)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.SelectedMultiAxisSquareColor = newColorValue;
//            }
//
//            // Let the user change the color of the multi-axis square lines
//            EditorGUILayout.Separator();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Construct the multi-axis label text
//                string multiAxisLabelText = ((MultiAxisSquare)multiAxisIndex).ToString() + "Multi Axis Square Line Color";
//
//                // Let the user change the multi-axis square line color
//                Color currentMultiAxisLineColor = _translationGizmo.GetMultiAxisSquareLineColor((MultiAxisSquare)multiAxisIndex);
//                Color newMultiAxisLineColor = EditorGUILayout.ColorField(multiAxisLabelText, currentMultiAxisLineColor);
//                if (newMultiAxisLineColor != currentMultiAxisLineColor)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                    _translationGizmo.SetMultiAxisSquareLineColor((MultiAxisSquare)multiAxisIndex, newMultiAxisLineColor);
//                }
//            }
//
//            // Let the user change the color of the selected multi-axis square's lines
//            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Selected Multi Axis Square Line Color", _translationGizmo.SelectedMultiAxisSquareLineColor);
//            if (newColorValue != _translationGizmo.SelectedMultiAxisSquareLineColor)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.SelectedMultiAxisSquareLineColor = newColorValue;
//            }
//
//            // Let the user change the multi-axis square size
//            EditorGUILayout.Separator();
//            newFloatValue = EditorGUILayout.FloatField("Multi Axis Square Size", _translationGizmo.MultiAxisSquareSize);
//            if (newFloatValue != _translationGizmo.MultiAxisSquareSize)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.MultiAxisSquareSize = newFloatValue;
//            }
//
//            // Let the user specify whether or not the mulit-axis squares must be adjusted during runtime for better visibility
//            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Adjust Multi Axis For Better Visibility", _translationGizmo.AdjustMultiAxisForBetterVisibility);
//            if (newBoolValue != _translationGizmo.AdjustMultiAxisForBetterVisibility)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.AdjustMultiAxisForBetterVisibility = newBoolValue;
//            }
//
//            // Let the user specify the special op square line color
//            EditorGUILayout.Separator();
//            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Color Of Special Op Square", _translationGizmo.SpecialOpSquareColor);
//            if (newColorValue != _translationGizmo.SpecialOpSquareColor)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.SpecialOpSquareColor = newColorValue;
//            }
//
//            // Let the user specify the special op square line color when the square is selected
//            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Color Of Special Op Square (Selected)", _translationGizmo.SpecialOpSquareColorWhenSelected);
//            if (newColorValue != _translationGizmo.SpecialOpSquareColorWhenSelected)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.SpecialOpSquareColorWhenSelected = newColorValue;
//            }
//
//            // Let the user specify the screen size of the special op square
//            newFloatValue = EditorGUILayout.FloatField(_runtimeOnlyPropertyPrefix + "Screen Size Of Special Op Square", _translationGizmo.ScreenSizeOfSpecialOpSquare);
//            if (newFloatValue != _translationGizmo.ScreenSizeOfSpecialOpSquare)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.ScreenSizeOfSpecialOpSquare = newFloatValue;
//            }
//
//            // Let the user specify the snap step value
//            EditorGUILayout.Separator();
//            newFloatValue = EditorGUILayout.FloatField(_runtimeOnlyPropertyPrefix + "Snap Step Value (In World Units)", _translationGizmo.SnapSettings.StepValueInWorldUnits);
//            if (newFloatValue != _translationGizmo.SnapSettings.StepValueInWorldUnits)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_translationGizmo);
//                _translationGizmo.SnapSettings.StepValueInWorldUnits = newFloatValue;
//            }
//
//            // Make sure that if any color properites have been modified, the changes can be seen immediately in the scene view
//            SceneView.RepaintAll();
//        }
//        #endregion
//
//        #region Protected Methods
//        /// <summary>
//        /// Called when the gizmo is selected in the scene view.
//        /// </summary>
//        protected override void OnEnable()
//        {
//            base.OnEnable();
//            _translationGizmo = target as TranslationGizmo;
//        }
//        #endregion
//    }
//}
//#endif
