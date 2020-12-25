using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    public class MeshSphereTree 
    {
        #region Private Variables
        private SphereTree<MeshSphereTreeTriangle> _sphereTree;
        private EditorMesh _editorMesh;
        private bool _wasBuilt = false;
        private MeshSphereTreeBuildJob _buildJob;
        #endregion

        #region Constructors
        public MeshSphereTree(EditorMesh editorMesh)
        {
            _editorMesh = editorMesh;
            _sphereTree = new SphereTree<MeshSphereTreeTriangle>(2);
            _buildJob = new MeshSphereTreeBuildJob(_editorMesh.GetAllTriangles(), _sphereTree);
            _buildJob.ValidateTriangle = new MeshSphereTreeBuildJob.TriangleValidation(IsTriangleValid);
            _buildJob.OnSilentBuildFinished += OnSilentBuildFinished;
        }
        #endregion

        #region Public Properties
        public bool IsBuildingSilent { get { return _buildJob.IsRunning; } }
        public bool WasBuilt { get { return _wasBuilt; } }
        #endregion

        #region Public Methods
        public List<Vector3> GetOverlappedWorldVerts(OrientedBox box, Matrix4x4 meshTransformMatrix)
        {
            if (IsBuildingSilent) while (IsBuildingSilent) ;
            if (!_wasBuilt) Build();

            // Work in mesh model space because the tree daata exists in model space
            Matrix4x4 inverseTransform = meshTransformMatrix.inverse;
            box.Transform(inverseTransform);

            // Retrieve the nodes overlapped by the specified box
            List<SphereTreeNode<MeshSphereTreeTriangle>> overlappedNodes = _sphereTree.OverlapBox(box);
            if (overlappedNodes.Count == 0) return new List<Vector3>();

            // Loop through all nodes
            var overlappedWorldVerts = new List<Vector3>();
            foreach(var node in overlappedNodes)
            {
                // Get the traingle associated with the node
                int triangleIndex = node.Data.TriangleIndex;
                Triangle3D modelSpaceTriangle = _editorMesh.GetTriangle(triangleIndex);

                // Now check which of the triangle points resides inside the box
                List<Vector3> trianglePoints = modelSpaceTriangle.GetPoints();
                foreach(var pt in trianglePoints)
                {
                    // When a point resides inside the box, we will transform it in world space and add it to the final point list
                    if (box.ContainsPoint(pt)) overlappedWorldVerts.Add(meshTransformMatrix.MultiplyPoint(pt));
                }
            }

            return overlappedWorldVerts;
        }

        /// <summary>
        /// Performs a ray cast against the mesh tree and returns an instance of the 'MeshRayHit'
        /// class which holds information about the ray hit. The method returns the hit which is
        /// closest to the ray origin. If no triangle was hit, the method returns null.
        /// </summary>
        public MeshRayHit Raycast(Ray ray, Matrix4x4 meshTransformMatrix)
        {
            if (IsBuildingSilent) while (IsBuildingSilent) ;
            if (!_wasBuilt) Build();
 
            // When the sphere tree is constructed it is constructed in the mesh local space (i.e. it takes
            // no position/rotation/scale into account). This is required because a mesh can be shared by
            // lots of different objects each with its own transform data. This is why we need the mes matrix
            // parameter. It allows us to transform the ray in the mesh local space and perform our tests there.
            Ray meshLocalSpaceRay = ray.InverseTransform(meshTransformMatrix);

            // First collect all terminal nodes which are intersected by this ray. If no nodes
            // are intersected, we will return null.
            List<SphereTreeNodeRayHit<MeshSphereTreeTriangle>> nodeRayHits = _sphereTree.RaycastAll(meshLocalSpaceRay);
            if (nodeRayHits.Count == 0) return null;

            // We now have to loop thorugh all intersected nodes and find the triangle whose
            // intersection point is closest to the ray origin.
            float minT = float.MaxValue; 
            Triangle3D closestTriangle = null;
            int indexOfClosestTriangle = -1;
            Vector3 closestHitPoint = Vector3.zero;
            foreach(var nodeRayHit in nodeRayHits)
            {
                // Retrieve the data associated with the node and construct the mesh triangle instance
                MeshSphereTreeTriangle sphereTreeTriangle = nodeRayHit.HitNode.Data;
                Triangle3D meshTriangle = _editorMesh.GetTriangle(sphereTreeTriangle.TriangleIndex);
 
                // Check if the ray intersects the trianlge which resides in the node
                float hitEnter;
                if(meshTriangle.Raycast(meshLocalSpaceRay, out hitEnter))
                {
                    // The trianlge is intersected by the ray, but we also have to ensure that the
                    // intersection point is closer than what we have found so far. If it is, we 
                    // store all relevant information.
                    if(hitEnter < minT)
                    {
                        minT = hitEnter;
                        closestTriangle = meshTriangle;
                        indexOfClosestTriangle = sphereTreeTriangle.TriangleIndex;
                        closestHitPoint = meshLocalSpaceRay.GetPoint(hitEnter);
                    }
                }
            }

            // If we found the closest triangle, we can construct the ray hit instance and return it.
            // Otherwise we return null. This can happen when the ray intersects the triangle node
            // spheres, but the triangles themselves.
            if (closestTriangle != null)
            {
                // We have worked in mesh local space up until this point, but we want to return the
                // hit info in world space, so we have to transform the hit data accordingly.
                closestHitPoint = meshTransformMatrix.MultiplyPoint(closestHitPoint);
                minT = (ray.origin - closestHitPoint).magnitude;
                Vector3 worldNormal = meshTransformMatrix.MultiplyVector(closestTriangle.Normal);

                return new MeshRayHit(ray, minT, indexOfClosestTriangle, closestHitPoint, worldNormal);
            }
            else return null;
        }

        /// <summary>
        /// Builds the mesh tree if it hasn't already been built.
        /// </summary>
        public void Build()
        {
            if (_wasBuilt || _buildJob.IsRunning) return;

            // Loop through all trianlges and register them with the tree
            for (int triangleIndex = 0; triangleIndex < _editorMesh.NumberOfTriangles; ++triangleIndex)
            {
                RegisterTriangle(triangleIndex);
            }
            _sphereTree.PerformPendingUpdates();

            // The tree was built
            _wasBuilt = true;
        }

        public void BuildSilent()
        {
            if (_wasBuilt || _buildJob.IsRunning) return;
            _buildJob.Start();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Registers the mesh trianlge with the specified index with the tree.
        /// </summary>
        private bool RegisterTriangle(int triangleIndex)
        {
            Triangle3D triangle = _editorMesh.GetTriangle(triangleIndex);
            if (!IsTriangleValid(triangle)) return false;
           
            // Create the triangle node data and instruct the tree to add this node
            var meshSphereTreeTriangle = new MeshSphereTreeTriangle(triangleIndex);
            _sphereTree.AddTerminalNode(triangle.GetEncapsulatingSphere(), meshSphereTreeTriangle);

            return true;
        }

        private bool IsTriangleValid(Triangle3D triangle)
        {
            return !triangle.IsDegenerate;
        }

        private void OnSilentBuildFinished()
        {
            _sphereTree.PerformPendingUpdates();
            _wasBuilt = true;
        }
        #endregion
    }
}