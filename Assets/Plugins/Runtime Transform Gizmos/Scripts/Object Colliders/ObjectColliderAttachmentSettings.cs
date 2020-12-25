using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// This class holds settings which control the way in which object colliders get attached
    /// to game objects.
    /// </summary>
    [Serializable]
    public class ObjectColliderAttachmentSettings
    {
        #region Private Variables
        /// <summary>
        /// The collider type which must be attached to mesh objects.
        /// </summary>
        [SerializeField]
        private ObjectCollider3DType _colliderTypeForMeshObjects = ObjectCollider3DType.Box;

        /// <summary>
        /// The collider type which must be attached to objects that have a light component attached to them.
        /// </summary>
        [SerializeField]
        private ObjectCollider3DType _colliderTypeForLightObjects = ObjectCollider3DType.Box;

        /// <summary>
        /// The collider type which must be attached to objects that have a particle system component attached to them.
        /// </summary>
        [SerializeField]
        private ObjectCollider3DType _colliderTypeForParticleSystemObjects = ObjectCollider3DType.Box;

        [SerializeField]
        private ObjectCollider2DType _colliderTypeForSpriteObjects = ObjectCollider2DType.Box;

        /// <summary>
        /// This represents the size of a box collider that gets attached to non-mesh objects.
        /// </summary>
        [SerializeField]
        private Vector3 _boxColliderSizeForNonMeshObjects = Vector3.one;

        /// <summary>
        /// This represents the radius of a sphere collider that gets attached to non-mesh objects.
        /// </summary>
        [SerializeField]
        private float _sphereColliderRadiusForNonMeshObjects = 1.0f;

        /// <summary>
        /// This represents the radius of a capsule collider that gets attached to non-mesh objects.
        /// </summary>
        [SerializeField]
        private float _capsuleColliderRadiusForNonMeshObjects = 1.0f;

        /// <summary>
        /// This represents the height of a capsule collider that gets attached to non-mesh objects.
        /// </summary>
        [SerializeField]
        private float _capsuleColliderHeightForNonMeshObjects = 1.0f;

        /// <summary>
        /// If this is true, the collider attachment module will ignore mesh objects and it will
        /// not touch their colliders.
        /// </summary>
        [SerializeField]
        private bool _ignoreMeshObjects = false;

        /// <summary>
        /// If this is true, the collider attachment module will ignore light objects and it will
        /// not touch their colliders.
        /// </summary>
        [SerializeField]
        private bool _ignoreLightObjects = true;

        /// <summary>
        /// If this is true, the collider attachment module will ignore particle system objects and 
        /// it will not touch their colliders.
        /// </summary>
        [SerializeField]
        private bool _ignoreParticleSystemObjects = true;

        [SerializeField]
        private bool _ignoreSpriteObjects = false;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimim size for box colliders that get attached to non-mesh objects.
        /// </summary>
        public static Vector3 MinBoxColliderSizeForNonMeshObjects { get { return Vector3.one * 0.1f; } }

        /// <summary>
        /// Returns the minimum radius for a sphere colider that gets attached to non-mesh objects.
        /// </summary>
        public static float MinSphereColliderRadiusForNonMeshObjects { get { return 0.1f; } }

        /// <summary>
        /// Returns the minimum radius for a capsule colider that gets attached to non-mesh objects.
        /// </summary>
        public static float MinCapsuleColliderRadiusForNonMeshObjects { get { return 0.1f; } }

        /// <summary>
        /// Returns the minimum radius for a capsule colider that gets attached to non-mesh objects.
        /// </summary>
        public static float MinCapsuleColliderHeightForNonMeshObjects { get { return 0.1f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the type of collider which must be attached to mesh objects.
        /// </summary>
        public ObjectCollider3DType ColliderTypeForMeshObjects { get { return _colliderTypeForMeshObjects; } set { _colliderTypeForMeshObjects = value; } }

        /// <summary>
        /// Gets/sets the type of collider which must be attached to light objects.
        /// </summary>
        public ObjectCollider3DType ColliderTypeForLightObjects { get { return _colliderTypeForLightObjects; } set { _colliderTypeForLightObjects = value; } }

        /// <summary>
        /// Gets/sets the type of collider which must be attached to particle system objects.
        /// </summary>
        public ObjectCollider3DType ColliderTypeForParticleSystemObjects { get { return _colliderTypeForParticleSystemObjects; } set { _colliderTypeForParticleSystemObjects = value; } }

        public ObjectCollider2DType ColliderTypeForSpriteObjects { get { return _colliderTypeForSpriteObjects; } set { _colliderTypeForSpriteObjects = value; } }

        /// <summary>
        /// Get/sets the box collider size for non-mesh objects. The minimum value for the collider size
        /// is given by the 'MinBoxColliderSizeForNonMeshObjects' property. Values smaller than that will
        /// be clamped accordingly.
        /// </summary>
        public Vector3 BoxColliderSizeForNonMeshObjects { get { return _boxColliderSizeForNonMeshObjects; } set { _boxColliderSizeForNonMeshObjects = Vector3.Max(value, MinBoxColliderSizeForNonMeshObjects); } }

        /// <summary>
        /// Gets/sets the sphere collider radius for non-mesh objects. The minimum value for the radius
        /// is given by the 'MinSphereColliderRadiusForNonMeshObjects' property. Values smaller than that 
        /// will be clamped accordingly.
        /// </summary>
        public float SphereColliderRadiusForNonMeshObjects { get { return _sphereColliderRadiusForNonMeshObjects; } set { _sphereColliderRadiusForNonMeshObjects = Mathf.Max(value, MinSphereColliderRadiusForNonMeshObjects); } }

        /// <summary>
        /// Gets/sets the capsule collider radius for non-mesh objects. The minimum value for the radius
        /// is given by the 'MinCapsuleColliderRadiusForNonMeshObjects' property. Values smaller than that 
        /// will be clamped accordingly.
        /// </summary>
        public float CapsuleColliderRadiusForNonMeshObjects { get { return _capsuleColliderRadiusForNonMeshObjects; } set { _capsuleColliderRadiusForNonMeshObjects = Mathf.Max(value, MinCapsuleColliderRadiusForNonMeshObjects); } }

        /// <summary>
        /// Gets/sets the capsule collider height for non-mesh objects. The minimum value for the radius
        /// is given by the 'MinCapsuleColliderHeightForNonMeshObjects' property. Values smaller than that 
        /// will be clamped accordingly.
        /// </summary>
        public float CapsuleColliderHeightForNonMeshObjects { get { return _capsuleColliderHeightForNonMeshObjects; } set { _capsuleColliderHeightForNonMeshObjects = Mathf.Max(value, MinCapsuleColliderHeightForNonMeshObjects); } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not mesh objects should be
        /// ignored during the collider attachment process.
        /// </summary>
        public bool IgnoreMeshObjects { get { return _ignoreMeshObjects; } set { _ignoreMeshObjects = value; } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not light objects should be
        /// ignored during the collider attachment process.
        /// </summary>
        public bool IgnoreLightObjects { get { return _ignoreLightObjects; } set { _ignoreLightObjects = value; } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not particle system objects should be
        /// ignored during the collider attachment process.
        /// </summary>
        public bool IgnoreParticleSystemObjects { get { return _ignoreParticleSystemObjects; } set { _ignoreParticleSystemObjects = value; } }

        public bool IgnoreSpriteObjects { get { return _ignoreSpriteObjects; } set { _ignoreSpriteObjects = value; } }
        #endregion
    }
}
