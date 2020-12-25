using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a base abstract class which represents a shape that can be used
    /// to select objects in the scene (e.g. selection rectangle).
    /// </summary>
    public abstract class ObjectSelectionShape
    {
        #region Protected Variables
        /// <summary>
        /// Specifies whether or not the selection shape is visible. The selection
        /// shape is rendered only when this is set to true.
        /// </summary>
        protected bool _isVisible;

        /// <summary>
        /// This is the rectangle which encloses the shape in screen space.
        /// </summary>
        protected Rect _enclosingRectangle;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the shape's visibility.
        /// </summary>
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the top left point of the shape's enclosing rectangle.
        /// </summary>
        public void SetEnclosingRectTopLeftPoint(Vector2 topLeftPoint)
        {
            _enclosingRectangle.xMin = topLeftPoint.x;
            _enclosingRectangle.yMax = topLeftPoint.y;
        }

        /// <summary>
        /// Sets the bottom right point of the shape's enclosing rectangle.
        /// </summary>
        public void SetEnclosingRectBottomRightPoint(Vector2 bottomRightPoint)
        {
            _enclosingRectangle.xMax = bottomRightPoint.x;
            _enclosingRectangle.yMin = bottomRightPoint.y;
        }
        #endregion

        #region Public Abstract Methods
        /// <summary>
        /// Abstract method which must be implemented by all derived classes. It is
        /// responsible for rendering the shape if it is visible.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Abstract method which returns a list of game objects which intersect the
        /// selection shape in screen space.
        /// </summary>
        /// <param name="gameObjects">
        /// This is the list of game objects which must be checked for intersection.
        /// </param>
        /// <param name="camera">
        /// This is the camera which is responsible for rendering the scene.
        /// </param>
        public abstract List<GameObject> GetIntersectingGameObjects(List<GameObject> gameObjects, Camera camera);
        #endregion

        #region Protected Methods
        /// <summary>
        /// Can be used to check if the enclosing rectangle is big enough to
        /// be able to perform an object selection.
        /// </summary>
        protected bool IsEnclosingRectangleBigEnoughForSelection()
        {
            return (Mathf.Abs(_enclosingRectangle.width) > 2 && Mathf.Abs(_enclosingRectangle.height) > 2);
        }
        #endregion
    }
}
