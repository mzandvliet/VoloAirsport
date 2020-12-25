#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'Gizmo' base class.
    /// </summary>
    [CustomEditor(typeof(Gizmo))]
    public class GizmoInspectorGUIBase : Editor
    {
        #region Private Variables
        /// <summary>
        /// Reference to the currently selected gizmo.
        /// </summary>
        private Gizmo _gizmo;
        #endregion

        #region Protected Variables
        /// <summary>
        /// This is a prefix that is added to all gizmo properties whose effect
        /// can only be checked at runtime.
        /// </summary>
        protected string _runtimeOnlyPropertyPrefix = "*";
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Inform the user that some gizmo properties can only be verified during runtime
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Note: Properties prefixed by \'" + _runtimeOnlyPropertyPrefix + "\' can only be verified at runtime.", EditorGUIStyles.GetInformativeLabelStyle());

            // Allow the user to specify the gizmo base scale
            float newFloatValue = EditorGUILayout.FloatField("Gizmo Base Scale", _gizmo.GizmoBaseScale);
            if (newFloatValue != _gizmo.GizmoBaseScale)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                _gizmo.GizmoBaseScale = newFloatValue;
            }

            // Allow the user to specify if whether or not the size of the gizmo must be preserved in screen space
            bool newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Preserve Gizmo Screen Size", _gizmo.PreserveGizmoScreenSize);
            if (newBoolValue != _gizmo.PreserveGizmoScreenSize)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                _gizmo.PreserveGizmoScreenSize = newBoolValue;
            }

            // Loop through each axis and let the user modify their colors
            EditorGUILayout.Separator();
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                // Construct the text used to draw the axis label
                string axisLabelText = ((GizmoAxis)axisIndex).ToString() + " Axis Color";

                // Allow the user to change the color
                Color currentAxisColor = _gizmo.GetAxisColor((GizmoAxis)axisIndex);
                Color newAxisColor = EditorGUILayout.ColorField(axisLabelText, currentAxisColor);
                if (newAxisColor != currentAxisColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                    _gizmo.SetAxisColor((GizmoAxis)axisIndex, newAxisColor);
                }
            }

            // Allow the user to choose the color which must be used to draw the currently selected axis
            Color newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Selected Axis Color", _gizmo.SelectedAxisColor);
            if (newColorValue != _gizmo.SelectedAxisColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_gizmo);
                _gizmo.SelectedAxisColor = newColorValue;
            }

            // Make sure that if any color properites have been modified, the changes can be seen immediately in the scene view
            SceneView.RepaintAll();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the gizmo is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _gizmo = target as Gizmo;
        }
        #endregion
    }
}
#endif
