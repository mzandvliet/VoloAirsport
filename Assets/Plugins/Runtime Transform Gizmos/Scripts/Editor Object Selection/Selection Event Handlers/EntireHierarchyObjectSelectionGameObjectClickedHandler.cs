using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a game object clicked handler which is fired when the object 
    /// selection mode is set to 'EntireHierarchy'.
    /// </summary>
    public class EntireHierarchyObjectSelectionGameObjectClickedHandler : ObjectSelectionGameObjectClickedHandler
    {
        #region Public Methods
        /// <summary>
        /// Handles the game object clicked event. The method returns true if the object 
        /// selection has changed after the event was handled and false otherwise.
        /// </summary>
        public override bool Handle(GameObject gameObject)
        {
            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

            // We must select the entire hierarchy, so we will retrieve the top parent and then
            // gather a list of all its children including the parent itself.
            List<GameObject> entireHierarchy = gameObject.transform.root.gameObject.GetAllChildrenIncludingSelf();

            // Should we append the objects to the selection or deselect them if already selected?
            if (editorObjectSelection.AppendOrDeselectOnClick)
            {
                // If already selected, deselect them. Otherwise, append them to the current selection.
                if (editorObjectSelection.IsGameObjectSelected(gameObject)) return editorObjectSelection.DeselectGameObjectCollection(entireHierarchy);
                else return editorObjectSelection.SelectGameObjectCollection(entireHierarchy);
            }
            else return editorObjectSelection.ClearAndSelectGameObjectCollection(entireHierarchy);    // Clear the selection and select the game objects
        }
        #endregion
    }
}
