using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Holds information for an object selection box.
    /// </summary>
    public class ObjectSelectionBox
    {
        #region Private Variables
        private Box _modelSpaceBox;
        private Matrix4x4 _transformMatrix;
        #endregion

        #region Constructors
        public ObjectSelectionBox()
        {
            _modelSpaceBox = Box.GetInvalid();
            _transformMatrix = Matrix4x4.identity;
        }

        public ObjectSelectionBox(Box modelSpaceBox)
        {
            _modelSpaceBox = modelSpaceBox;
            _transformMatrix = Matrix4x4.identity;
        }

        public ObjectSelectionBox(Box modelSpaceBox, Matrix4x4 transformMatrix)
        {
            _modelSpaceBox = modelSpaceBox;
            _transformMatrix = transformMatrix;
        }
        #endregion

        #region Public Properties
        public Box ModelSpaceBox { get { return _modelSpaceBox; } set { _modelSpaceBox = value; } }
        public Matrix4x4 TransformMatrix { get { return _transformMatrix; } set { _transformMatrix = value; } }
        #endregion
    }
}
