using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a class which is responsible for detecing when the user presses
    /// any shortcut keys and taking appropriate action.
    /// </summary>
    public class EditorShortuctKeys : MonoSingletonBase<EditorShortuctKeys>
    {
        #region Private Methods
        /// <summary>
        /// Called every frame to perform any necessary updates.
        /// </summary>
        private void Update()
        {
            HandleEditorGizmoSystemKeys();
            HandleEditorUndoRedoSystemKeys();
            HandleEditorObjectSelectionKeys();
            HandleEditorCameraKeys();
        }

        /// <summary>
        /// This method handles the shortcut keys which are related to the gizmo system.
        /// </summary>
        private void HandleEditorGizmoSystemKeys()
        {
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;

            // Check which shortcut key was pressed and execute the corresponding action
            if (Input.GetKeyDown(KeyCode.W) && !Input.GetMouseButton((int)MouseButton.Right))
            {
                var action = new ActiveGizmoTypeChangeAction(gizmoSystem.ActiveGizmoType, GizmoType.Translation);
                action.Execute();
            }
            else
            if (Input.GetKeyDown(KeyCode.E) && !Input.GetMouseButton((int)MouseButton.Right))
            {
                var action = new ActiveGizmoTypeChangeAction(gizmoSystem.ActiveGizmoType, GizmoType.Rotation);
                action.Execute();
            }
            else
            if (Input.GetKeyDown(KeyCode.R))
            {
                var action = new ActiveGizmoTypeChangeAction(gizmoSystem.ActiveGizmoType, GizmoType.Scale);
                action.Execute();
            }
            else
            if (Input.GetKeyDown(KeyCode.G))
            {
                EditorGizmoSystem.Instance.TransformSpace = TransformSpace.Global;
            }
            else
            if (Input.GetKeyDown(KeyCode.L))
            {
                EditorGizmoSystem.Instance.TransformSpace = TransformSpace.Local;
            }
            else
            if (Input.GetKeyDown(KeyCode.Q) && !Input.GetMouseButton((int)MouseButton.Right))
            {
                var action = new GizmosTurnOffAction(gizmoSystem.ActiveGizmoType);
                action.Execute();
            }
            else
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Toggle the pivot point and execute the action
                TransformPivotPoint newTransformPivotPoint = gizmoSystem.TransformPivotPoint == TransformPivotPoint.Center ? TransformPivotPoint.MeshPivot : TransformPivotPoint.Center;
                EditorGizmoSystem.Instance.TransformPivotPoint = newTransformPivotPoint;
            }

            // Check if any gizmo shorcut keys were pressed. We will do this based on which type of
            // gizmo is currently active.
            GizmoType activeGizmoType = gizmoSystem.ActiveGizmoType;
            if (activeGizmoType == GizmoType.Translation)
            {
//                // If the SHIFT key is pressed, we will activate translation along the camera right and up axes
//                TranslationGizmo translationGizmo = gizmoSystem.TranslationGizmo;
//                bool isShiftPressed = InputHelper.IsAnyShiftKeyPressed();
//                if (isShiftPressed) translationGizmo.TranslateAlongCameraRightAndUpAxes = true;
//                else translationGizmo.TranslateAlongCameraRightAndUpAxes = false;
//
//                // If the CTRL/COMMAND key is pressed, we will activate step snapping
//                bool isCtrlOrCommandPressed = InputHelper.IsAnyCtrlOrCommandKeyPressed();
//                if (isCtrlOrCommandPressed) translationGizmo.SnapSettings.IsStepSnappingEnabled = true;
//                else translationGizmo.SnapSettings.IsStepSnappingEnabled = false;
//
//                // If the 'V' key was pressed in the current frame, we will activate vertex snapping. When
//                // the 'V' key is released in the current frame, we will deactivate vertex snapping.
//                if (Input.GetKeyDown(KeyCode.V))
//                {
//                    translationGizmo.SnapSettings.IsVertexSnappingEnabled = true;
//                    VertexSnappingEnabledMessage.SendToInterestedListeners();
//                }
//                if (Input.GetKeyUp(KeyCode.V))
//                {
//                    translationGizmo.SnapSettings.IsVertexSnappingEnabled = false;
//                    VertexSnappingDisabledMessage.SendToInterestedListeners();
//                }
//
//                if (Input.GetKeyDown(KeyCode.Space)) translationGizmo.PlaceObjectsOnSurface = true;
//
//                Axis alignmentAxis = Axis.Y;
//                if (Input.GetKey(KeyCode.X)) alignmentAxis = Axis.X;
//                else if (Input.GetKey(KeyCode.Z)) alignmentAxis = Axis.Z;
//                translationGizmo.SurfaceAlignmentAxis = alignmentAxis;
//
//                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)) translationGizmo.AlignObjectAxisToSurface = false;
//                else translationGizmo.AlignObjectAxisToSurface = true;
//
//                if (Input.GetKeyUp(KeyCode.Space)) translationGizmo.PlaceObjectsOnSurface = false;
            }
            else
            if (activeGizmoType == GizmoType.Rotation)
            {
                // If the CTRL/COMMAND key is pressed, we will activate snapping
                RotationGizmo rotationGizmo = gizmoSystem.RotationGizmo;
                bool isCtrlOrCommandPressed = InputHelper.IsAnyCtrlOrCommandKeyPressed();
                if (isCtrlOrCommandPressed) rotationGizmo.SnapSettings.IsSnappingEnabled = true;
                else rotationGizmo.SnapSettings.IsSnappingEnabled = false;
            }
            else
            if (activeGizmoType == GizmoType.Scale)
            {
                // If the shift key is pressed, we will activate scaling along all axes at once
//                ScaleGizmo scaleGizmo = gizmoSystem.ScaleGizmo;
//                bool isShiftPressed = InputHelper.IsAnyShiftKeyPressed();
//                if (isShiftPressed) scaleGizmo.ScaleAlongAllAxes = true;
//                else scaleGizmo.ScaleAlongAllAxes = false;
//
//                // If the CTRL/COMMAND key is pressed, we will activate snapping
//                bool isCtrlOrCommandPressed = InputHelper.IsAnyCtrlOrCommandKeyPressed();
//                if (isCtrlOrCommandPressed) scaleGizmo.SnapSettings.IsSnappingEnabled = true;
//                else scaleGizmo.SnapSettings.IsSnappingEnabled = false;
            }
        }

        /// <summary>
        /// This method handles the shortcut keys which are related to the undo/redo system.
        /// </summary>
        private void HandleEditorUndoRedoSystemKeys()
        {
            EditorUndoRedoSystem undoRedoSystem = EditorUndoRedoSystem.Instance;

            // Note: When the application is not running in editor mode, we will use the 
            //       standard shortcut keys for Undo/Redo (CTRL/CMD + Z, CTRL/CMD + Y).
            //       Otherwise, we will add the SHIFT key into the mix in order to stop
            //       the Unity editor from invoking its own Undo/Redo system.
            if (!Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.Z) && InputHelper.IsAnyCtrlOrCommandKeyPressed()) undoRedoSystem.Undo();
                else
                if (Input.GetKeyDown(KeyCode.Y) && InputHelper.IsAnyCtrlOrCommandKeyPressed()) undoRedoSystem.Redo();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Z) && InputHelper.IsAnyCtrlOrCommandKeyPressed() && InputHelper.IsAnyShiftKeyPressed()) undoRedoSystem.Undo();
                else
                if (Input.GetKeyDown(KeyCode.Y) && InputHelper.IsAnyCtrlOrCommandKeyPressed() && InputHelper.IsAnyShiftKeyPressed()) undoRedoSystem.Redo();
            }
        }

        /// <summary>
        /// This method handles the shortcut keys which are related to the editor object selection system.
        /// </summary>
        private void HandleEditorObjectSelectionKeys()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;

            // If any of the CTRL/CMD keys are pressed, we will let the user add objects to the selection. Otherwise,
            // adding to the current selection will be disabled.
            if (InputHelper.IsAnyCtrlOrCommandKeyPressed()) objectSelection.AppendOrDeselectOnClick = true;
            else objectSelection.AppendOrDeselectOnClick = false;

            // If any of the SHIFT keys are pressed, we will let the user deselect multiple object at once using the 
            // object selection shape. Otherwise, multiple deselection will be disabled.
            if (InputHelper.IsAnyShiftKeyPressed()) objectSelection.MultiDeselect = true;
            else objectSelection.MultiDeselect = false;

            if(Input.GetKeyDown(KeyCode.D) && !Input.GetMouseButton((int)MouseButton.Right))
            {
                if((Application.isEditor && Input.GetKey(KeyCode.LeftShift)) || (!Application.isEditor && Input.GetKey(KeyCode.LeftControl)))
                {
                    var action = new ObjectDuplicationAction(new List<GameObject>(EditorObjectSelection.Instance.SelectedGameObjects));
                    action.Execute();
                }
            }
        }

        /// <summary>
        /// This method handles the shortcut keys which are related to the editor camera.
        /// </summary>
        private void HandleEditorCameraKeys()
        {
            EditorCamera editorCamera = EditorCamera.Instance;

            // Focus camera if necessary
            if (Input.GetKeyDown(KeyCode.F)) editorCamera.FocusOnSelection();

            // Handle camera rotation modes
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) editorCamera.RotationMode = EditorCameraRotationMode.Orbit;
            else editorCamera.RotationMode = EditorCameraRotationMode.LookAround;
        }
        #endregion
    }
}
