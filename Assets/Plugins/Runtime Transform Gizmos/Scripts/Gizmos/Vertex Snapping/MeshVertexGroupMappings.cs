using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// The purpose of this class is to map individual meshes to their vertex groups.
    /// This mapping allows us to optimize the vertex snapping operations because
    /// instead if looping though all the vertices inside a mesh to find the one that
    /// is closest to the mouse cursor, we will first find the closest vertex group
    /// and then perform the nearest vertex check against the vertices which reside
    /// inside that group.
    /// </summary>
    public class MeshVertexGroupMappings : SingletonBase<MeshVertexGroupMappings>
    {
        #region Private Variables
        /// <summary>
        /// This dictionary maps a mesh to the list of vertex groups that can be found 
        /// in that mesh.
        /// </summary>
        private Dictionary<Mesh, List<MeshVertexGroup>> _meshToVertexGroups = new Dictionary<Mesh, List<MeshVertexGroup>>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a vertex group mapping for the specified mesh. If the mesh already
        /// has a mapping, the mapping will be removed and a new one will be created.
        /// </summary>
        /// <returns>
        /// True on success and false on failure.
        /// </returns>
        public bool CreateMappingForMesh(Mesh mesh)
        {
            if (mesh == null || !mesh.isReadable) return false;

            // Remove the existing mapping if necessary
            if (_meshToVertexGroups.ContainsKey(mesh)) _meshToVertexGroups.Remove(mesh);

            // Retrieve the mesh vertex groups and create the mapping
            List<MeshVertexGroup> meshVertexGroups = MeshVertexGroupFactory.Create(mesh);
            if (meshVertexGroups.Count != 0)
            {
                _meshToVertexGroups.Add(mesh, meshVertexGroups);
                return true;
            }

            // If no vertex groups, were created, return failure
            return false;
        }

        /// <summary>
        /// Returns the vertex groups which are mapped to the specified mesh.
        /// </summary>
        /// <returns>
        /// The vertex groups which are mapped to the specified mesh or an empty list
        /// if no vertex groups were mapped to the mesh.
        /// </returns>
        public List<MeshVertexGroup> GetMeshVertexGroups(Mesh mesh)
        {
            if (_meshToVertexGroups.ContainsKey(mesh)) return new List<MeshVertexGroup>(_meshToVertexGroups[mesh]);
            else
            if (CreateMappingForMesh(mesh)) return new List<MeshVertexGroup>(_meshToVertexGroups[mesh]);

            return new List<MeshVertexGroup>();
        }

        /// <summary>
        /// This method can be used to check if there is a mapping present for the specified mesh.
        /// </summary>
        public bool ContainsMappingForMesh(Mesh mesh)
        {
            return mesh != null && _meshToVertexGroups.ContainsKey(mesh);
        }
        #endregion
    }
}
