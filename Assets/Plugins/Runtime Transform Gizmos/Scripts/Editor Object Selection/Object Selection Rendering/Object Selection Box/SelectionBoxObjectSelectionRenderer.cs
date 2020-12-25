using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class implements the object selection rendering mechanism by rendering
    /// a selection box for each selected game object.
    /// </summary>
    public class SelectionBoxObjectSelectionRenderer : ObjectSelectionRenderer
    {
        #region Public Methods
        /// <summary>
        /// Renders the specified selected objects by rendering a selection box for each game object.
        /// </summary>
        /// <param name="objectSelectionSettings">
        /// Needed to have access to all the settings which are required for the rendering operation.
        /// </param>
        public override void RenderObjectSelection(HashSet<GameObject> selectedObjects, ObjectSelectionSettings objectSelectionSettings)
        {
            // Render the object selection boxes
            ObjectSelectionBoxRenderer objectSelectionBoxRenderer = ObjectSelectionBoxRendererFactory.CreateObjectSelectionBoxDrawer(objectSelectionSettings.ObjectSelectionBoxRenderSettings.SelectionBoxStyle);
            objectSelectionBoxRenderer.RenderObjectSelectionBoxes(selectedObjects);
        }
        #endregion
    }
}
