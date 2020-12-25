using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    [Serializable]
    public class XZGrid
    {
        #region Private Variables
        [SerializeField]
        private bool _isVisible = true;
        [SerializeField]
        private float _cellSizeX = 1.0f;
        [SerializeField]
        private float _cellSizeZ = 1.0f;
        [SerializeField]
        private Color _lineColor = new Color(0.5f, 0.5f, 0.5f, 102.0f / 255.0f);
        #endregion

        #region Public Static Properties
        public static float MinLineThickness { get { return 0.01f; } }
        public static float MinCellSize { get { return 0.1f; } }
        public static float MinLineFadeZoomFactor { get { return 1e-4f; } }
        public static int MinColorFadeCellCount { get { return 2; } }
        #endregion

        #region Public Properties
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }
        public float CellSizeX { get { return _cellSizeX; } set { _cellSizeX = Mathf.Max(value, MinCellSize); } }
        public float CellSizeZ { get { return _cellSizeZ; } set { _cellSizeZ = Mathf.Max(value, MinCellSize); } }
        public Color LineColor { get { return _lineColor; } set { _lineColor = value; } }
        public Plane Plane { get { return new Plane(Vector3.up, Vector3.zero); } }
        #endregion

        #region Public Methods
        public void Render()
        {
            if (!_isVisible) return;

            Camera editorCamera = EditorCamera.Instance.Camera;
            Transform cameraTransform = editorCamera.transform;

            float zoom = Mathf.Abs(cameraTransform.position.y);
            int p0 = MathHelper.GetNumberOfDigits((int)zoom) - 1;
            int p1 = p0 + 1;

            float s0 = Mathf.Pow(10.0f, p0);
            float s1 = Mathf.Pow(10.0f, p1);

            Material material = MaterialPool.Instance.XZGrid;
            material.SetFloat("_CamFarPlane", editorCamera.farClipPlane);
            material.SetVector("_CamLook", editorCamera.transform.forward);
            material.SetFloat("_FadeScale", zoom / 10.0f);

            float alphaScale = Mathf.Clamp(1.0f - ((zoom - s0) / (s1 - s0)), 0.0f, 1.0f);
            GLPrimitives.DrawGridLines(_cellSizeX * s0, _cellSizeZ * s0, editorCamera, material, new Color(_lineColor.r, _lineColor.g, _lineColor.b, _lineColor.a * alphaScale));
            GLPrimitives.DrawGridLines(_cellSizeX * s1, _cellSizeZ * s1, editorCamera, material, new Color(_lineColor.r, _lineColor.g, _lineColor.b, _lineColor.a - _lineColor.a * alphaScale)); 
        }

        public XZGridCell GetCellFromWorldPoint(Vector3 worldPoint)
        {
            Vector3 projectedPoint = Plane.ProjectPoint(worldPoint);
            return GetCellFromWorldXZ(projectedPoint.x, projectedPoint.z);
        }

        public XZGridCell GetCellFromWorldXZ(float worldX, float worldZ)
        {
            int cellIndexX = Mathf.FloorToInt((worldX + 0.5f * _cellSizeX) / _cellSizeX);
            int cellIndexZ = Mathf.FloorToInt((worldZ + 0.5f * _cellSizeZ) / _cellSizeZ);

            return new XZGridCell(cellIndexX, cellIndexZ, this);
        }

        public List<Vector3> GetCellCornerPoints(XZGridCell gridCell)
        {
            float startX = gridCell.CellIndexX * _cellSizeX;
            float startZ = gridCell.CellIndexZ * _cellSizeX;

            var cellCornerPoints = new List<Vector3>();
            cellCornerPoints.Add(new Vector3(startX, 0.0f, startZ));
            cellCornerPoints.Add(new Vector3(startX + _cellSizeX, 0.0f, startZ));
            cellCornerPoints.Add(new Vector3(startX + _cellSizeX, 0.0f, startZ + _cellSizeZ));
            cellCornerPoints.Add(new Vector3(startX, 0.0f, startZ + _cellSizeZ));

            return cellCornerPoints;
        }

        public Vector3 GetCellCornerPointClosestToMouseCursor()
        {
            Ray ray = EditorCamera.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            float t;
            if(Plane.Raycast(ray, out t))
            {
                Vector3 pickPoint = ray.GetPoint(t);
                List<Vector3> cellCornerPoints = GetCellCornerPoints(GetCellFromWorldXZ(pickPoint.x, pickPoint.z));
                return Vector3Extensions.GetPointClosestToPoint(cellCornerPoints, pickPoint);
            }

            return Vector3.zero;
        }
        #if UNITY_EDITOR
        public void RenderView(MonoBehaviour parentMono)
        {
            bool newBool = EditorGUILayout.ToggleLeft("Is Visible", _isVisible);
            if(newBool != _isVisible)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                _isVisible = newBool;
            }

            Color newColor = EditorGUILayout.ColorField("Line Color", _lineColor);
            if(newColor != _lineColor)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                _lineColor = newColor;
            }

            float newFloat = EditorGUILayout.FloatField("Cell Size X", CellSizeX);
            if (newFloat != CellSizeX)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                CellSizeX = newFloat;
            }

            newFloat = EditorGUILayout.FloatField("Cell Size Z", CellSizeZ);
            if (newFloat != CellSizeZ)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                CellSizeZ = newFloat;
            }
        }
        #endif
        #endregion
    }
}
