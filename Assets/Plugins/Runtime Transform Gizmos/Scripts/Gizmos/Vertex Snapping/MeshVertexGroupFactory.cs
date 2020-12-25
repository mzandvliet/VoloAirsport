using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class can be used to build a list of mesh vertex groups for a specified input mesh.
    /// </summary>
    public static class MeshVertexGroupFactory
    {
        #region Private Nested Structs
        private struct VertexGroupIndices
        {
            public int XIndex;
            public int YIndex;
            public int ZIndex;

            public VertexGroupIndices(int xIndex, int yIndex, int zIndex)
            {
                XIndex = xIndex;
                YIndex = yIndex;
                ZIndex = zIndex;
            }
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Creates and returns a list of vertex groups for the specified mesh.
        /// </summary>
        public static List<MeshVertexGroup> Create(Mesh mesh)
        {
            if (!mesh.isReadable) return new List<MeshVertexGroup>();

            // These variables holds the number of groups per world unit. It's probably worth
            // experimenting with these values, but there is no correct value that you can set.
            // The bigger the values, the bigger the number of vertices which can exist in one
            // group. The smaller the value, the bigger the number of vertex groups. It all
            // depends on the kind of meshes you are dealing with. Setting this to 2, seems to
            // provide reasonably good results.
            const float numberOfGroupsPerWorldUnitX = 2.0f;
            const float numberOfGroupsPerWorldUnitY = 2.0f;
            const float numberOfGroupsPerWorldUnitZ = 2.0f;

            // Cache needed data
            Bounds meshBounds = mesh.bounds;
            Vector3 meshBoundsSize = meshBounds.size;
            Vector3[] meshVertices = mesh.vertices;

            // Calculate the vertec group size on all axes
            float vertexGroupSizeX = meshBoundsSize.x / numberOfGroupsPerWorldUnitX;
            float vertexGroupSizeY = meshBoundsSize.y / numberOfGroupsPerWorldUnitY;
            float vertexGroupSizeZ = meshBoundsSize.z / numberOfGroupsPerWorldUnitZ;
         
            var vertexGroupIndexMappings = new Dictionary<VertexGroupIndices, MeshVertexGroup>();
            for(int vIndex = 0; vIndex < meshVertices.Length; ++vIndex)
            {
                Vector3 vertex = meshVertices[vIndex];

                int groupIndexX = Mathf.FloorToInt(vertex.x / vertexGroupSizeX);
                int groupIndexY = Mathf.FloorToInt(vertex.y / vertexGroupSizeY);
                int groupIndexZ = Mathf.FloorToInt(vertex.z / vertexGroupSizeZ);

                VertexGroupIndices vertGroupIndices = new VertexGroupIndices(groupIndexX, groupIndexY, groupIndexZ);
                if (vertexGroupIndexMappings.ContainsKey(vertGroupIndices)) vertexGroupIndexMappings[vertGroupIndices].AddVertex(vertex);
                else
                {
                    MeshVertexGroup meshVertexGroup = new MeshVertexGroup();
                    meshVertexGroup.AddVertex(vertex);
                    vertexGroupIndexMappings.Add(vertGroupIndices, meshVertexGroup);
                }
            }
            if (vertexGroupIndexMappings.Count == 0) return new List<MeshVertexGroup>();

            var meshVertexGroups = new List<MeshVertexGroup>(vertexGroupIndexMappings.Count);
            foreach(var pair in vertexGroupIndexMappings)
            {
                MeshVertexGroup vertGroup = pair.Value;
                vertGroup.Close();

                meshVertexGroups.Add(vertGroup);
            }

            return meshVertexGroups;        
        }
        #endregion
    }
}
