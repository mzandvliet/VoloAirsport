using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class represents an object selection rectangle which is used by the
    /// object selection module to select multiple objects at once.
    /// </summary>
    [Serializable]
    public class ObjectSelectionRectangle : ObjectSelectionShape
    {
        #region Private Variables
        /// <summary>
        /// Holds the settings which specify how the rectangle must be rendered.
        /// </summary>
        [SerializeField]
        ObjectSelectionRectangleRenderSettings _renderSettings = new ObjectSelectionRectangleRenderSettings();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the object selection rectangle render settings. The client code can modify these
        /// settings to control the way in which the rectangle is rendered.
        /// </summary>
        public ObjectSelectionRectangleRenderSettings RenderSettings { get { return _renderSettings; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a list of game objects which intersect the selection rectangle in screen space.
        /// </summary>
        /// <param name="gameObjects">
        /// This is the list of game objects which must be checked for intersection.
        /// </param>
        /// <param name="camera">
        /// This is the camera which is responsible for rendering the scene.
        /// </param>
        public override List<GameObject> GetIntersectingGameObjects(List<GameObject> gameObjects, Camera camera)
        {
            // Just make sure the area of the enclosing rectangle is big enough for object selection
            if (!IsEnclosingRectangleBigEnoughForSelection()) return new List<GameObject>();

            // Lop through all game objects in the list
            var intersectingGameObjects = new List<GameObject>();
            foreach (GameObject gameObject in gameObjects)
            {
                // If the game object's screen rectangle intersects the selection rectangle, add it to the list
                if (_enclosingRectangle.Overlaps(gameObject.GetScreenRectangle(camera), true)) intersectingGameObjects.Add(gameObject);
            }

            // Return the list of intersecting game objects
            return intersectingGameObjects;
        }

        /// <summary>
        /// Renders the object selection rectangle if it was marked as visible.
        /// </summary>
        public override void Render()
        {
            if(_isVisible)
            {
                GLPrimitives.Draw2DFilledRectangle(_enclosingRectangle, _renderSettings.FillColor, MaterialPool.Instance.Geometry2D, EditorCamera.Instance.Camera);
                GLPrimitives.Draw2DRectangleBorderLines(_enclosingRectangle, _renderSettings.BorderLineColor, MaterialPool.Instance.GLLine, EditorCamera.Instance.Camera);
            }
        }
        #endregion
    }
}
