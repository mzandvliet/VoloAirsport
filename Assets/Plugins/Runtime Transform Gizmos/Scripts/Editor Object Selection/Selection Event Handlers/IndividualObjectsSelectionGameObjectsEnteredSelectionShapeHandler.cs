using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a game object entered selection shape handler which is fired when the object 
    /// selection mode is set to 'IndividualObjects'.
    /// </summary>
    public class IndividualObjectsSelectionGameObjectsEnteredSelectionShapeHandler : ObjectSelectionGameObjectsEnteredSelectionShapeHandler
    {
        #region Public Methods
        /// <summary>
        /// Handles the game objects entered selection shape event. The method returns true if the  
        /// object selection has changed after the event was handled and false otherwise.
        /// </summary>
        public override bool Handle(List<GameObject> gameObjects)
        {
            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

            // If multi-object deselection is enabled, we will deselect the game objects which were intersetced by the selection shape
            if (editorObjectSelection.MultiDeselect) return editorObjectSelection.DeselectGameObjectCollection(gameObjects);
            else
            // If append is enabled, we will append the objects to the selection
            if (editorObjectSelection.AppendOrDeselectOnClick) return editorObjectSelection.SelectGameObjectCollection(gameObjects);
            // If non of the above, we will clear the selection and make sure that only the specified collection of objects is selected
            else return editorObjectSelection.ClearAndSelectGameObjectCollection(gameObjects);
        }
        #endregion
    }
}
