using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class MeshSphereTreeBuildJob : SilentJob
    {
        #region Private Variables
        // Note: This array must have a one-to-one mapping with the actual 'Mesh' triangle array.
        private List<Triangle3D> _meshTriangles;
        private SphereTree<MeshSphereTreeTriangle> _sphereTree;
        #endregion

        #region Delegates
        public delegate bool TriangleValidation(Triangle3D triangle);
        public delegate void SilentBuildFinished();

        public TriangleValidation ValidateTriangle = null;
        public event SilentBuildFinished OnSilentBuildFinished;
        #endregion

        #region Constructors
        public MeshSphereTreeBuildJob(List<Triangle3D> meshTriangles, SphereTree<MeshSphereTreeTriangle> sphereTree)
        {
            _meshTriangles = new List<Triangle3D>(meshTriangles);
            _sphereTree = sphereTree;
        }
        #endregion

        #region Protected Methods
        protected override void DoJob()
        {
            for (int triIndex = 0; triIndex < _meshTriangles.Count; ++triIndex)
            {
                Triangle3D triangle = _meshTriangles[triIndex];
                if (ValidateTriangle != null && !ValidateTriangle(triangle)) continue;

                var meshSphereTreeTriangle = new MeshSphereTreeTriangle(triIndex);
                _sphereTree.AddTerminalNode(triangle.GetEncapsulatingSphere(), meshSphereTreeTriangle);
            }

            if (OnSilentBuildFinished != null) OnSilentBuildFinished();
        }
        #endregion
    }
}
