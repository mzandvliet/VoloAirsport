using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    public class EditorMesh
    {
        #region Private Variables
        private Mesh _mesh;
        private Vector3[] _vertexPositions;
        private int[] _vertexIndices;
        private int _numberOfTriangles;
        private MeshSphereTree _meshSphereTree;
        #endregion

        #region Public Properties
        public Mesh Mesh { get { return _mesh; } }
        public int NumberOfTriangles { get { return _numberOfTriangles; } }
        public Vector3[] VertexPositions { get { return _vertexPositions.Clone() as Vector3[]; } }
        public int[] VertexIndices { get { return _vertexIndices.Clone() as int[]; } }
        public bool IsBuildingTreeSilent { get { return _meshSphereTree.IsBuildingSilent; } }
        #endregion

        #region Constructors
        public EditorMesh(Mesh mesh)
        {
            _mesh = mesh;
            _vertexPositions = _mesh.vertices;
            _vertexIndices = _mesh.triangles;
            _numberOfTriangles = (int)(_vertexIndices.Length / 3);

            _meshSphereTree = new MeshSphereTree(this);
        }
        #endregion

        #region Public Methods
        public void StartSilentTreeBuild()
        {
            _meshSphereTree.BuildSilent();
        }

        public Box GetBox()
        {
            if (_mesh == null) return Box.GetInvalid();
            return new Box(_mesh.bounds);
        }

        public OrientedBox GetOrientedBox(Matrix4x4 transformMatrix)
        {
            if (_mesh == null) return OrientedBox.GetInvalid();

            OrientedBox orientedBox = new OrientedBox(GetBox());
            orientedBox.Transform(transformMatrix);

            return orientedBox;
        }

        public List<Triangle3D> GetAllTriangles()
        {
            if (NumberOfTriangles == 0) return new List<Triangle3D>();

            var allTriangles = new List<Triangle3D>(NumberOfTriangles);
            for(int triIndex = 0; triIndex < NumberOfTriangles; ++triIndex)
            {
                allTriangles.Add(GetTriangle(triIndex));
            }

            return allTriangles;
        }

        public Triangle3D GetTriangle(int triangleIndex)
        {
            int baseIndex = triangleIndex * 3;
            return new Triangle3D(_vertexPositions[_vertexIndices[baseIndex]], _vertexPositions[_vertexIndices[baseIndex + 1]], _vertexPositions[_vertexIndices[baseIndex + 2]]);
        }

        public MeshRayHit Raycast(Ray ray, Matrix4x4 meshTransformMatrix)
        {
            // Note: I can't think of a situation in which negative scale would be useful,
            //       so we're going to set the scale to a positive value.
            //meshTransformMatrix.Scale = meshTransformMatrix.Scale.GetVectorWithPositiveComponents();
            return _meshSphereTree.Raycast(ray, meshTransformMatrix);
        }

        public List<Vector3> GetOverlappedWorldVerts(Box box, Matrix4x4 meshTransformMatrix)
        {
            return _meshSphereTree.GetOverlappedWorldVerts(box.ToOrientedBox(), meshTransformMatrix);
        }
        #endregion
    }
}