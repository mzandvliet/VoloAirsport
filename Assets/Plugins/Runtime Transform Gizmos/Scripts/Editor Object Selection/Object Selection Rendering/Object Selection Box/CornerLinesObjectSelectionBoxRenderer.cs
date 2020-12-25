using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class can be used to render object selection boxes for a group
    /// of selected objects using the 'ObjectSelectionBoxStyle.CornerLines' style.
    /// </summary>
    public class CornerLinesObjectSelectionBoxRenderer : ObjectSelectionBoxRenderer
    {
        #region Public Methods
        /// <summary>
        /// Renders the selection boxes for the specified selected game objects.
        /// </summary>
        public override void RenderObjectSelectionBoxes(HashSet<GameObject> selectedObjects)
        {
            // Cache needed data
            EditorObjectSelection editorObjectSelecton = EditorObjectSelection.Instance;
            Material lineRenderingMaterial = MaterialPool.Instance.GLLine;
            ObjectSelectionSettings objectSelectionSettings = editorObjectSelecton.ObjectSelectionSettings;
            ObjectSelectionBoxRenderSettings objectSelectionBoxRenderSettings = objectSelectionSettings.ObjectSelectionBoxRenderSettings;

            // Create the object selection box calculator instance.
            // Note: This can be null if the user has activated the 'Custom' object selection mode
            //       but hasn't specified a selection box calculator.
            ObjectSelectionBoxCalculator objectSelectionBoxCalculator = ObjectSelectionBoxCalculatorFactory.Create(objectSelectionSettings.ObjectSelectionMode);
            if (objectSelectionBoxCalculator != null)
            {
                // Calculate and retrieve the selection boxes and then render them
                List<ObjectSelectionBox> objectSelectionBoxes = objectSelectionBoxCalculator.CalculateForObjectSelection(selectedObjects);
                GLPrimitives.DrawCornerLinesForSelectionBoxes(objectSelectionBoxes, objectSelectionBoxRenderSettings.BoxSizeAdd, objectSelectionBoxRenderSettings.SelectionBoxCornerLinePercentage, 
                                                              EditorCamera.Instance.Camera, objectSelectionBoxRenderSettings.SelectionBoxLineColor, lineRenderingMaterial);
            }  
        }
        #endregion
    }
}
