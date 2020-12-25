using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// Implements the behaviour for the runtime editor application. You can add functionality
    /// to this class to suit your own needs.
    /// </summary>
    [Serializable]
    public class RuntimeEditorApplication : MonoSingletonBase<RuntimeEditorApplication>
    {
        #region Private Variables
        [SerializeField]
        private Vector3 _volumeSizeForLightObjects = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField]
        private Vector3 _volumeSizeForParticleSystemObjects = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField]
        private Vector3 _volumeSizeForEmptyObjects = new Vector3(0.5f, 0.5f, 0.5f);

        [SerializeField]
        private XZGrid _xzGrid = new XZGrid();
        #endregion

        #region Public Static Properties
        public static Vector3 MinObjectVolumeSize { get { return new Vector3(0.001f, 0.001f, 0.001f); } }
        #endregion

        #region Public Properties
        public Vector3 VolumeSizeForLightObjects { get { return _volumeSizeForLightObjects; } set { _volumeSizeForLightObjects = Vector3.Max(MinObjectVolumeSize, value.GetVectorWithAbsComponents()); } }
        public Vector3 VolumeSizeForParticleSystemObjects { get { return _volumeSizeForParticleSystemObjects; } set { _volumeSizeForParticleSystemObjects = Vector3.Max(MinObjectVolumeSize, value.GetVectorWithAbsComponents()); } }
        public Vector3 VolumeSizeForEmptyObjects { get { return _volumeSizeForEmptyObjects; } set { _volumeSizeForEmptyObjects = Vector3.Max(MinObjectVolumeSize, value.GetVectorWithAbsComponents()); } }
        public XZGrid XZGrid { get { return _xzGrid; } }
        #endregion

        #region Private Methods
        private void Update()
        {
            EditorScene.Instance.Update();
        }
        #endregion

        #if UNITY_EDITOR
        #region Menu Items
        /// <summary>
        /// Creates all the necessary subsystems which are needed for the runtime editor.
        /// </summary>
        [MenuItem("Tools/Runtime Transform Gizmos/Initialize")]
        private static void CreateSubsystems()
        {
            CreateRuntimeEditorApplicationSubsystems();
        }
        #endregion
        #endif

        #region Private Static Functions
        #if UNITY_EDITOR
        /// <summary>
        /// Creates all the necessary runtime editor subsystems.
        /// </summary>
        private static void CreateRuntimeEditorApplicationSubsystems()
        {
            // First, make sure all existing subsystems are destroyed
            DestroyExistingSubsystems();

            // Now, create each subsystem  
            RuntimeEditorApplication runtimeEditorApplication = RuntimeEditorApplication.Instance;
            Transform runtimeEditorApplicationTransform = runtimeEditorApplication.transform;

            EditorGizmoSystem editorGizmoSystem = EditorGizmoSystem.Instance;
            editorGizmoSystem.transform.parent = runtimeEditorApplicationTransform;

            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;
            editorObjectSelection.transform.parent = runtimeEditorApplicationTransform;

            EditorCamera editorCamera = EditorCamera.Instance;
            editorCamera.transform.parent = runtimeEditorApplicationTransform;
            editorCamera.gameObject.AddComponent<Camera>();

            EditorUndoRedoSystem editorUndoRedoSystem = EditorUndoRedoSystem.Instance;
            editorUndoRedoSystem.transform.parent = runtimeEditorApplicationTransform;

            EditorShortuctKeys editorShortcutKeys = EditorShortuctKeys.Instance;
            editorShortcutKeys.transform.parent = runtimeEditorApplicationTransform;

            EditorMeshDatabase editorMeshDatabase = EditorMeshDatabase.Instance;
            editorMeshDatabase.transform.parent = runtimeEditorApplicationTransform;

            // Create all gizmos and attach them to the gizmo system
            GameObject gizmoObject = new GameObject();
            gizmoObject.name = "Translation Gizmo";
//            TranslationGizmo translationGizmo = gizmoObject.AddComponent<TranslationGizmo>();
//            editorGizmoSystem.TranslationGizmo = translationGizmo;

            gizmoObject = new GameObject();
            gizmoObject.name = "Rotation Gizmo";
            RotationGizmo rotationGizmo = gizmoObject.AddComponent<RotationGizmo>();
            rotationGizmo.GizmoBaseScale = 1.3f;
            editorGizmoSystem.RotationGizmo = rotationGizmo;

            gizmoObject = new GameObject();
            gizmoObject.name = "Scale Gizmo";
//            ScaleGizmo scaleGizmo = gizmoObject.AddComponent<ScaleGizmo>();
//            editorGizmoSystem.ScaleGizmo = scaleGizmo;
        }

        /// <summary>
        /// Destroys all existing editor subsystems.
        /// </summary>
        private static void DestroyExistingSubsystems()
        {
            DestroyAllEntities(FindObjectsOfType<RuntimeEditorApplication>());
            DestroyAllEntities(FindObjectsOfType<EditorGizmoSystem>());
            DestroyAllEntities(FindObjectsOfType<EditorMeshDatabase>());
            DestroyAllEntities(FindObjectsOfType<EditorObjectSelection>());
            DestroyAllEntities(FindObjectsOfType<EditorCamera>());
            DestroyAllEntities(FindObjectsOfType<EditorUndoRedoSystem>());
            DestroyAllEntities(FindObjectsOfType<EditorShortuctKeys>());
            //DestroyAllEntities(FindObjectsOfType<TranslationGizmo>());
            DestroyAllEntities(FindObjectsOfType<RotationGizmo>());
            //DestroyAllEntities(FindObjectsOfType<ScaleGizmo>());
        }

        /// <summary>
        /// This function recieves a list of entities whose type must derive from 'MonoBehaviour'
        /// and destorys their associated game objects.
        /// </summary>
        private static void DestroyAllEntities<DataType>(DataType[] entitiesToDestroy) where DataType : MonoBehaviour
        {
            foreach (DataType entity in entitiesToDestroy)
            {
                DestroyImmediate(entity.gameObject);
            }
        }
        #endif
        #endregion

        #region Private Methods
        private void OnRenderObject()
        {
            _xzGrid.Render();
        }
        #endregion
    }
}
