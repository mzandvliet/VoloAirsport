using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a game object clicked handler which is fired when the object 
    /// selection mode is set to 'IndividualObjects'.
    /// </summary>
    public class IndividualObjectsSelectionGameObjectClickedHandler : ObjectSelectionGameObjectClickedHandler
    {
        #region Public Methods
        /// <summary>
        /// Handles the game object clicked event. The method returns true if the object 
        /// selection has changed after the event was handled and false otherwise.
        /// </summary>
        public override bool Handle(GameObject gameObject)
        {
            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

            // Should we append the object to the selection or deselect it if already selected?
            if (editorObjectSelection.AppendOrDeselectOnClick)
            {
                // If already selected, deselect it. Otherwise, append it to the current selection.
                if (editorObjectSelection.IsGameObjectSelected(gameObject)) return editorObjectSelection.DeselectGameObject(gameObject);
                else return editorObjectSelection.SelectGameObject(gameObject);
            }
            else return editorObjectSelection.ClearAndSelectGameObject(gameObject);    // Clear the selection and select the clicked object
        }
        #endregion
    }
}
