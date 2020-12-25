#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Custom inspector implementation for the 'EditorObjectSelection' class.
    /// </summary>
    [CustomEditor(typeof(EditorObjectSelection))]
    public class EditorObjectSelectionInspectorGUI : Editor
    {
        #region Private Static Variables
        /// <summary>
        /// The following variables control the visibility for different categories of settings.
        /// </summary>
        private static bool _objectSelectionBoxRenderSettingsAreVisible = true;
        private static bool _objectSelectionRectangleDrawSettingsAreVisible = true;
        private static bool _selectableLayersListIsVisible = true;
        #endregion

        #region Private Variables
        /// <summary>
        /// Reference to the editor object selection module.
        /// </summary>
        private EditorObjectSelection _editorObjectSelection;
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when the inspector needs to be rendered.
        /// </summary>
        public override void OnInspectorGUI()
        {
            const int indentLevel = 1;
            Color newColor;
            ObjectSelectionSettings objectSelectionSettings = _editorObjectSelection.ObjectSelectionSettings;

            bool newBool = EditorGUILayout.ToggleLeft("Can Select Terrain Objects", objectSelectionSettings.CanSelectTerrainObjects);
            if(newBool != objectSelectionSettings.CanSelectTerrainObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.CanSelectTerrainObjects = newBool;
            }

            newBool = EditorGUILayout.ToggleLeft("Can Select Light Objects", objectSelectionSettings.CanSelectLightObjects);
            if (newBool != objectSelectionSettings.CanSelectLightObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.CanSelectLightObjects = newBool;
            }

            newBool = EditorGUILayout.ToggleLeft("Can Select Particle System Objects", objectSelectionSettings.CanSelectParticleSystemObjects);
            if (newBool != objectSelectionSettings.CanSelectParticleSystemObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.CanSelectParticleSystemObjects = newBool;
            }

            newBool = EditorGUILayout.ToggleLeft("Can Select Sprite Objects", objectSelectionSettings.CanSelectSpriteObjects);
            if (newBool != objectSelectionSettings.CanSelectSpriteObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.CanSelectSpriteObjects = newBool;
            }

            newBool = EditorGUILayout.ToggleLeft("Can Select Empty Objects", objectSelectionSettings.CanSelectEmptyObjects);
            if (newBool != objectSelectionSettings.CanSelectEmptyObjects)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.CanSelectEmptyObjects = newBool;
            }

            // Let the user specify the selectable layers
            _selectableLayersListIsVisible = EditorGUILayout.Foldout(_selectableLayersListIsVisible, "Selectable Layers");
            if(_selectableLayersListIsVisible)
            {
                EditorGUI.indentLevel += indentLevel;

                // Show all available layer names and let the user add/remove layers using toggle buttons
                List<string> allLayerNames = LayerHelper.GetAllLayerNames();
                foreach (string layerName in allLayerNames)
                {
                    int layerNumber = LayerMask.NameToLayer(layerName);
                    bool isSelectable = LayerHelper.IsLayerBitSet(objectSelectionSettings.SelectableLayers, layerNumber);

                    newBool = EditorGUILayout.ToggleLeft(layerName, isSelectable);
                    if (newBool != isSelectable)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                        if (isSelectable) objectSelectionSettings.SelectableLayers = LayerHelper.ClearLayerBit(objectSelectionSettings.SelectableLayers, layerNumber);
                        else objectSelectionSettings.SelectableLayers = LayerHelper.SetLayerBit(objectSelectionSettings.SelectableLayers, layerNumber);
                    }
                }

                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user specify the object selection mode
            EditorGUILayout.Separator();
            ObjectSelectionMode newObjectSelectionMode = (ObjectSelectionMode)EditorGUILayout.EnumPopup("Selection Mode", objectSelectionSettings.ObjectSelectionMode);
            if(newObjectSelectionMode != objectSelectionSettings.ObjectSelectionMode)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.ObjectSelectionMode = newObjectSelectionMode;
            }

            // Let the user specify if any selected objects must be deselected when the selection mechanism is disabled
            bool newBoolValue = EditorGUILayout.ToggleLeft("Deselect Objects When Disabled", objectSelectionSettings.DeselectObjectsWhenDisabled);
            if(newBoolValue != objectSelectionSettings.DeselectObjectsWhenDisabled)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                objectSelectionSettings.DeselectObjectsWhenDisabled = newBoolValue;
            }

            // Let the user modify the object selection box render settings
            EditorGUILayout.Separator();
            _objectSelectionBoxRenderSettingsAreVisible = EditorGUILayout.Foldout(_objectSelectionBoxRenderSettingsAreVisible, "Selection Box Render Settings");
            if(_objectSelectionBoxRenderSettingsAreVisible)
            {
                EditorGUI.indentLevel += indentLevel;
                ObjectSelectionBoxRenderSettings objectSelectionBoxDrawSettings = objectSelectionSettings.ObjectSelectionBoxRenderSettings;

                newBool = EditorGUILayout.ToggleLeft("Draw Selection Boxes", objectSelectionBoxDrawSettings.DrawBoxes);
                if(newBool != objectSelectionBoxDrawSettings.DrawBoxes)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.DrawBoxes = newBool;
                }

                // Let the user choose the object selection box style
                ObjectSelectionBoxStyle newObjectSelectionBoxStyle = (ObjectSelectionBoxStyle)EditorGUILayout.EnumPopup("Selection Box Style", objectSelectionBoxDrawSettings.SelectionBoxStyle);
                if(newObjectSelectionBoxStyle != objectSelectionBoxDrawSettings.SelectionBoxStyle)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.SelectionBoxStyle = newObjectSelectionBoxStyle;
                }

                // If the object selection box style is set to 'CornerLines', let the user choose the length of the corner lines
                float newFloatValue;
                if(objectSelectionBoxDrawSettings.SelectionBoxStyle == ObjectSelectionBoxStyle.CornerLines)
                {
                    newFloatValue = EditorGUILayout.FloatField("Corner Line Percentage", objectSelectionBoxDrawSettings.SelectionBoxCornerLinePercentage);
                    if(newFloatValue != objectSelectionBoxDrawSettings.SelectionBoxCornerLinePercentage)
                    {
                        UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                        objectSelectionBoxDrawSettings.SelectionBoxCornerLinePercentage = newFloatValue;
                    }
                }

                // Let the user choose the selection box line color
                Color newColorValue = EditorGUILayout.ColorField("Selection Box Line Color", objectSelectionBoxDrawSettings.SelectionBoxLineColor);
                if(newColorValue != objectSelectionBoxDrawSettings.SelectionBoxLineColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.SelectionBoxLineColor = newColorValue;
                }

                // Let the user choose the selection box size add value
                newFloatValue = EditorGUILayout.FloatField("Selection Box Size Add", objectSelectionBoxDrawSettings.BoxSizeAdd);
                if(newFloatValue != objectSelectionBoxDrawSettings.BoxSizeAdd)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionBoxDrawSettings.BoxSizeAdd = newFloatValue;
                }
                EditorGUI.indentLevel -= indentLevel;
            }

            // Let the user modify the object selection rectangle render settings
            EditorGUILayout.Separator();            
            _objectSelectionRectangleDrawSettingsAreVisible = EditorGUILayout.Foldout(_objectSelectionRectangleDrawSettingsAreVisible, "Selection Rectangle Render Settings");
            if(_objectSelectionRectangleDrawSettingsAreVisible)
            {
                EditorGUI.indentLevel += indentLevel;

                // Let the user modify the object selection border line color
                ObjectSelectionRectangleRenderSettings objectSelectionRectangleDrawSettings = _editorObjectSelection.ObjectSelectionRectangleRenderSettings;
                newColor = EditorGUILayout.ColorField("Border Line Color", objectSelectionRectangleDrawSettings.BorderLineColor);
                if(newColor != objectSelectionRectangleDrawSettings.BorderLineColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionRectangleDrawSettings.BorderLineColor = newColor;
                }

                // Let the user modify the object selection rectangle fill color
                newColor = EditorGUILayout.ColorField("Fill Color", objectSelectionRectangleDrawSettings.FillColor);
                if(newColor != objectSelectionRectangleDrawSettings.FillColor)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(_editorObjectSelection);
                    objectSelectionRectangleDrawSettings.FillColor = newColor;
                }

                EditorGUI.indentLevel -= indentLevel;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Called when the editor object selection object is selected in the scene view.
        /// </summary>
        protected virtual void OnEnable()
        {
            _editorObjectSelection = target as EditorObjectSelection;
        }
        #endregion
    }
}
#endif
