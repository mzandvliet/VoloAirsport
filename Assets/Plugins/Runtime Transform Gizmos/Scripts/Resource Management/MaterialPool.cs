using UnityEngine;

namespace RTEditor
{
    public class MaterialPool : SingletonBase<MaterialPool>
    {
        #region Private Variables
        private Material _geometry2D;
        private Material _gizmoSolidComponent;
        private Material _gizmoLine;
        private Material _GLLine;
        private Material _gradientCameraBk;
        private Material _xzGrid;
        #endregion

        #region Public Properties
        public Material Geometry2D
        {
            get
            {
                if (_geometry2D == null) _geometry2D = new Material(ShaderPool.Instance.Geometry2D);
                return _geometry2D;
            }
        }

        public Material GizmoSolidComponent
        {
            get
            {
                if (_gizmoSolidComponent == null) _gizmoSolidComponent = new Material(ShaderPool.Instance.GizmoSolidComponent);
                return _gizmoSolidComponent;
            }
        }

        public Material GizmoLine
        {
            get
            {
                if (_gizmoLine == null) _gizmoLine = new Material(ShaderPool.Instance.GizmoLine);
                return _gizmoLine;
            }
        }

        public Material GLLine
        {
            get
            {
                if (_GLLine == null) _GLLine = new Material(ShaderPool.Instance.GLLine);
                return _GLLine;
            }
        }

        public Material GradientCameraBk
        {
            get
            {
                if (_gradientCameraBk == null) _gradientCameraBk = new Material(ShaderPool.Instance.GradientCameraBk);
                return _gradientCameraBk;
            }
        }

        public Material XZGrid
        {
            get
            {
                if (_xzGrid == null) _xzGrid = new Material(ShaderPool.Instance.XZGrid);
                return _xzGrid;
            }
        }
        #endregion
    }
}
