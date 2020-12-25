using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// This class is used to hold different types of settings which relate
    /// to the startup of the editor application. It allows you to control
    /// certain actions that need or need not happen at application startup.
    /// </summary>
    [Serializable]
    public class RuntimeEditorApplicationStartupSettings
    {
        #region Private Variables
        /// <summary>
        /// If this is set to true, all the necessary information that is needed
        /// to perform vertex snapping will be acquired on application startup.
        /// </summary>
        [SerializeField]
        private bool _acquireVertexSnappingInfoOnStartup = true;

        /// <summary>
        /// If this is set to true, object colliders will be attached to all scene objects at
        /// startup using the specified object collider attachment settings.
        /// </summary>
        [SerializeField]
        private bool _attachObjectCollidersToAllSceneObjectsAtStartup = true;

        /// <summary>
        /// This holds object collider settings which are used to attach colliders to all game
        /// objects in the scene at startup if '_attachObjectCollidersAtStartup' is true.
        /// </summary>
        [SerializeField]
        private ObjectColliderAttachmentSettings _objectColliderAttachmentSettings = new ObjectColliderAttachmentSettings();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not vertex snapping info must be
        /// acquired at startup.
        /// </summary>
        public bool AcquireVertexSnappingInfoOnStartup { get { return _acquireVertexSnappingInfoOnStartup; } set { _acquireVertexSnappingInfoOnStartup = value; } }

        /// <summary>
        /// Gets/sets the boolean flag which specifies whether or not object colliders must be attached
        /// to all scene objects at startup.
        /// </summary>
        public bool AttachObjectCollidersToAllSceneObjectsAtStartup { get { return _attachObjectCollidersToAllSceneObjectsAtStartup; } set { _attachObjectCollidersToAllSceneObjectsAtStartup = value; } }

        /// <summary>
        /// Returns the object collider attachment startup settings.
        /// </summary>
        public ObjectColliderAttachmentSettings ObjectColliderAttachmentSettings { get { return _objectColliderAttachmentSettings; } }
        #endregion
    }
}
