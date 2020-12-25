using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    [Serializable]
    public class EditorCameraBk
    {
        #region Private Variables
        [SerializeField]
        private bool _isVisible = false;
        [SerializeField]
        private Color _topColor = new Color(71.0f / 255.0f, 71.0f / 255.0f, 71.0f / 255.0f, 1.0f);
        [SerializeField]
        private Color _bottomColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        [SerializeField]
        private float _gradientOffset = 0.0f;
        private GameObject _bkObject;
        #endregion

        #region Public Static Properties
        public static float MinGradientOffset { get { return -1.0f; } }
        public static float MaxGradientOffset { get { return 1.0f; } }
        #endregion

        #region Public Properties
        public Color TopColor { get { return _topColor; } set { _topColor = value; } }
        public Color BottomColor { get { return _bottomColor; } set { _bottomColor = value; } }
        public float GradientOffset { get { return _gradientOffset; } set { _gradientOffset = Mathf.Clamp(value, MinGradientOffset, MaxGradientOffset); } }
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                if (Application.isPlaying) GetBkGameObject().SetActive(_isVisible);
            }
        }
        #endregion

        #region Public Methods
        public bool IsSameAs(GameObject gameObject)
        {
            return gameObject == GetBkGameObject();
        }

        public void OnCameraUpdate(Camera camera)
        {
            Transform cameraTransform = camera.transform;
            Transform bkTransform = GetBkGameObject().transform;

            bkTransform.position = cameraTransform.position + cameraTransform.forward * camera.farClipPlane * 0.998f;
            bkTransform.rotation = cameraTransform.rotation;

            bkTransform.parent = null;
            CameraViewVolume viewVolume = camera.GetViewVolume();
            bkTransform.localScale = new Vector3(viewVolume.FarPlaneSize.x, viewVolume.FarPlaneSize.y, 1.0f);
            bkTransform.parent = cameraTransform;

            Material material = MaterialPool.Instance.GradientCameraBk;
            material.SetVector("_TopColor", _topColor);
            material.SetVector("_BottomColor", _bottomColor);
            material.SetFloat("_GradientOffset", _gradientOffset);
            material.SetFloat("_Height", bkTransform.localScale.y);
        }

        #if UNITY_EDITOR
        public void RenderView(MonoBehaviour parentMono)
        {
            bool newBool = EditorGUILayout.ToggleLeft("Is Visible", IsVisible);
            if(newBool != _isVisible)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                IsVisible = newBool;
            }

            Color newColor = EditorGUILayout.ColorField("Top Color", _topColor);
            if(newColor != _topColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                _topColor = newColor;
            }

            newColor = EditorGUILayout.ColorField("Bottom Color", _bottomColor);
            if (newColor != _bottomColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                _bottomColor = newColor;
            }

            float newFloat = EditorGUILayout.FloatField("Gradient Offset", GradientOffset);
            if (newFloat != GradientOffset)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                GradientOffset = newFloat;
            }
        }
        #endif
        #endregion

        #region Private Methods
        private GameObject GetBkGameObject()
        {
            if(_bkObject == null)
            {
                _bkObject = new GameObject("Editor Camera Bk Object");

                MeshFilter meshFilter = _bkObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = MeshPool.Instance.XYSquareMesh;

                MeshRenderer meshRenderer = _bkObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = MaterialPool.Instance.GradientCameraBk;

                _bkObject.SetActive(IsVisible);
            }

            return _bkObject;
        }
        #endregion
    }
}
