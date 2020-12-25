using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Instances of this class are used in conjunction with vertex snapping. A mesh
    /// vertex group is nothing more than a portion of 3D space which contains vertices.
    /// You can imagine a mesh being partitioned in many bounding boxes. Each bounding
    /// box that contains mesh vertices is a meh vertex group.
    /// </summary>
    public class MeshVertexGroup
    {
        #region Private Variables
        /// <summary>
        /// The vertices which belong to the group expressed in mesh model space.
        /// </summary>
        private List<Vector3> _modelSpaceVertices = new List<Vector3>();

        /// <summary>
        /// The AABB which surrounds all the vertices in mesh model space.
        /// </summary>
        private Bounds _groupAABB;

        private bool _isClosed = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the group's AABB. Because the group contains mesh vertices which are defined
        /// in mesh model space, the returned AABB is also in mesh model space.
        /// </summary>
        public Bounds GroupAABB { get { return _groupAABB; } }

        /// <summary>
        /// Returns a copy of the model space mesh vertices which reside inside the group.
        /// </summary>
        public List<Vector3> ModelSpaceVertices { get { return new List<Vector3>(_modelSpaceVertices); } }

        public bool IsClosed { get { return _isClosed; } }
        #endregion

        #region Public Methods
        public void AddVertex(Vector3 vertex)
        {
            if (_isClosed) return;
            _modelSpaceVertices.Add(vertex);
        }

        public void Close()
        {
            _isClosed = true;
            CalculateGroupAABB();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculates the AABB of the vertex group.
        /// </summary>
        private void CalculateGroupAABB()
        {
            Vector3 minPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // Loop through all vertices in the group and adjust the min and max position variables
            foreach(Vector3 vertex in _modelSpaceVertices)
            {
                minPosition = Vector3.Min(minPosition, vertex);
                maxPosition = Vector3.Max(maxPosition, vertex);
            }

            // Calculate the AABB
            _groupAABB = new Bounds();
            _groupAABB.SetMinMax(minPosition, maxPosition);

            // Note: It is possible for a group to have a zero size in case it contains only
            //       one vertex or if it contains more vertices with the same position. In 
            //       that case, we will just set the size to a value of 0.3 on all axes.
            if (_groupAABB.size.magnitude < 1e-5f) _groupAABB.size = Vector3.one * 0.3f;
        }
        #endregion
    }
}
