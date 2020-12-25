//#define USE_TRANSFORM_HAS_CHANGED
using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    public class GameObjectSphereTree
    {
        #region Private Classes
        private class GameObjectTransformData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }
        #endregion

        #region Private Variables
        private float _nullCleanupTime = 0.0f;
        private SphereTree<GameObject> _sphereTree;
        private Dictionary<GameObject, SphereTreeNode<GameObject>> _gameObjectToNode = new Dictionary<GameObject, SphereTreeNode<GameObject>>();

        #if !USE_TRANSFORM_HAS_CHANGED
        private Dictionary<GameObject, GameObjectTransformData> _gameObjectToTransformData = new Dictionary<GameObject, GameObjectTransformData>();
        #endif
        #endregion

        #region Public Properties
        public int NumberOfGameObjects { get { return _gameObjectToNode.Count; } }
        #endregion

        #region Constructors
        public GameObjectSphereTree(int numberOfChildNodesPerNode)
        {
            _sphereTree = new SphereTree<GameObject>(numberOfChildNodesPerNode);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a list of all objects which are overlapped by the specified sphere.
        /// </summary>
        /// <param name="sphere">
        /// The sphere involved in the overlap query.
        /// </param>
        /// <param name="objectOverlapPrecision">
        /// The desired overlap precision. For the moment this is not used.
        /// </param>
        public List<GameObject> OverlapSphere(Sphere3D sphere, ObjectOverlapPrecision objectOverlapPrecision = ObjectOverlapPrecision.ObjectBox)
        {
            // Retrieve all the sphere tree nodes which are overlapped by the sphere. If no nodes are overlapped,
            // we can return an empty list because it means that no objects could possibly be overlapped either.
            List<SphereTreeNode<GameObject>> allOverlappedNodes = _sphereTree.OverlapSphere(sphere);
            if (allOverlappedNodes.Count == 0) return new List<GameObject>();

            // Loop through all overlapped nodes
            var overlappedObjects = new List<GameObject>();
            foreach(SphereTreeNode<GameObject> node in allOverlappedNodes)
            {
                // Store the node's object for easy access
                GameObject gameObject = node.Data;

                // Note: It is important to check for null because the object may have been destroyed. 'RemoveNullObjectNodes'
                //       removes null objects but given the order in which Unity calls certain key functions such as 'OnSceneGUI'
                //       and any editor registered callbacks, null object references can still pop up.
                if (gameObject == null) continue;
                if (!gameObject.activeSelf) continue;

                // We need to perform an additional check. Even though the sphere overlaps the object's node (which is
                // another sphere), we must also check if the sphere overlaps the object's world oriented box. This allows
                // for better precision.
                OrientedBox objectWorldOrientedBox = gameObject.GetWorldOrientedBox();
                if(sphere.OverlapsFullyOrPartially(objectWorldOrientedBox)) overlappedObjects.Add(gameObject);
            }

            return overlappedObjects;
        }

        /// <summary>
        /// Returns a list of all objects which are overlapped by the specified box.
        /// </summary>
        /// <param name="box">
        /// The box involved in the overlap query.
        /// </param>
        /// <param name="objectOverlapPrecision">
        /// The desired overlap precision. For the moment this is not used.
        /// </param>
        public List<GameObject> OverlapBox(OrientedBox box, ObjectOverlapPrecision objectOverlapPrecision = ObjectOverlapPrecision.ObjectBox)
        {
            // Retrieve all the sphere tree nodes which are overlapped by the box. If no nodes are overlapped,
            // we can return an empty list because it means that no objects could possibly be overlapped either.
            List<SphereTreeNode<GameObject>> allOverlappedNodes = _sphereTree.OverlapBox(box);
            if (allOverlappedNodes.Count == 0) return new List<GameObject>();

            // Loop through all overlapped nodes
            var overlappedObjects = new List<GameObject>();
            foreach (SphereTreeNode<GameObject> node in allOverlappedNodes)
            {
                // Store the node's object for easy access
                GameObject gameObject = node.Data;
                if (gameObject == null) continue;
                if (!gameObject.activeSelf) continue;

                // We need to perform an additional check. Even though the box overlaps the object's node (which is
                // a sphere), we must also check if the box overlaps the object's world oriented box. This allows
                // for better precision.
                OrientedBox objectWorldOrientedBox = gameObject.GetWorldOrientedBox();
                if (box.Intersects(objectWorldOrientedBox)) overlappedObjects.Add(gameObject);
            }

            return overlappedObjects;
        }

        /// <summary>
        /// Returns a list of all objects which are overlapped by the specified box.
        /// </summary>
        /// <param name="box">
        /// The box involved in the overlap query.
        /// </param>
        /// <param name="objectOverlapPrecision">
        /// The desired overlap precision. For the moment this is not used.
        /// </param>
        public List<GameObject> OverlapBox(Box box, ObjectOverlapPrecision objectOverlapPrecision = ObjectOverlapPrecision.ObjectBox)
        {
            // Retrieve all the sphere tree nodes which are overlapped by the box. If no nodes are overlapped,
            // we can return an empty list because it means that no objects could possibly be overlapped either.
            List<SphereTreeNode<GameObject>> allOverlappedNodes = _sphereTree.OverlapBox(box);
            if (allOverlappedNodes.Count == 0) return new List<GameObject>();

            // Loop through all overlapped nodes
            OrientedBox orientedBox = box.ToOrientedBox();
            var overlappedObjects = new List<GameObject>();
            foreach (SphereTreeNode<GameObject> node in allOverlappedNodes)
            {
                // Store the node's object for easy access
                GameObject gameObject = node.Data;
                if (gameObject == null) continue;
                if (!gameObject.activeSelf) continue;

                // We need to perform an additional check. Even though the box overlaps the object's node (which is
                // a sphere), we must also check if the box overlaps the object's world oriented box. This allows
                // for better precision.
                OrientedBox objectWorldOrientedBox = gameObject.GetWorldOrientedBox();
                if (orientedBox.Intersects(objectWorldOrientedBox)) overlappedObjects.Add(gameObject);
            }
 
            return overlappedObjects;
        }

        /// <summary>
        /// Performs a raycast and returns a list of hits for all objects whose oriented 
        /// boxes are intersected by the specified ray.
        /// </summary>
        public List<GameObjectRayHit> RaycastAllBox(Ray ray)
        {
            // First, retrieve a list of the sphere tree nodes which were hit by the ray.
            // If no nodes were hit, it means no object was hit either.
            List<SphereTreeNodeRayHit<GameObject>> allNodeHits = _sphereTree.RaycastAll(ray);
            if (allNodeHits.Count == 0) return new List<GameObjectRayHit>();

            // Loop through all nodes which were hit by the ray. For each node, we have to detect
            // if the ray hits the actual object box.
            var gameObjectHits = new List<GameObjectRayHit>();
            foreach(SphereTreeNodeRayHit<GameObject> nodeHit in allNodeHits)
            {
                // Retrieve the object which resides in the node
                GameObject gameObject = nodeHit.HitNode.Data;
                if (gameObject == null) continue;
                if (!gameObject.activeSelf) continue;
         
                // If the ray intersects the object's box, add the hit to the list
                GameObjectRayHit gameObjectRayHit = null;
                if (gameObject.RaycastBox(ray, out gameObjectRayHit)) gameObjectHits.Add(gameObjectRayHit);
            }

            return gameObjectHits;
        }

        /// <summary>
        /// Performs a raycast and returns a list of hits for all sprite objects intersected
        /// by the ray.
        /// </summary>
        public List<GameObjectRayHit> RaycastAllSprite(Ray ray)
        {
            // First, retrieve a list of the sphere tree nodes which were hit by the ray.
            // If no nodes were hit, it means no object was hit either.
            List<SphereTreeNodeRayHit<GameObject>> allNodeHits = _sphereTree.RaycastAll(ray);
            if (allNodeHits.Count == 0) return new List<GameObjectRayHit>();

            // Loop through all nodes which were hit by the ray. For each node, we have to detect
            // if the ray hits the sprite object.
            var gameObjectHits = new List<GameObjectRayHit>();
            foreach (SphereTreeNodeRayHit<GameObject> nodeHit in allNodeHits)
            {
                // Retrieve the object which resides in the node
                GameObject gameObject = nodeHit.HitNode.Data;
                if (gameObject == null) continue;
                if (!gameObject.HasSpriteRendererWithSprite()) continue;

                // If the ray intersects the object's sprite, add the hit to the list
                GameObjectRayHit gameObjectRayHit = null;
                if (gameObject.RaycastSprite(ray, out gameObjectRayHit)) gameObjectHits.Add(gameObjectRayHit);
            }

            return gameObjectHits;
        }

        /// <summary>
        /// Performs a raycast and returns a list of hits for all objects whose meshes 
        /// are intersected by the specified ray.
        /// </summary>
        public List<GameObjectRayHit> RaycastAllMesh(Ray ray)
        {
            // First, we will gather the objects whos boxes are intersected by the ray. If
            // no such objects exist, we will return an empty list.
            List<GameObjectRayHit> allBoxHits = RaycastAllBox(ray);
            if (allBoxHits.Count == 0) return new List<GameObjectRayHit>();

            // Now we will loop through all these objects and identify the ones whose meshes
            // are hit by the ray.
            var allMeshObjectHits = new List<GameObjectRayHit>(allBoxHits.Count);
            foreach(var boxHit in allBoxHits)
            {
                // Store the object for easy access
                GameObject hitObject = boxHit.HitObject;
                if (hitObject == null) continue;
                if (!hitObject.activeSelf) continue;

                GameObjectRayHit gameObjectRayHit = null;
                if (hitObject.RaycastMesh(ray, out gameObjectRayHit)) allMeshObjectHits.Add(gameObjectRayHit);
            }

            return allMeshObjectHits;
        }

        /// <summary>
        /// Must be called from an 'Update' method to perform any necessary tree
        /// updates.
        /// </summary>
        public void Update()
        {
            _nullCleanupTime += Time.deltaTime;
            if (_nullCleanupTime >= 1.0f)
            {
                RemoveNullObjectNodes();
                _nullCleanupTime = 0.0f;
            }

            GameObject[] sceneObjects = MonoBehaviour.FindObjectsOfType<GameObject>();
            foreach(GameObject gameObject in sceneObjects)
            {
                if (!IsGameObjectRegistered(gameObject)) 
                {
                    if (!RegisterGameObject(gameObject)) continue;
                }

                #if !USE_TRANSFORM_HAS_CHANGED
                Transform objectTransform = gameObject.transform;
                GameObjectTransformData objectTransformData = _gameObjectToTransformData[gameObject];
                if(objectTransformData.Position != objectTransform.position || 
                   objectTransformData.Rotation != objectTransform.rotation || 
                   objectTransformData.Scale != objectTransform.lossyScale)
                {
                    HandleObjectTransformChange(objectTransform);
                    _gameObjectToTransformData[gameObject] = GetGameObjectTransformData(gameObject);
                }
                #else
                Transform objectTransform = gameObject.transform;
                if (objectTransform.hasChanged)
                {
                    HandleObjectTransformChange(objectTransform);
                    objectTransform.hasChanged = false;
                }
                #endif
            }

            _sphereTree.PerformPendingUpdates();
        }

        /// <summary>
        /// Can be used to check if an object was registered with the tree.
        /// </summary>
        public bool IsGameObjectRegistered(GameObject gameObject)
        {
            return _gameObjectToNode.ContainsKey(gameObject);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method accepts a list of object hierarchy root nodes and registers
        /// all objects in those hierarchies with the tree.
        /// </summary>
        private void RegisterGameObjectHierarchies(List<GameObject> roots)
        {
            foreach (GameObject root in roots)
            {
                RegisterGameObjectHierarchy(root);
            }
        }

        /// <summary>
        /// Registers all objects in the specified hierarchy with the tree.
        /// </summary>
        private void RegisterGameObjectHierarchy(GameObject root)
        {
            List<GameObject> allChildrenIncludingSelf = root.GetAllChildrenIncludingSelf();
            foreach (GameObject gameObject in allChildrenIncludingSelf)
            {
                RegisterGameObject(gameObject);
            }
        }

        /// <summary>
        /// Registers the specified object with the tree.
        /// </summary>
        private bool RegisterGameObject(GameObject gameObject)
        {
            if (!CanGameObjectBeRegisteredWithTree(gameObject)) return false;
   
            // Build the object's sphere
            Box objectWorldBox = gameObject.GetWorldBox();
            Sphere3D objectSphere = objectWorldBox.GetEncapsulatingSphere();

            // Add the object as a terminal node. Also store the node in the dictionary so that we can 
            // use it when it's needed.
            SphereTreeNode<GameObject> objectNode = _sphereTree.AddTerminalNode(objectSphere, gameObject);
            _gameObjectToNode.Add(gameObject, objectNode);
            
            #if !USE_TRANSFORM_HAS_CHANGED
            _gameObjectToTransformData.Add(gameObject, GetGameObjectTransformData(gameObject));
            #endif

            // If it is a mesh object, start silent building the mesh
            if (gameObject.HasMesh())
            {
                EditorMesh editorMesh = EditorMeshDatabase.Instance.CreateEditorMesh(gameObject.GetMesh());
                if (editorMesh != null) EditorMeshDatabase.Instance.AddMeshToSilentBuild(editorMesh);
            }

            EditorCamera.Instance.AdjustObjectVisibility(gameObject);
            return true;
        }

        private GameObjectTransformData GetGameObjectTransformData(GameObject gameObject)
        {
            Transform gameObjectTransform = gameObject.transform;

            var gameObjectTransformData = new GameObjectTransformData();
            gameObjectTransformData.Position = gameObjectTransform.position;
            gameObjectTransformData.Rotation = gameObjectTransform.rotation;
            gameObjectTransformData.Scale = gameObjectTransform.lossyScale;

            return gameObjectTransformData;
        }

        /// <summary>
        /// Returns true if the specified game object can be registered with the tree.
        /// </summary>
        private bool CanGameObjectBeRegisteredWithTree(GameObject gameObject)
        {
            if (gameObject == null || _gameObjectToNode.ContainsKey(gameObject)) return false;
            if (gameObject == EditorCamera.Instance.Background.IsSameAs(gameObject)) return false;
            if (gameObject.HasTerrain()) return false;
            if (gameObject.HasCamera()) return false;
            if (gameObject.IsRTEditorSystemObject()) return false;

            return true;
        }

        /// <summary>
        /// Handles the transform change for the specified object transform.
        /// </summary>
        private void HandleObjectTransformChange(Transform gameObjectTransform)
        {
            // Just ensure that the object is registered with the tree
            GameObject gameObject = gameObjectTransform.gameObject;
            if (!IsGameObjectRegistered(gameObject)) return;

            // Store the object's node for easy access. We will need to instruct the 
            // tree to update this node as needed.
            SphereTreeNode<GameObject> objectNode = _gameObjectToNode[gameObject];

            // We will first have to detect what has changed. So we will compare the
            // object's sphere as it is now with what was before.
            bool updateCenter = false;
            bool updateRadius = false;
            Sphere3D previousSphere = objectNode.Sphere;
            Sphere3D currentSphere = gameObject.GetWorldBox().GetEncapsulatingSphere();

            // Detect what changed
            if (previousSphere.Center != currentSphere.Center) updateCenter = true;
            if (previousSphere.Radius != currentSphere.Radius) updateRadius = true;

            // Call the appropriate node update method
            if (updateCenter && updateRadius) _sphereTree.UpdateTerminalNodeCenterAndRadius(objectNode, currentSphere.Center, currentSphere.Radius);
            else if (updateCenter) _sphereTree.UpdateTerminalNodeCenter(objectNode, currentSphere.Center);
            else if (updateRadius) _sphereTree.UpdateTerminalNodeRadius(objectNode, currentSphere.Radius);

            EditorCamera.Instance.AdjustObjectVisibility(gameObject);
        }

        /// <summary>
        /// Removes any terminal nodes from the tree that have null object references.
        /// </summary>
        private void RemoveNullObjectNodes()
        {
            // Loop through each dictionaty entry
            var newObjectToNodeDictionary = new Dictionary<GameObject, SphereTreeNode<GameObject>>();
            foreach (var pair in _gameObjectToNode)
            {
                // If the key is null, remove the node, otherwise store this node in the new dictionary
                if (pair.Key == null) _sphereTree.RemoveNode(pair.Value);
                else newObjectToNodeDictionary.Add(pair.Key, pair.Value);
            }

            // Adjust the dictionary reference to point to the new one which doesn't contain any null object nodes.
            _gameObjectToNode = newObjectToNodeDictionary;
        }
        #endregion
    }
}