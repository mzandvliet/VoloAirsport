#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'RotationGizmo' class.
    /// </summary>
    [CustomEditor(typeof(RotationGizmo))]
    public class RotationGizmoInspectorGUI : GizmoInspectorGUIBase
    {
        #region Private Variables
        /// <summary>
        /// Reference to the currently selected rotation gizmo.
        /// </summary>
        private RotationGizmo _rotationGizmo;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Draw the common gizmo properties
            base.OnInspectorGUI();

            // Let the user change the rotation sphere radius
            EditorGUILayout.Separator();
            float newFloatValue = EditorGUILayout.FloatField("Rotation Sphere Radius", _rotationGizmo.RotationSphereRadius);
            if (newFloatValue != _rotationGizmo.RotationSphereRadius)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.RotationSphereRadius = newFloatValue;
            }

            // Let the user change the rotation sphere color
            Color newColorValue = EditorGUILayout.ColorField("Rotation Sphere Color", _rotationGizmo.RotationSphereColor);
            if (newColorValue != _rotationGizmo.RotationSphereColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.RotationSphereColor = newColorValue;
            }

            // Let the user specify whether or not the rotation sphere must be lit
            bool newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Is Rotation Sphere Lit", _rotationGizmo.IsRotationSphereLit);
            if (newBoolValue != _rotationGizmo.IsRotationSphereLit)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.IsRotationSphereLit = newBoolValue;
            }

            // Let the user control the visibility of the rotation guide
            EditorGUILayout.Separator();
            newBoolValue = EditorGUILayout.Toggle("Show Rotation Guide", _rotationGizmo.ShowRotationGuide);
            if (newBoolValue != _rotationGizmo.ShowRotationGuide)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.ShowRotationGuide = newBoolValue;
            }

            // Let the user control the rotation guide line color
            newColorValue = EditorGUILayout.ColorField("Rotation Guide Line Color", _rotationGizmo.RotationGuieLineColor);
            if (newColorValue != _rotationGizmo.RotationGuieLineColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.RotationGuieLineColor = newColorValue;
            }

            // Let the user control the color of the rotation guide disc
            newColorValue = EditorGUILayout.ColorField("Rotation Guide Disc Color", _rotationGizmo.RotationGuideDiscColor);
            if (newColorValue != _rotationGizmo.RotationGuideDiscColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.RotationGuideDiscColor = newColorValue;
            }

            // Let the user control the visibility of the rotation sphere boundary
            EditorGUILayout.Separator();
            newBoolValue = EditorGUILayout.Toggle(_runtimeOnlyPropertyPrefix + "Show Rotation Sphere Boundary", _rotationGizmo.ShowSphereBoundary);
            if (newBoolValue != _rotationGizmo.ShowSphereBoundary)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.ShowSphereBoundary = newBoolValue;
            }

            // Let the user control the rotation sphere boundary line color
            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Rotation Sphere Boundary Line Color", _rotationGizmo.SphereBoundaryLineColor);
            if (newColorValue != _rotationGizmo.SphereBoundaryLineColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.SphereBoundaryLineColor = newColorValue;
            }

            // Let the user control the visibility of the camera look rotation circle
            EditorGUILayout.Separator();
            newBoolValue = EditorGUILayout.Toggle("Show Camera Look Rotation Circle", _rotationGizmo.ShowCameraLookRotationCircle);
            if (newBoolValue != _rotationGizmo.ShowCameraLookRotationCircle)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.ShowCameraLookRotationCircle = newBoolValue;
            }

            // Let the user control the camera look rotation circle scale
            newFloatValue = EditorGUILayout.FloatField("Camera Look Rotation Circle Radius Scale", _rotationGizmo.CameraLookRotationCircleRadiusScale);
            if (newFloatValue != _rotationGizmo.CameraLookRotationCircleRadiusScale)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.CameraLookRotationCircleRadiusScale = newFloatValue;
            }

            // Let the user control the camera look rotation circle line color
            newColorValue = EditorGUILayout.ColorField("Camera Look Rotation Circle Line Color", _rotationGizmo.CameraLookRotationCircleLineColor);
            if (newColorValue != _rotationGizmo.SphereBoundaryLineColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.CameraLookRotationCircleLineColor = newColorValue;
            }

            // Let the user control the color of the camera look rotation circle when it is selected
            newColorValue = EditorGUILayout.ColorField(_runtimeOnlyPropertyPrefix + "Camera Look Rotation Circle Color (Selected)", _rotationGizmo.CameraLookRotationCircleColorWhenSelected);
            if (newColorValue != _rotationGizmo.CameraLookRotationCircleColorWhenSelected)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.CameraLookRotationCircleColorWhenSelected = newColorValue;
            }

            // Let the user specify the snap step value
            EditorGUILayout.Separator();
            newFloatValue = EditorGUILayout.FloatField(_runtimeOnlyPropertyPrefix + "Snap Step Value (In Degrees)", _rotationGizmo.SnapSettings.StepValueInDegrees);
            if(newFloatValue != _rotationGizmo.SnapSettings.StepValueInDegrees)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_rotationGizmo);
                _rotationGizmo.SnapSettings.StepValueInDegrees = newFloatValue;
            }

            // Make sure that if any color properites have been modified, the changes can be seen immediately in the scene view
            SceneView.RepaintAll();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the gizmo is selected in the scene view.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _rotationGizmo = target as RotationGizmo;
        }
        #endregion
    }
}
#endif
