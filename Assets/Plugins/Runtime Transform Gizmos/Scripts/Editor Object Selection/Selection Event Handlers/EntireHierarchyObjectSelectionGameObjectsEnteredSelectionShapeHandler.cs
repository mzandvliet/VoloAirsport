using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a game object entered selection shape handler which is fired when the object 
    /// selection mode is set to 'EntireHierarchy'.
    /// </summary>
    public class EntireHierarchyObjectSelectionGameObjectsEnteredSelectionShapeHandler : ObjectSelectionGameObjectsEnteredSelectionShapeHandler
    {
        #region Public Methods
        /// <summary>
        /// Handles the game objects entered selection shape event. The method returns true if the  
        /// object selection has changed after the event was handled and false otherwise.
        /// </summary>
        public override bool Handle(List<GameObject> gameObjects)
        {
            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

            // We will need to select the entire hierarchy of all objects which were
            // passed as parameter. We will start by identifying the roots/top parents
            // of all those objects and store them in a hash set to easily avoid duplicates.
            var objectRoots = GameObjectExtensions.GetRootObjectsFromObjectCollection(gameObjects);

            // Now, we need to construct the final object list which will be used to perform the
            // necessary actions (i.e. select or deselect). We do this by adding the hierarchies
            // of all root objects that we identified earlier to the same object list.
            var finalObjectsList = new List<GameObject>();
            foreach(GameObject rootObject in objectRoots)
            {
                finalObjectsList.AddRange(rootObject.GetAllChildrenIncludingSelf());
            }

            // If multi-object deselection is enabled, we will deselect the game objects which were intersetced by the selection shape
            if (editorObjectSelection.MultiDeselect) return editorObjectSelection.DeselectGameObjectCollection(finalObjectsList);
            else
            // If append is enabled, we will append the objects to the selection
            if (editorObjectSelection.AppendOrDeselectOnClick) return editorObjectSelection.SelectGameObjectCollection(finalObjectsList);
            // If non of the above, we will clear the selection and make sure that only the specified collection of objects is selected
            else return editorObjectSelection.ClearAndSelectGameObjectCollection(finalObjectsList);
        }
        #endregion
    }
}