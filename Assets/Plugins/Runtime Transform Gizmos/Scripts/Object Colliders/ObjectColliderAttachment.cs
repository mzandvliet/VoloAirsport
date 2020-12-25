using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is the module which handles object collider attachment based on specified
    /// collider attachment settings.
    /// </summary>
    public class ObjectColliderAttachment : SingletonBase<ObjectColliderAttachment>
    {
        #region Public Methods
        /// <summary>
        /// Attaches a collider to all scene objects using the specified collider
        /// attachment settings.
        /// </summary>
        /// <remarks>
        /// The method will remove any existing object colliders. Also, the method 
        /// will attach colliders only to mesh, light and particle system objects.
        /// If an object has 2 or more of these components attached, the colliders
        /// will be attached in the following manner:
        ///     a) if the object has a mesh attached to it (i.e. a mesh filter with
        ///        a valid mesh or a skinned mesh renderer with a valid mesh), the
        ///        mesh collider attachement settings will be used;
        ///     b) if the object doesn't have a mesh, but it has a light component,
        ///        the light collider attachement settings will be used;
        ///     c) if the object doesn't have a light either, but it has a particle
        ///        system, the particle system collider attachment settings will be used.
        /// </remarks>
        public void AttachCollidersToAllSceneObjects(ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            // Loop through all scene objects and attach colliders
            GameObject[] sceneObjects = RuntimeEditorApplication.FindObjectsOfType<GameObject>();
            foreach (GameObject gameObject in sceneObjects)
            {
                AttachColliderToGameObject(gameObject, colliderAttachmentSettings);
            }
        }

        /// <summary>
        /// Attaches a collider to the objects that belong to the specified object hierarchy using
        /// the specified object collider attachment settings.
        /// </summary>
        /// <param name="hierarchyRoot">
        /// The root object of the hierarchy whose objects must have colliders attached to them.
        /// </param>
        /// <remarks>
        /// The method will remove any existing object colliders. Also, the method 
        /// will attach colliders only to mesh, light and particle system objects.
        /// If an object has 2 or more of these components attached, the colliders
        /// will be attached in the following manner:
        ///     a) if the object has a mesh attached to it (i.e. a mesh filter with
        ///        a valid mesh or a skinned mesh renderer with a valid mesh), the
        ///        mesh collider attachement settings will be used;
        ///     b) if the object doesn't have a mesh, but it has a light component,
        ///        the light collider attachement settings will be used;
        ///     c) if the object doesn't have a light either, but it has a particle
        ///        system, the particle system collider attachment settings will be used.
        /// </remarks>
        public void AttachCollidersToObjectHierarchy(GameObject hierarchyRoot, ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            // Loop through all objects in the hierarchy and attach colliders
            Transform[] allChildTransforms = hierarchyRoot.GetComponentsInChildren<Transform>(true);
            foreach(Transform childTransform in allChildTransforms)
            {
                GameObject gameObject = childTransform.gameObject;
                AttachColliderToGameObject(gameObject, colliderAttachmentSettings);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Attaches a collider to the specified game object based on the specified object collider
        /// attachment settings.
        /// </summary>
        private void AttachColliderToGameObject(GameObject gameObject, ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            // Attach a collider to the game object based on the type of object we are dealing with.
            // Note: We also remove all attached colliders before attaching new ones and we also check
            //       if the collider attachement settings allow us to modify the object collider.
            if(gameObject.GetComponent<SpriteRenderer>() != null && !colliderAttachmentSettings.IgnoreSpriteObjects)
            {
                gameObject.RemoveAllColliders();
                AttachColliderToSpriteObject(gameObject, colliderAttachmentSettings);
            }
            else
            if (gameObject.GetMesh() != null && !colliderAttachmentSettings.IgnoreMeshObjects)
            {
                gameObject.RemoveAllColliders();
                AttachColliderToMeshObject(gameObject, colliderAttachmentSettings);
            }
            else
            if (gameObject.GetFirstLightComponent() != null && !colliderAttachmentSettings.IgnoreLightObjects)
            {
                gameObject.RemoveAllColliders();
                AttachColliderToLightObject(gameObject, colliderAttachmentSettings);
            }
            else
            if (gameObject.GetFirstParticleSystemComponent() != null && !colliderAttachmentSettings.IgnoreParticleSystemObjects)
            {
                gameObject.RemoveAllColliders();
                AttachColliderToParticleSystemObject(gameObject, colliderAttachmentSettings);
            }
        }

        /// <summary>
        /// Attaches a collider to the specified mesh object based on the specified settings.
        /// </summary>
        private void AttachColliderToMeshObject(GameObject gameObject, ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            // Attach a new collider based on the required collider type
            if (colliderAttachmentSettings.ColliderTypeForMeshObjects == ObjectCollider3DType.MeshCollider) gameObject.AddComponent<MeshCollider>();
            else if (colliderAttachmentSettings.ColliderTypeForMeshObjects == ObjectCollider3DType.Box) gameObject.AddComponent<BoxCollider>();
            else if (colliderAttachmentSettings.ColliderTypeForMeshObjects == ObjectCollider3DType.Sphere) gameObject.AddComponent<SphereCollider>();
            else if (colliderAttachmentSettings.ColliderTypeForMeshObjects == ObjectCollider3DType.Capsule) gameObject.AddComponent<CapsuleCollider>();
        }

        private void AttachColliderToSpriteObject(GameObject gameObject, ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            if (colliderAttachmentSettings.ColliderTypeForSpriteObjects == ObjectCollider2DType.Box) gameObject.AddComponent<BoxCollider2D>();
            else if (colliderAttachmentSettings.ColliderTypeForSpriteObjects == ObjectCollider2DType.Circle) gameObject.AddComponent<CircleCollider2D>();
            else if (colliderAttachmentSettings.ColliderTypeForSpriteObjects == ObjectCollider2DType.Polygon) gameObject.AddComponent<PolygonCollider2D>();
        }

        /// <summary>
        /// Attaches a collider to the specified light object based on the specified settings.
        /// </summary>
        /// <remarks>
        /// If the collider attachment settings specify a mesh collider for light objects, a 
        /// box collider will be used instead.
        /// </remarks>
        private void AttachColliderToLightObject(GameObject gameObject, ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            // Attach a new collider based on the required collider type
            if(colliderAttachmentSettings.ColliderTypeForLightObjects == ObjectCollider3DType.Box || 
               colliderAttachmentSettings.ColliderTypeForLightObjects == ObjectCollider3DType.MeshCollider)
            {
                // Attach the box collider and set its size to what the settings dictate
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = colliderAttachmentSettings.BoxColliderSizeForNonMeshObjects;
            }
            else
            if(colliderAttachmentSettings.ColliderTypeForLightObjects == ObjectCollider3DType.Sphere)
            {
                // Attach the sphere collider and set its radius to what the settings dictate
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = colliderAttachmentSettings.SphereColliderRadiusForNonMeshObjects;
            }
            else
            if(colliderAttachmentSettings.ColliderTypeForLightObjects == ObjectCollider3DType.Capsule)
            {
                // Attach the capsule collider and set its radius ad height to what the settings dictate
                CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = colliderAttachmentSettings.CapsuleColliderRadiusForNonMeshObjects;
                capsuleCollider.height = colliderAttachmentSettings.CapsuleColliderHeightForNonMeshObjects;
            }
        }

        /// <summary>
        /// Attaches a collider to the specified particle system object based on the specified settings.
        /// </summary>
        /// <remarks>
        /// If the collider attachment settings specify a mesh collider for particle system objects, a 
        /// box collider will be used instead.
        /// </remarks>
        private void AttachColliderToParticleSystemObject(GameObject gameObject, ObjectColliderAttachmentSettings colliderAttachmentSettings)
        {
            // Attach a new collider based on the required collider type
            if (colliderAttachmentSettings.ColliderTypeForParticleSystemObjects == ObjectCollider3DType.Box ||
                colliderAttachmentSettings.ColliderTypeForParticleSystemObjects == ObjectCollider3DType.MeshCollider)
            {
                // Attach the box collider and set its size to what the settings dictate
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = colliderAttachmentSettings.BoxColliderSizeForNonMeshObjects;
            }
            else
            if (colliderAttachmentSettings.ColliderTypeForParticleSystemObjects == ObjectCollider3DType.Sphere)
            {
                // Attach the sphere collider and set its radius to what the settings dictate
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = colliderAttachmentSettings.SphereColliderRadiusForNonMeshObjects;
            }
            else
            if (colliderAttachmentSettings.ColliderTypeForParticleSystemObjects == ObjectCollider3DType.Capsule)
            {
                // Attach the capsule collider and set its radius ad height to what the settings dictate
                CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = colliderAttachmentSettings.CapsuleColliderRadiusForNonMeshObjects;
                capsuleCollider.height = colliderAttachmentSettings.CapsuleColliderHeightForNonMeshObjects;
            }
        }
        #endregion
    }
}
