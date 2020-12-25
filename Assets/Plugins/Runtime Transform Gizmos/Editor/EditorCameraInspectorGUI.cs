#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorCamera' class.
    /// </summary>
    [CustomEditor(typeof(EditorCamera))]
    public class EditorCameraInspectorGUI : Editor
    {
        #region Private Variables
        /// <summary>
        /// Reference to the currently selected editor camera.
        /// </summary>
        private EditorCamera _editorCamera;

        /// <summary>
        /// The following variables control the visibility for different categories of settings.
        /// </summary>
        private static bool _zoomSettingsAreVisible = true;
        private static bool _panSettingsAreVisible = true;
        private static bool _focusSettingsAreVisible = true;
        private static bool _bkSettingsAreVisible = true;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            float newFloatValue;
            const int indentLevel = 1;

            bool newBool = EditorGUILayout.ToggleLeft("Take Zoom Factor Into Account", _editorCamera.TakeZoomFactorIntoAccount);
            if (newBool != _editorCamera.TakeZoomFactorIntoAccount)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                _editorCamera.TakeZoomFactorIntoAccount = newBool;
            }

            // Let the user change the zoom settings
            _zoomSettingsAreVisible = EditorGUILayout.Foldout(_zoomSettingsAreVisible, "Zoom Settings");
            if(_zoomSettingsAreVisible)
            {
                EditorCameraZoomSettings zoomSettings = _editorCamera.ZoomSettings;
                EditorGUI.indentLevel += indentLevel;

                // Let the user specify if camera zoom is enabled/disabled
                newBool = EditorGUILayout.ToggleLeft("Is Zoom Enabled", zoomSettings.IsZoomEnabled);
                if(newBool != zoomSettings.IsZoomEnabled)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    zoomSettings.IsZoomEnabled = newBool;
                }

                // Let the user specify the camera zoom mode
                EditorCameraZoomMode newZoomMode = (EditorCameraZoomMode)EditorGUILayout.EnumPopup("Zoom Mode", zoomSettings.ZoomMode);
                if (newZoomMode != zoomSettings.ZoomMode)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    zoomSettings.ZoomMode = newZoomMode;
                }

                if(newZoomMode == EditorCameraZoomMode.Smooth)
                {
                    // Let the user choose a smooth value for both camera types
                    EditorGUILayout.Separator();
                    newFloatValue = EditorGUILayout.Slider("Orthographic Smooth Value", zoomSettings.OrthographicSmoothValue, EditorCameraZoomSettings.MinSmoothValue, EditorCameraZoomSettings.MaxSmoothValue);
                    if (newFloatValue != zoomSettings.OrthographicSmoothValue)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        zoomSettings.OrthographicSmoothValue = newFloatValue;
                    }

                    newFloatValue = EditorGUILayout.Slider("Perspective Smooth Value", zoomSettings.PerspectiveSmoothValue, EditorCameraZoomSettings.MinSmoothValue, EditorCameraZoomSettings.MaxSmoothValue);
                    if (newFloatValue != zoomSettings.PerspectiveSmoothValue)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        zoomSettings.PerspectiveSmoothValue = newFloatValue;
                    }
                }
             
                if (newZoomMode == EditorCameraZoomMode.Standard)
                {
                    // Let the user specify the zoom speed when the camera operates in ortho mode and the zoom mode is set to 'Standard'
                    EditorGUILayout.Separator();
                    newFloatValue = EditorGUILayout.FloatField("Orthographic Standard Zoom Speed", zoomSettings.OrthographicStandardZoomSpeed);
                    if (newFloatValue != zoomSettings.OrthographicStandardZoomSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        zoomSettings.OrthographicStandardZoomSpeed = newFloatValue;
                    }

                    // Let the user specify the zoom speed when the camera operates in perspective mode and the zoom mode is set to 'Standard'
                    newFloatValue = EditorGUILayout.FloatField("Perspective Standard Zoom Speed", zoomSettings.PerspectiveStandardZoomSpeed);
                    if (newFloatValue != zoomSettings.PerspectiveStandardZoomSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        zoomSettings.PerspectiveStandardZoomSpeed = newFloatValue;
                    }
                }

                if (newZoomMode == EditorCameraZoomMode.Smooth)
                {
                    // Let the user specify the zoom speed when the camera operates in ortho mode and the zoom mode is set to 'Smooth'
                    EditorGUILayout.Separator();
                    newFloatValue = EditorGUILayout.FloatField("Orthographic Smooth Zoom Speed", zoomSettings.OrthographicSmoothZoomSpeed);
                    if (newFloatValue != zoomSettings.OrthographicSmoothZoomSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        zoomSettings.OrthographicSmoothZoomSpeed = newFloatValue;
                    }

                    // Let the user specify the zoom speed when the camera operates in perspective mode and the zoom mode is set to 'Smooth'
                    newFloatValue = EditorGUILayout.FloatField("Perspective Smooth Zoom Speed", zoomSettings.PerspectiveSmoothZoomSpeed);
                    if (newFloatValue != zoomSettings.PerspectiveSmoothZoomSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        zoomSettings.PerspectiveSmoothZoomSpeed = newFloatValue;
                    }
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user change the pan settings
            _panSettingsAreVisible = EditorGUILayout.Foldout(_panSettingsAreVisible, "Pan Settings");
            if(_panSettingsAreVisible)
            {
                EditorCameraPanSettings panSettings = _editorCamera.PanSettings;
                EditorGUI.indentLevel += indentLevel;

                // Let the user choose the pan mode
                EditorCameraPanMode newPanMode = (EditorCameraPanMode)EditorGUILayout.EnumPopup("Pan Mode", panSettings.PanMode);
                if(newPanMode != panSettings.PanMode)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    panSettings.PanMode = newPanMode;
                }

                if(panSettings.PanMode == EditorCameraPanMode.Smooth)
                {
                    // Let the user choose the pan smooth value
                    EditorGUILayout.Separator();
                    newFloatValue = EditorGUILayout.Slider("Smooth Value", panSettings.SmoothValue, EditorCameraPanSettings.MinSmoothValue, EditorCameraPanSettings.MaxSmoothValue);
                    if(newFloatValue != panSettings.SmoothValue)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        panSettings.SmoothValue = newFloatValue;
                    }

                    // Let the user choose the smooth pan speed
                    newFloatValue = EditorGUILayout.FloatField("Smooth Pan Speed", panSettings.SmoothPanSpeed);
                    if (newFloatValue != panSettings.SmoothPanSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        panSettings.SmoothPanSpeed = newFloatValue;
                    }
                }
                else
                {
                    // Let the user choose the standard pan speed
                    newFloatValue = EditorGUILayout.FloatField("Standard Pan Speed", panSettings.StandardPanSpeed);
                    if (newFloatValue != panSettings.StandardPanSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        panSettings.StandardPanSpeed = newFloatValue;
                    }
                }

                // Let the user specify which pan axes should be inverted
                EditorGUILayout.Separator();
                bool newBoolean = EditorGUILayout.ToggleLeft("Invert X Axis", panSettings.InvertXAxis);
                if(newBoolean != panSettings.InvertXAxis)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    panSettings.InvertXAxis = newBoolean;
                }

                newBoolean = EditorGUILayout.ToggleLeft("Invert Y Axis", panSettings.InvertYAxis);
                if (newBoolean != panSettings.InvertYAxis)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    panSettings.InvertYAxis = newBoolean;
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user change the focus settings
            _focusSettingsAreVisible = EditorGUILayout.Foldout(_focusSettingsAreVisible, "Focus Settings");
            if(_focusSettingsAreVisible)
            {
                EditorCameraFocusSettings focusSettings = _editorCamera.FocusSettings;
                EditorGUI.indentLevel += indentLevel;

                // Let the user choose the focus mode
                EditorCameraFocusMode newFocusMode = (EditorCameraFocusMode)EditorGUILayout.EnumPopup("Focus Mode", focusSettings.FocusMode);
                if(newFocusMode != focusSettings.FocusMode)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    focusSettings.FocusMode = newFocusMode;
                }

                
                // Continue drawing the GUI based on the active focus mode
                if(focusSettings.FocusMode == EditorCameraFocusMode.ConstantSpeed)
                {
                    // Let the user specify the constant focus speed
                    EditorGUILayout.Separator();
                    newFloatValue = EditorGUILayout.FloatField("Constant Focus Speed", focusSettings.ConstantFocusSpeed);
                    if(newFloatValue != focusSettings.ConstantFocusSpeed)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        focusSettings.ConstantFocusSpeed = newFloatValue;
                    }
                }
                else
                if(focusSettings.FocusMode == EditorCameraFocusMode.Smooth)
                {
                    // Let the user specify the smooth focus time
                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("Note: Time value is approximate.", EditorGUIStyles.GetInformativeLabelStyle());
                    newFloatValue = EditorGUILayout.FloatField("Smooth Focus Time", focusSettings.SmoothFocusTime);
                    if (newFloatValue != focusSettings.SmoothFocusTime)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                        focusSettings.SmoothFocusTime = newFloatValue;
                    }
                }

                // We always let the user choose the focus distance scale
                newFloatValue = EditorGUILayout.FloatField("Focus Distance Scale", focusSettings.FocusDistanceScale);
                if(newFloatValue != focusSettings.FocusDistanceScale)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                    focusSettings.FocusDistanceScale = newFloatValue;
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user specify the camera rotation speed in degree units
            EditorGUILayout.Separator();
            newFloatValue = EditorGUILayout.FloatField("Rotation Speed (Degrees)", _editorCamera.RotationSpeedInDegrees);
            if (newFloatValue != _editorCamera.RotationSpeedInDegrees)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                _editorCamera.RotationSpeedInDegrees = newFloatValue;
            }

            // Let the user specify the camera move spedd in units/second
            newFloatValue = EditorGUILayout.FloatField("Move Speed (units/second)", _editorCamera.MoveSettings.MoveSpeed);
            if (newFloatValue != _editorCamera.MoveSettings.MoveSpeed)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorCamera);
                _editorCamera.MoveSettings.MoveSpeed = newFloatValue;
            }

            _bkSettingsAreVisible = EditorGUILayout.Foldout(_bkSettingsAreVisible, "Background Settings");
            if (_bkSettingsAreVisible)
            {
                EditorGUI.indentLevel += 1;
                _editorCamera.Background.RenderView(_editorCamera);
                EditorGUI.indentLevel -= 1;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the editor camera is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _editorCamera = target as EditorCamera;
        }
        #endregion
    }
}
#endif
