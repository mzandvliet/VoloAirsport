//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//
//namespace RTEditor
//{
//    /// <summary>
//    /// Custom inspector implementation for the 'ScaleGizmo' class.
//    /// </summary>
//    [CustomEditor(typeof(ScaleGizmo))]
//    public class ScaleGizmoInspectorGUI : GizmoInspectorGUIBase
//    {
//        #region Private Variables
//        /// <summary>
//        /// Reference to the currently selected scale gizmo.
//        /// </summary>
//        private ScaleGizmo _scaleGizmo;
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
//            float newFloatValue = EditorGUILayout.FloatField("Axis Length", _scaleGizmo.AxisLength);
//            if (newFloatValue != _scaleGizmo.AxisLength)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.AxisLength = newFloatValue;
//            }
//
//            // Let the user control the scale box width
//            newFloatValue = EditorGUILayout.FloatField("Scale Box Width", _scaleGizmo.ScaleBoxWidth);
//            if (newFloatValue != _scaleGizmo.ScaleBoxWidth)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.ScaleBoxWidth = newFloatValue;
//            }
//
//            // Let the user control the scale box height
//            newFloatValue = EditorGUILayout.FloatField("Scale Box Height", _scaleGizmo.ScaleBoxHeight);
//            if (newFloatValue != _scaleGizmo.ScaleBoxHeight)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.ScaleBoxHeight = newFloatValue;
//            }
//
//            // Let the user control the scale box depth
//            newFloatValue = EditorGUILayout.FloatField("Scale Box Depth", _scaleGizmo.ScaleBoxDepth);
//            if (newFloatValue != _scaleGizmo.ScaleBoxDepth)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.ScaleBoxDepth = newFloatValue;
//            }
//
//            // Let the user specify whether or not the scale boxes must be lit
//            bool newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Are Scale Boxes Lit", _scaleGizmo.AreScaleBoxesLit);
//            if (newBoolValue != _scaleGizmo.AreScaleBoxesLit)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.AreScaleBoxesLit = newBoolValue;
//            }
//
//            // Let the user specify whether or not the length of the scale axes must be adjusted while performing a scale operation
//            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Adjust Axis Length While Scaling Objects", _scaleGizmo.AdjustAxisLengthWhileScalingObjects);
//            if (newBoolValue != _scaleGizmo.AdjustAxisLengthWhileScalingObjects)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.AdjustAxisLengthWhileScalingObjects = newBoolValue;
//            }
//
//            // Let the user change the color of the multi-axis triangles
//            EditorGUILayout.Separator();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Construct the multi-axis label text
//                string multiAxisLabelText = ((MultiAxisTriangle)multiAxisIndex).ToString() + "Multi Axis Triangle Color";
//
//                // Let the user change the multi-axis triangle color
//                Color currentMultiAxisColor = _scaleGizmo.GetMultiAxisTriangleColor((MultiAxisTriangle)multiAxisIndex);
//                Color newMultiAxisColor = EditorGUILayout.ColorField(multiAxisLabelText, currentMultiAxisColor);
//                if (newMultiAxisColor != currentMultiAxisColor)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                    _scaleGizmo.SetMultiAxisTriangleColor((MultiAxisTriangle)multiAxisIndex, newMultiAxisColor);
//                }
//            }
//
//            // Let the user change the color of the selected multi-axis triangle
//            Color newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Selected Multi Axis Triangle Color", _scaleGizmo.SelectedMultiAxisTriangleColor);
//            if (newColorValue != _scaleGizmo.SelectedMultiAxisTriangleColor)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.SelectedMultiAxisTriangleColor = newColorValue;
//            }
//
//            // Let the user change the color of the multi-axis triangle lines
//            EditorGUILayout.Separator();
//            for (int multiAxisIndex = 0; multiAxisIndex < 3; ++multiAxisIndex)
//            {
//                // Construct the multi-axis label text
//                string multiAxisLabelText = ((MultiAxisTriangle)multiAxisIndex).ToString() + "Multi Axis Triangle Line Color";
//
//                // Let the user change the multi-axis triangle line color
//                Color currentMultiAxisLineColor = _scaleGizmo.GetMultiAxisTriangleLineColor((MultiAxisTriangle)multiAxisIndex);
//                Color newMultiAxisLineColor = EditorGUILayout.ColorField(multiAxisLabelText, currentMultiAxisLineColor);
//                if (newMultiAxisLineColor != currentMultiAxisLineColor)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                    _scaleGizmo.SetMultiAxisTriangleLineColor((MultiAxisTriangle)multiAxisIndex, newMultiAxisLineColor);
//                }
//            }
//
//            // Let the user change the color of the selected multi-axis triangle's lines
//            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Selected Multi Axis Triangle Line Color", _scaleGizmo.SelectedMultiAxisTriangleLineColor);
//            if (newColorValue != _scaleGizmo.SelectedMultiAxisTriangleLineColor)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.SelectedMultiAxisTriangleLineColor = newColorValue;
//            }
//
//            // Let the user change the length of the adjacent sides of the multi-axis triangles
//            EditorGUILayout.Separator();
//            newFloatValue = EditorGUILayout.FloatField("Multi Axis Triangle Side Length", _scaleGizmo.MultiAxisTriangleSideLength);
//            if (newFloatValue != _scaleGizmo.MultiAxisTriangleSideLength)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.MultiAxisTriangleSideLength = newFloatValue;
//            }
//
//            // Let the user specify whether or not the mulit-axis triangles must be adjusted during runtime for better visibility
//            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Adjust Multi Axis For Better Visibility", _scaleGizmo.AdjustMultiAxisForBetterVisibility);
//            if (newBoolValue != _scaleGizmo.AdjustMultiAxisForBetterVisibility)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.AdjustMultiAxisForBetterVisibility = newBoolValue;
//            }
//
//            // Let the user specify whether or not the multi-axis triangles must have their area adjusted during a scale operation
//            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Adjust Multi Axis Triangles While Scaling Objects", _scaleGizmo.AdjustMultiAxisTrianglesWhileScalingObjects);
//            if (newBoolValue != _scaleGizmo.AdjustMultiAxisTrianglesWhileScalingObjects)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.AdjustMultiAxisTrianglesWhileScalingObjects = newBoolValue;
//            }
//
//            // Let the user specify the color of the square which can be used to scale along all axes at once
//            EditorGUILayout.Separator();
//            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Color Of All-Axes Square Lines", _scaleGizmo.ColorOfAllAxesSquareLines);
//            if (newColorValue != _scaleGizmo.ColorOfAllAxesSquareLines)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.ColorOfAllAxesSquareLines = newColorValue;
//            }
//
//            // Let the user specify the color of the square which can be used to scale along all axes at once when it is selected
//            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Color Of All-Axes Square Lines (Selected)", _scaleGizmo.ColorOfAllAxesSquareLinesWhenSelected);
//            if (newColorValue != _scaleGizmo.ColorOfAllAxesSquareLinesWhenSelected)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.ColorOfAllAxesSquareLinesWhenSelected = newColorValue;
//            }
//
//            // Let the user specify the screen size of the square which can be used to scale along all axes at once
//            newFloatValue = EditorGUILayout.FloatField(_runtimeOnlyPropertyPrefix + "Screen Size Of All-Axes Square", _scaleGizmo.ScreenSizeOfAllAxesSquare);
//            if (newFloatValue != _scaleGizmo.ScreenSizeOfAllAxesSquare)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.ScreenSizeOfAllAxesSquare = newFloatValue;
//            }
//
//            // Let the user specify whether or not the all-axes scale square must have its size adjusted during a scale operation
//            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Adjust All-Axes Square While Scaling Objects", _scaleGizmo.AdjustAllAxesScaleSquareWhileScalingObjects);
//            if (newBoolValue != _scaleGizmo.AdjustAllAxesScaleSquareWhileScalingObjects)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.AdjustAllAxesScaleSquareWhileScalingObjects = newBoolValue;
//            }
//
//            // Let the user specify whether or not the objects local axes must be drawn during a scale operation
//            EditorGUILayout.Separator();
//            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Draw Objects Local Axes While Scaling", _scaleGizmo.DrawObjectsLocalAxesWhileScaling);
//            if (newBoolValue != _scaleGizmo.DrawObjectsLocalAxesWhileScaling)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.DrawObjectsLocalAxesWhileScaling = newBoolValue;
//            }
//
//            // If the objects local coordinate system axes must be drawn, we have some more controls to draw
//            if (_scaleGizmo.DrawObjectsLocalAxesWhileScaling)
//            {
//                // Let the user choose the objects' local axis length
//                newFloatValue = EditorGUILayout.FloatField(_runtimeOnlyPropertyPrefix + "Objects Local Axes Length", _scaleGizmo.ObjectsLocalAxesLength);
//                if (newFloatValue != _scaleGizmo.ObjectsLocalAxesLength)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                    _scaleGizmo.ObjectsLocalAxesLength = newFloatValue;
//                }
//
//                // Let the user specify whether or not the objects' local axes must have their size preserved in screen space
//                newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Preserve Axes Screen Size", _scaleGizmo.PreserveObjectLocalAxesScreenSize);
//                if (newBoolValue != _scaleGizmo.PreserveObjectLocalAxesScreenSize)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                    _scaleGizmo.PreserveObjectLocalAxesScreenSize = newBoolValue;
//                }
//
//                // Let the user specify whether or not the objects' local axes must be scaled along with the objects
//                newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Adjust Object Local Axes While Scaling", _scaleGizmo.AdjustObjectLocalAxesWhileScalingObjects);
//                if (newBoolValue != _scaleGizmo.AdjustObjectLocalAxesWhileScalingObjects)
//                {
//                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                    _scaleGizmo.AdjustObjectLocalAxesWhileScalingObjects = newBoolValue;
//                }
//            }
//
//            // Let the user specify the snap step value
//            EditorGUILayout.Separator();
//            newFloatValue = EditorGUILayout.FloatField(_runtimeOnlyPropertyPrefix + "Snap Step Value (In World Units)", _scaleGizmo.SnapSettings.StepValueInWorldUnits);
//            if (newFloatValue != _scaleGizmo.SnapSettings.StepValueInWorldUnits)
//            {
//                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_scaleGizmo);
//                _scaleGizmo.SnapSettings.StepValueInWorldUnits = newFloatValue;
//            }
//
//            // Make sure that if any color properites have been modified, the changes can be seen immediately in the scene view
//            SceneView.RepaintAll();
//        }
//        #endregion
//
//        #region Private Methods
//        /// <summary>
//        /// Called when the gizmo is selected in the scene view.
//        /// </summary>
//        protected override void OnEnable()
//        {
//            base.OnEnable();
//            _scaleGizmo = target as ScaleGizmo;
//        }
//        #endregion
//    }
//}
//#endif