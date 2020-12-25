using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RTEditor
{
    public class EditorMeshDatabase : MonoSingletonBase<EditorMeshDatabase>
    {
        #region Private Variables
        private Dictionary<Mesh, EditorMesh> _meshes = new Dictionary<Mesh, EditorMesh>();

        // Note: For the moment a value of 1 seems to be like the best value. Otherwise, we can run into situations
        //       such as 2 meshes being silent built while the user attempts to pick one of it using the mouse cursor.
        //       In that case the editor mesh instance will have to wait until the silent job finishes building the
        //       sphere tree. However, because there are 2 threads competing, the other may be given more execution time
        //       and this can lead to even small meshes such as cubes taking much more time to have their trees built
        //       than needed. 
        private const int _maxNumberOfSilentBuildMeshes = 1;        
        private List<EditorMesh> _sortedSilentBuildCandidates = new List<EditorMesh>();
        private List<EditorMesh> _silentBuildMeshes = new List<EditorMesh>();
        #endregion

        #region Public Methods
        public bool AddMeshToSilentBuild(EditorMesh editorMesh)
        {
            if (!Contains(editorMesh.Mesh) || IsMeshSilentBuilding(editorMesh)) return false;

            _sortedSilentBuildCandidates.Add(editorMesh);
            _sortedSilentBuildCandidates.Sort(delegate(EditorMesh mesh0, EditorMesh mesh1)
            {
                if (mesh0.NumberOfTriangles < mesh1.NumberOfTriangles) return 1;
                if (mesh1.NumberOfTriangles < mesh0.NumberOfTriangles) return -1;
                return 0;
            });

            return true;
        }

        public bool AddMeshesToSilentBuild(List<EditorMesh> editorMeshes)
        {
            if (editorMeshes == null || editorMeshes.Count == 0) return false;

            bool allMeshesWereAdded = true;
            foreach(var editorMesh in editorMeshes)
            {
                if (!AddMeshToSilentBuild(editorMesh)) allMeshesWereAdded = false;
            }

            return allMeshesWereAdded;
        }

        public EditorMesh CreateEditorMesh(Mesh mesh)
        {
            if (!IsMeshValid(mesh)) return null;

            if (_meshes.ContainsKey(mesh)) return null;
            else
            {
                EditorMesh editorMesh = new EditorMesh(mesh);
                _meshes.Add(mesh, editorMesh);

                return editorMesh;
            }
        }

        public List<EditorMesh> CreateEditorMeshes(List<Mesh> meshes)
        {
            if (meshes == null || meshes.Count == 0) return new List<EditorMesh>();

            var editorMeshes = new List<EditorMesh>(meshes.Count);
            foreach(var mesh in meshes)
            {
                EditorMesh editorMesh = CreateEditorMesh(mesh);
                if (editorMesh == null) continue;

                editorMeshes.Add(editorMesh);
            }

            return editorMeshes;
        }

        public EditorMesh GetEditorMesh(Mesh mesh)
        {
            if (!IsMeshValid(mesh)) return null;

            if (Contains(mesh)) return _meshes[mesh];
            else return CreateEditorMesh(mesh);
        }

        public bool Contains(Mesh mesh)
        {
            return mesh != null && _meshes.ContainsKey(mesh);
        }

        public bool IsMeshValid(Mesh mesh)
        {
            return mesh != null && mesh.isReadable;
        }

        public bool IsMeshSilentBuilding(EditorMesh editorMesh)
        {
            return _silentBuildMeshes.Contains(editorMesh) || _sortedSilentBuildCandidates.Contains(editorMesh);
        }
        #endregion

        #region Private Methods
        private void Start()
        {
            StartCoroutine(DoEditorMeshSilentBuild());
        }

        private void RemoveNullMeshEntries()
        {
            var newMeshDictionary = new Dictionary<Mesh, EditorMesh>();
            foreach (KeyValuePair<Mesh, EditorMesh> pair in _meshes)
            {
                if (pair.Key != null) newMeshDictionary.Add(pair.Key, pair.Value);
            }
    
            _meshes = newMeshDictionary;
        }
        #endregion

        #region Coroutines
        private IEnumerator DoEditorMeshSilentBuild()
        {
            while (true)
            {
                if (_silentBuildMeshes.Count < _maxNumberOfSilentBuildMeshes)
                {
                    while (_silentBuildMeshes.Count < _maxNumberOfSilentBuildMeshes && _sortedSilentBuildCandidates.Count != 0)
                    {
                        EditorMesh editorMesh = _sortedSilentBuildCandidates[0];
                        _silentBuildMeshes.Add(editorMesh);
                        editorMesh.StartSilentTreeBuild();
 
                        _sortedSilentBuildCandidates.RemoveAt(0);
                    }
                }

                _silentBuildMeshes.RemoveAll(item => !item.IsBuildingTreeSilent);
                yield return null;
            }
        }
        #endregion
    }
}