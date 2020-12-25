using UnityEngine;

namespace RTEditor
{
    public class MeshPool : SingletonBase<MeshPool>
    {
        #region Private Variables
        private Mesh _coneMesh;
        private Mesh _XYSquareMesh;
        private Mesh _sphereMesh;
        private Mesh _boxMesh;
        private Mesh _rightAngledTriangleMesh;
        private Mesh _xzGridLineMesh;
        #endregion

        #region Public Properties
        public Mesh ConeMesh
        {
            get
            {
                if (_coneMesh == null) _coneMesh = ProceduralMeshGenerator.CreateConeMesh(1.0f, 1.0f, 30, 30, 5);
                return _coneMesh;
            }
        }

        public Mesh XYSquareMesh
        {
            get
            {
                if (_XYSquareMesh == null) _XYSquareMesh = ProceduralMeshGenerator.CreatePlaneMesh(1.0f, 1.0f);
                return _XYSquareMesh;
            }
        }

        public Mesh SphereMesh
        {
            get
            {
                if (_sphereMesh == null) _sphereMesh = ProceduralMeshGenerator.CreateSphereMesh(1.0f, 50, 50);
                return _sphereMesh;
            }
        }

        public Mesh BoxMesh
        {
            get
            {
                if (_boxMesh == null) _boxMesh = ProceduralMeshGenerator.CreateBoxMesh(1.0f, 1.0f, 1.0f);
                return _boxMesh;
            }
        }

        public Mesh RightAngledTriangleMesh
        {
            get
            {
                if (_rightAngledTriangleMesh == null) _rightAngledTriangleMesh = ProceduralMeshGenerator.CreateRightAngledTriangleMesh(1.0f, 1.0f);
                return _rightAngledTriangleMesh;
            }
        }

        public Mesh XGridLineMesh
        {
            get
            {
                if (_xzGridLineMesh == null) _xzGridLineMesh = ProceduralMeshGenerator.CreateXZGridLineMesh(1.0f, 1.0f, 150, 150);
                return _xzGridLineMesh;
            }
        }
        #endregion
    }
}
