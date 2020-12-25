using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'GameObject' extension methods.
    /// </summary>
    public static class GameObjectExtensions
    {
        #region Public Static Functions
        #if UNITY_EDITOR
        /// <summary>
        /// Can be used to check if the specified game object is a scene object. This is useful
        /// when wanting to avoid assigning prefabs to 'GameObject' references.
        /// </summary>
        public static bool IsSceneObject(this GameObject gameObject)
        {
            PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);

            // Make sure the specified game object is not a prefab. A game object is not a prefab if
            // 'prefabType' is either 'None' or if the value specifies that object is some kind of
            // a prefab instance. If it is an instance it means it exists inside the scene.
            return prefabType == PrefabType.None || prefabType == PrefabType.PrefabInstance ||
                   prefabType == PrefabType.DisconnectedPrefabInstance || prefabType == PrefabType.MissingPrefabInstance;
        }
        #endif

        public static bool RaycastBox(this GameObject gameObject, Ray ray, out GameObjectRayHit objectRayHit)
        {
            objectRayHit = null;
            OrientedBox objectWorldOrientedBox = gameObject.GetWorldOrientedBox();

            OrientedBoxRayHit objectBoxRayHit;
            if (objectWorldOrientedBox.Raycast(ray, out objectBoxRayHit))
                objectRayHit = new GameObjectRayHit(ray, gameObject, objectBoxRayHit, null, null, null);

            return objectRayHit != null;
        }

        public static bool RaycastSprite(this GameObject gameObject, Ray ray, out GameObjectRayHit objectRayHit)
        {
            objectRayHit = null;

            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return false;

            OrientedBox objectWorldOrientedBox = gameObject.GetWorldOrientedBox();

            OrientedBoxRayHit objectBoxRayHit;
            if (objectWorldOrientedBox.Raycast(ray, out objectBoxRayHit))
            {
                SpriteRayHit spriteHit = new SpriteRayHit(ray, objectBoxRayHit.HitEnter, spriteRenderer, objectBoxRayHit.HitPoint, objectBoxRayHit.HitNormal);
                objectRayHit = new GameObjectRayHit(ray, gameObject, null, null, null, spriteHit);
            }

            return objectRayHit != null;
        }

        public static bool RaycastMesh(this GameObject gameObject, Ray ray, out GameObjectRayHit objectRayHit)
        {
            objectRayHit = null;
            Mesh objectMesh = gameObject.GetMeshFromFilterOrSkinnedMeshRenderer();
            if (objectMesh == null) return false;

            EditorMesh editorMesh = EditorMeshDatabase.Instance.GetEditorMesh(objectMesh);
            if (editorMesh != null)
            {
                MeshRayHit meshRayHit = editorMesh.Raycast(ray, gameObject.transform.GetWorldMatrix());
                if (meshRayHit == null) return false;

                objectRayHit = new GameObjectRayHit(ray, gameObject, null, meshRayHit, null, null);
                return true;
            }
            else
            {
                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if(meshCollider != null)
                {
                    RaycastHit rayHit;
                    if (meshCollider.Raycast(ray, out rayHit, float.MaxValue))
                    {
                        MeshRayHit meshRayHit = new MeshRayHit(ray, rayHit.distance, rayHit.triangleIndex, rayHit.point, rayHit.normal);
                        objectRayHit = new GameObjectRayHit(ray, gameObject, null, meshRayHit, null, null);
                        return true;
                    }
                }
                else return gameObject.RaycastBox(ray, out objectRayHit);
            }

            return false;
        }

        public static bool RaycastTerrain(this GameObject gameObject, Ray ray, out GameObjectRayHit objectRayHit)
        {
            objectRayHit = null;
            if (!gameObject.HasTerrain()) return false;

            TerrainCollider terrainCollider = gameObject.GetComponent<TerrainCollider>();
            if (terrainCollider == null) return false;

            RaycastHit raycastHit;
            if (terrainCollider.Raycast(ray, out raycastHit, float.MaxValue))
            {
                TerrainRayHit terrainRayHit = new TerrainRayHit(ray, raycastHit);
                objectRayHit = new GameObjectRayHit(ray, gameObject, null, null, terrainRayHit, null);
            }

            return objectRayHit != null;
        }

        public static GameObject Clone(this GameObject gameObject, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null)
        {
            GameObject clone = MonoBehaviour.Instantiate(gameObject, position, rotation) as GameObject;
            clone.name = gameObject.name;
            clone.layer = gameObject.layer;

            Transform cloneTransform = clone.transform;
            cloneTransform.localScale = scale;
            cloneTransform.parent = parent;

            return clone;
        }

        public static void PlaceHierarchyOnPlane(this GameObject root, Vector3 ptOnPlane, Vector3 planeNormal, int alignAxisIndex)
        {
            Transform rootTransform = root.transform;
            Box hierarchyModelSpaceAABB = root.GetHierarchyModelSpaceBox();
            Vector3 worldAABBSize = Vector3.Scale(rootTransform.lossyScale, hierarchyModelSpaceAABB.Size);

            planeNormal.Normalize();
            Plane plane = new Plane(planeNormal, ptOnPlane);

            if (alignAxisIndex >= 0 && alignAxisIndex < 3)
            {
                root.AlignAxis(planeNormal, alignAxisIndex);

                Vector3 worldAABBCenter = rootTransform.TransformPoint(hierarchyModelSpaceAABB.Center);
                float sizeAlongAxis = worldAABBSize[alignAxisIndex];

                Vector3 ptOnBoxFace = worldAABBCenter - plane.normal * sizeAlongAxis * 0.5f;
                Vector3 fromPtToPos = rootTransform.position - ptOnBoxFace;
                Vector3 projectedPt = worldAABBCenter - plane.normal * plane.GetDistanceToPoint(worldAABBCenter);
                projectedPt += (ptOnPlane - projectedPt);

                rootTransform.position = projectedPt + fromPtToPos;
            }
            else rootTransform.position = ptOnPlane;
        }

        public static void AlignAxis(this GameObject gameObject, Vector3 destAxis, int srcAxisIndex)
        {
            Transform objectTransform = gameObject.transform;
            Vector3 srcAxis = objectTransform.right;
            if (srcAxisIndex == 1) srcAxis = objectTransform.up;
            if (srcAxisIndex == 2) srcAxis = objectTransform.forward;

            // Already aligned?
            destAxis.Normalize();
            float dot = Vector3.Dot(destAxis, srcAxis);
            if (dot > 0.0f && Mathf.Abs(dot - 1.0f) < 1e-5f) return;

            // Construct perp vector on the 2 axes. This will be used as the rotation axis.
            Vector3 rotAxis = Vector3.Cross(srcAxis, destAxis);
            rotAxis.Normalize();
            float rotAngle = MathHelper.SafeAcos(dot) * Mathf.Rad2Deg;

            // Rotate to bring the axis in the required state
            objectTransform.Rotate(rotAxis, rotAngle, Space.World);
        }

        /// <summary>
        /// Static function which can be used to retrieve a hash set of all root/top parent
        /// objects from the specified object collection.
        /// </summary>
        public static HashSet<GameObject> GetRootObjectsFromObjectCollection(List<GameObject> gameObjects)
        {
            // Loop through all game objects and store their roots inside the set
            var objectRoots = new HashSet<GameObject>();
            foreach (GameObject gameObject in gameObjects)
            {
                objectRoots.Add(gameObject.transform.root.gameObject);
            }

            return objectRoots;
        }

        /// <summary>
        /// Given a collection of game objects, the function returns a list that contains the
        /// objects in the list that don't have any parents inside 'gameObjects'. For example,
        /// if the specified object collection contains the objects A, B, C, D, E and F, where
        /// A is the parent of B and C and D is the parent of E and F, the function will return
        /// the objects A and D. The parents of A and D (if any) are not taken into account.
        /// </summary>
        public static List<GameObject> GetParentsFromObjectCollection(IEnumerable<GameObject> gameObjects)
        {
            // Loop through all game objects
            var parents = new List<GameObject>();
            foreach (GameObject currentObject in gameObjects)
            {
                // Cache the object's transform for easy and fast access
                Transform currentObjectTransform = currentObject.transform;

                // Assume the object doesn't have a parent
                bool foundParent = false;

                // Now loop through the game object collection again and check if the object has a parent
                foreach (GameObject gameObject in gameObjects)
                {
                    // Same object?
                    if (gameObject != currentObject)
                    {
                        // If the current game object has a parent, we set the 'foundParent' variable to
                        // true and exit the loop because we found the info that we were interested in.
                        if (currentObjectTransform.IsChildOf(gameObject.transform))
                        {
                            foundParent = true;
                            break;
                        }
                    }
                }

                // If no parent was found, add the game object to the top parents list
                if (!foundParent) parents.Add(currentObject);
            }

            return parents;
        }

        /// <summary>
        /// Static function which can be used to retrieve a hash set of all root/top parent
        /// objects from the specified object collection.
        /// </summary>
        public static HashSet<GameObject> GetRootObjectsFromObjectCollection(HashSet<GameObject> gameObjects)
        {
            // Loop through all game objects and store their roots inside the set
            var objectRoots = new HashSet<GameObject>();
            foreach (GameObject gameObject in gameObjects)
            {
                objectRoots.Add(gameObject.transform.root.gameObject);
            }

            return objectRoots;
        }

        /// <summary>
        /// Returns a list of of all children of 'gameObject' including 'gameObject' itself.
        /// </summary>
        public static List<GameObject> GetAllChildrenIncludingSelf(this GameObject gameObject)
        {
            // Retrieve all child transforms
            Transform[] allChildTransforms = gameObject.GetComponentsInChildren<Transform>();
            var gameObjects = new List<GameObject>(allChildTransforms.Length);

            // Loop through all child transforms and add them to the output list
            foreach (Transform childTransform in allChildTransforms)
            {
                gameObjects.Add(childTransform.gameObject);
            }

            return gameObjects;
        }

        /// <summary>
        /// Sets the absolute scale of the specified game object.
        /// </summary>
        public static void SetAbsoluteScale(this GameObject gameObject, Vector3 absoluteScale)
        {
            // Cache needed data
            Transform objectTransform = gameObject.transform;
            Transform oldParent = objectTransform.parent;

            // In order to set the absolute scale, we will first detach the object from its
            // parent and then use the 'localScale' property to set the scale. We then attach
            // the game object to its parent again.
            objectTransform.parent = null;
            objectTransform.localScale = absoluteScale;
            objectTransform.parent = oldParent;
        }

        /// <summary>
        /// Rotates 'gameObject' around 'rotationAxis' by 'angleInDegrees'. 
        /// </summary>
        /// <remarks>
        /// The function rotates both the object's orientation and its position.
        /// </remarks>
        /// <param name="gameObject">
        /// The game object which must be rotated.
        /// </param>
        /// <param name="rotationAxis">
        /// The rotation axis. The function assumes the rotation axis is normalized.
        /// </param>
        /// <param name="angleInDegrees">
        /// The angle of rotation in degrees.
        /// </param>
        /// <param name="pivotPoint">
        /// This is point around which the rotation is performed.
        /// </param>
        public static void Rotate(this GameObject gameObject, Vector3 rotationAxis, float angleInDegrees, Vector3 pivotPoint)
        {
            // Cache needed data
            Transform objectTransform = gameObject.transform;

            // Calculate the vector which holds the relationship between the pivot point and the object's position
            Vector3 fromPivotToPosition = objectTransform.position - pivotPoint;

            // Rotate the relationship vector. We need to do this because after the rotation is applied to the
            // game object, we need to also adjust its position in such a way that the rotation happens around
            // the pivot point.
            Quaternion rotationQuaternion = Quaternion.AngleAxis(angleInDegrees, rotationAxis);
            fromPivotToPosition = rotationQuaternion * fromPivotToPosition;

            // Rotate the object's local coordinate system
            objectTransform.Rotate(rotationAxis, angleInDegrees, Space.World);

            // Now adjust the position. The new position is the pivot point + the transformed relationshop vector. This
            // has the effect of locking the object's position to the tip of the vector which unites the pivot and the
            // position. Regardless of how the relationshop vector is rotated, the position vector will rotate with it 
            // around the pivot point.
            objectTransform.position = pivotPoint + fromPivotToPosition;
        }

        /// <summary>
        /// Returns the specified game object's screen rectangle.
        /// </summary>
        /// <param name="camera">
        /// The camera which renders the game object.
        /// </param>
        /// <remarks>
        /// If the specified game object doesn't have a mesh attached to it or a box,
        /// sphere or capsule collider, the function will return an empty rectangle.
        /// </remarks>
        public static Rect GetScreenRectangle(this GameObject gameObject, Camera camera)
        {
            // Retrieve the game object's world space AABB
            Bounds worldSpaceAABB = gameObject.GetWorldBox().ToBounds();
            if (!worldSpaceAABB.IsValid()) return new Rect(0.0f, 0.0f, 0.0f, 0.0f);

            // Return the rectangle which encloses the world space AABB in screen space
            return worldSpaceAABB.GetScreenRectangle(camera);
        }

        public static OrientedBox GetWorldOrientedBox(this GameObject gameObject)
        {
            OrientedBox worldOrientedBox = gameObject.GetMeshWorldOrientedBox();
            if (worldOrientedBox.IsValid()) return worldOrientedBox;

            return gameObject.GetNonMeshWorldOrientedBox();
        }

        public static Box GetWorldBox(this GameObject gameObject)
        {
            Box worldBox = gameObject.GetMeshWorldBox();
            if (worldBox.IsValid()) return worldBox;

            return gameObject.GetNonMeshWorldBox();
        }

        public static OrientedBox GetModelSpaceOrientedBox(this GameObject gameObject)
        {
            OrientedBox modelSpaceOrientedBox = gameObject.GetMeshModelSpaceOrientedBox();
            if (modelSpaceOrientedBox.IsValid()) return modelSpaceOrientedBox;

            return gameObject.GetNonMeshModelSpaceOrientedBox();
        }

        public static Box GetModelSpaceBox(this GameObject gameObject)
        {
            Box modelSpaceBox = gameObject.GetMeshModelSpaceBox();
            if (modelSpaceBox.IsValid()) return modelSpaceBox;

            return gameObject.GetNonMeshModelSpaceBox();
        }

        public static OrientedBox GetMeshWorldOrientedBox(this GameObject gameObject)
        {
            Mesh mesh = gameObject.GetMeshFromMeshFilter();
            if (mesh != null) return new OrientedBox(new Box(mesh.bounds), gameObject.transform);

            mesh = gameObject.GetMeshFromSkinnedMeshRenderer();
            //if (mesh != null) return new OrientedBox(new Box(gameObject.GetComponent<SkinnedMeshRenderer>().localBounds), gameObject.transform);
            if (mesh != null) return new OrientedBox(new Box(mesh.bounds), gameObject.transform);

            return OrientedBox.GetInvalid();
        }

        public static Box GetMeshWorldBox(this GameObject gameObject)
        {
            Mesh mesh = gameObject.GetMeshFromMeshFilter();
            if (mesh != null) return new Box(mesh.bounds).Transform(gameObject.transform.GetWorldMatrix());

            mesh = gameObject.GetMeshFromSkinnedMeshRenderer();
            //if (mesh != null) return new Box(gameObject.GetComponent<SkinnedMeshRenderer>().localBounds).Transform(gameObject.transform.GetWorldMatrix());
            if (mesh != null) return new Box(mesh.bounds).Transform(gameObject.transform.GetWorldMatrix());

            return Box.GetInvalid();
        }

        public static OrientedBox GetNonMeshWorldOrientedBox(this GameObject gameObject)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return new OrientedBox(Box.FromBounds(spriteRenderer.GetModelSpaceBounds()), gameObject.transform);
            }
            else
            {
                OrientedBox modelSpaceOrientedBox = gameObject.GetNonMeshModelSpaceOrientedBox();
                if (!modelSpaceOrientedBox.IsValid()) return modelSpaceOrientedBox;

                OrientedBox worldOrientedBox = new OrientedBox(modelSpaceOrientedBox);
                Transform objectTransform = gameObject.transform;
                worldOrientedBox.Center = objectTransform.position;
                worldOrientedBox.Rotation = objectTransform.rotation;
                worldOrientedBox.Scale = objectTransform.lossyScale;

                return worldOrientedBox;
            }
        }

        public static Box GetNonMeshWorldBox(this GameObject gameObject)
        {
            Box modelSpaceBox = gameObject.GetNonMeshModelSpaceBox();
            if (!modelSpaceBox.IsValid()) return modelSpaceBox;

            Box worldBox = modelSpaceBox.Transform(gameObject.transform.GetWorldMatrix());
            return worldBox;
        }

        public static OrientedBox GetMeshModelSpaceOrientedBox(this GameObject gameObject)
        {
            Mesh mesh = gameObject.GetMeshFromMeshFilter();
            if (mesh != null) return new OrientedBox(new Box(mesh.bounds), Quaternion.identity);

            mesh = gameObject.GetMeshFromSkinnedMeshRenderer();
            //if (mesh != null) return new OrientedBox(new Box(gameObject.GetComponent<SkinnedMeshRenderer>().localBounds), Quaternion.identity);
            if (mesh != null) return new OrientedBox(new Box(mesh.bounds), Quaternion.identity);

            return OrientedBox.GetInvalid();
        }

        public static Box GetMeshModelSpaceBox(this GameObject gameObject)
        {
            Mesh mesh = gameObject.GetMeshFromMeshFilter();
            if (mesh != null) return new Box(mesh.bounds);

            mesh = gameObject.GetMeshFromSkinnedMeshRenderer();
            //if (mesh != null) return new Box(gameObject.GetComponent<SkinnedMeshRenderer>().localBounds);
            if (mesh != null) return new Box(mesh.bounds);

            return Box.GetInvalid();
        }

        public static OrientedBox GetNonMeshModelSpaceOrientedBox(this GameObject gameObject)
        {
            return new OrientedBox(gameObject.GetNonMeshModelSpaceBox());
        }

        public static Box GetNonMeshModelSpaceBox(this GameObject gameObject)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) return Box.FromBounds(spriteRenderer.GetModelSpaceBounds());

            if (gameObject.HasTerrain())
            {
                Terrain terrain = gameObject.GetComponent<Terrain>();
                TerrainData terrainData = terrain.terrainData;
                Vector3 terrainSize = new Vector3(terrainData.size.x, 1.0f, terrainData.size.z);

                if (terrainData != null) return new Box(terrainSize * 0.5f, terrainSize);
            }

            if (gameObject.HasLight()) return new Box(Vector3.zero, RuntimeEditorApplication.Instance.VolumeSizeForLightObjects);
            if (gameObject.HasParticleSystem()) return new Box(Vector3.zero, RuntimeEditorApplication.Instance.VolumeSizeForParticleSystemObjects);
            return new Box(Vector3.zero, RuntimeEditorApplication.Instance.VolumeSizeForEmptyObjects);
        }
      
        public static bool IsEmpty(this GameObject gameObject)
        {
            if (gameObject.HasMesh()) return false;
            if (gameObject.HasTerrain()) return false;
            if (gameObject.HasLight()) return false;
            if (gameObject.HasParticleSystem()) return false;
            if (gameObject.IsSprite()) return false;

            return true;
        }

        /// <summary>
        /// Given a game object, the function will assign it and its children to the specified layer.
        /// </summary>
        public static void SetLayerForEntireHierarchy(this GameObject gameObject, int layer)
        {
            // Assign the specified game object to the specified layer
            gameObject.layer = layer;

            // Assign the objects children to the specified layer
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform objectTransform in transforms)
            {
                objectTransform.gameObject.layer = layer;
            }
        }

        /// <summary>
        /// Returns the model space AABB of the hierarchy whose root is 'gameObject'.
        /// </summary>
        public static Box GetHierarchyModelSpaceBox(this GameObject gameObject)
        {
            // Retrieve all child transforms
            Transform rootTransform = gameObject.transform;
            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();

            // Initialize the hierarchy model space AAB with the model space AABB of the root.
            // This AABB will be adjusted to encapsulate the AABB of the entire object hierarchy.
            Box hierarchyModelSpaceBox = gameObject.GetModelSpaceBox();

            // Loop through all child transforms
            foreach (Transform childTransform in childTransforms)
            {
                // Skip the root transform because we already handled this outside the 'foreach' loop
                if (rootTransform != childTransform)
                {
                    // Store child object for easy access
                    GameObject childObject = childTransform.gameObject;

                    // Retrieve the child's model space AABB. If it is valid, we can continue. Otherwise, we ignore it and move on.
                    Box childModelSpaceBox = childObject.GetModelSpaceBox();
                    if (childModelSpaceBox.IsValid())
                    {
                        // The AABB is valud. The next step is to calculate a transform matrix which places the child's
                        // AABB in hierarchy model space. This is the matrix which contains the transform relative to the
                        // root of the hierarchy.
                        Matrix4x4 rootRelativeTransformMatrix = childTransform.GetRelativeTransform(rootTransform);
                        childModelSpaceBox = childModelSpaceBox.Transform(rootRelativeTransformMatrix);

                        // If the hierarchy AABB is valid, we can perform a merge. Otherwise, we must initialize it.
                        if (hierarchyModelSpaceBox.IsValid()) hierarchyModelSpaceBox.Encapsulate(childModelSpaceBox);
                        else hierarchyModelSpaceBox = childModelSpaceBox;
                    }
                }
            }

            return hierarchyModelSpaceBox;
        }
     
        /// <summary>
        /// Returns the mesh attached to the specified game object. If the game object has both
        /// a mesh filter and a skinned mesh renderer attached to it, the mesh associated with the
        /// mesh filter will be returned if one is present. Otherwise, the skinned mesh renderer's
        /// mesh will be returned. If none of them have a valid mesh, or if no mesh filter or
        /// skinned mesh renderer are present, the function will return null.
        /// </summary>
        public static Mesh GetMesh(this GameObject gameObject)
        {
            // Check if the object has a mesh filter component with a valid mesh attached to it
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null) return meshFilter.sharedMesh;

            // The game object may have a skinned mesh renderer with a valid mesh
            SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null) return skinnedMeshRenderer.sharedMesh;

            return null;
        }

        public static Renderer GetRenderer(this GameObject gameObject)
        {
            return gameObject.GetComponent<Renderer>();
        }

        public static bool IsSprite(this GameObject gameObject)
        {
            if (gameObject.HasMesh()) return false;

            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            return spriteRenderer != null && spriteRenderer.sprite != null;
        }

        public static bool IsRTEditorSystemObject(this GameObject gameObject)
        {
            var runtimeEditorApp = RuntimeEditorApplication.Instance;
            if (runtimeEditorApp.gameObject == gameObject) return true;

            if (gameObject.transform.IsChildOf(runtimeEditorApp.transform)) return true;
            if (gameObject.GetComponent<Gizmo>() != null) return true;

            return false;
        }

        public static Mesh GetMeshFromFilterOrSkinnedMeshRenderer(this GameObject gameObject)
        {
            Mesh mesh = gameObject.GetMeshFromMeshFilter();
            if (mesh == null) mesh = gameObject.GetMeshFromSkinnedMeshRenderer();

            return mesh;
        }

        public static Mesh GetMeshFromMeshFilter(this GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null) return meshFilter.sharedMesh;

            return null;
        }

        public static Mesh GetMeshFromSkinnedMeshRenderer(this GameObject gameObject)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null) return skinnedMeshRenderer.sharedMesh;

            return null;
        }

        public static bool HasMesh(this GameObject gameObject)
        {
            return gameObject.HasMeshFilterWithValidMesh() || gameObject.HasSkinnedMeshRendererWithValidMesh();
        }

        public static bool HasMeshFilterWithValidMesh(this GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            return meshFilter != null && meshFilter.sharedMesh != null;
        }

        public static bool HasSkinnedMeshRendererWithValidMesh(this GameObject gameObject)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            return skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null;
        }

        public static bool HasTerrain(this GameObject gameObject)
        {
            return gameObject.GetComponent<Terrain>() != null;
        }

        public static bool HasLight(this GameObject gameObject)
        {
            return gameObject.GetComponent<Light>() != null;
        }

        public static bool HasParticleSystem(this GameObject gameObject)
        {
            return gameObject.GetComponent<ParticleSystem>() != null;
        }

        public static bool HasSpriteRenderer(this GameObject gameObject)
        {
            return gameObject.GetComponent<SpriteRenderer>() != null;
        }

        public static bool HasCamera(this GameObject gameObject)
        {
            return gameObject.GetComponent<Camera>() != null;
        }

        public static bool HasSpriteRendererWithSprite(this GameObject gameObject)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return false;

            return spriteRenderer.sprite != null;
        }

        /// <summary>
        /// Removes all attached colliders from the specified game object.
        /// </summary>
        public static void RemoveAllColliders(this GameObject gameObject)
        {
            // Loop through all colliders and destroy them
            Collider[] all3DColliders = gameObject.GetComponents<Collider>();
            foreach (Collider collider3D in all3DColliders)
            {
                // Destory collider
                #if UNITY_EDITOR
                RuntimeEditorApplication.DestroyImmediate(collider3D);
                #else
                RuntimeEditorApplication.Destroy(collider3D);
                #endif
            }

            Collider2D[] all2DColliders = gameObject.GetComponents<Collider2D>();
            foreach (Collider2D collider2D in all2DColliders)
            {
                // Destory collider
                #if UNITY_EDITOR
                RuntimeEditorApplication.DestroyImmediate(collider2D);
                #else
                RuntimeEditorApplication.Destroy(collider2D);
                #endif
            }
        }

        /// <summary>
        /// Destroys all children of the specified game object.
        /// </summary>
        public static void DestroyAllChildren(this GameObject gameObject)
        {
            // Loop through all child transforms
            Transform objectTransform = gameObject.transform;
            Transform[] allChildTransforms = gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform childTransform in allChildTransforms)
            {
                // Same as parent object?
                if (objectTransform == childTransform) continue;

                // Destroy object
                #if UNITY_EDITOR
                RuntimeEditorApplication.DestroyImmediate(childTransform.gameObject);
                #else
                RuntimeEditorApplication.Destroy(childTransform.gameObject);
                #endif
            }
        }

        /// <summary>
        /// Returns all the light components which are attached to the specified game object. If
        /// no light component is attached to the game object, the returned array will be empty.
        /// </summary>
        public static Light[] GetAllLightComponents(this GameObject gameObject)
        {
            return gameObject.GetComponents<Light>();
        }

        /// <summary>
        /// Returns the first encountered light component which is attached to the specified game
        /// object. If no light component is attached to the game object, the function will return
        /// null.
        /// </summary>
        public static Light GetFirstLightComponent(this GameObject gameObject)
        {
            Light[] allLightComponents = gameObject.GetAllLightComponents();
            if (allLightComponents.Length != 0) return allLightComponents[0];

            return null;
        }

        /// <summary>
        /// Returns all the particle system components which are attached to the specified game object. If
        /// no particle system component is attached to the game object, the returned array will be empty.
        /// </summary>
        public static ParticleSystem[] GetAllParticleSystemComponents(this GameObject gameObject)
        {
            return gameObject.GetComponents<ParticleSystem>();
        }

        /// <summary>
        /// Returns the first encountered particle system component which is attached to the specified game
        /// object. If no particle system component is attached to the game object, the function will return
        /// null.
        /// </summary>
        public static ParticleSystem GetFirstParticleSystemComponent(this GameObject gameObject)
        {
            ParticleSystem[] allParticleSystemComponents = gameObject.GetAllParticleSystemComponents();
            if (allParticleSystemComponents.Length != 0) return allParticleSystemComponents[0];

            return null;
        }
        #endregion
    }
}
