using UnityEngine;
using System;

namespace RTEditor
{
    public class MeshSphereTreeTriangle
    {
        #region Private Variables
        private int _triangleIndex;
        #endregion

        #region Public Properties
        public int TriangleIndex { get { return _triangleIndex; } }
        #endregion

        #region Constructors
        public MeshSphereTreeTriangle(int triangleIndex)
        {
            _triangleIndex = triangleIndex;
        }
        #endregion
    }
}