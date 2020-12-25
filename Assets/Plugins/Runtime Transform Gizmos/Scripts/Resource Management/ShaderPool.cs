using UnityEngine;

namespace RTEditor
{
    public class ShaderPool : SingletonBase<ShaderPool>
    {
        #region Private Variables
        private Shader _geometry2D;
        private Shader _gizmoSolidComponent;
        private Shader _gizmoLine;
        private Shader _GLLine;
        private Shader _gradientCameraBk;
        private Shader _xzGrid;
        #endregion

        #region Public Properties
        public Shader Geometry2D
        {
            get
            {
                if (_geometry2D == null) _geometry2D = Shader.Find("Geometry2D");
                return _geometry2D;
            }
        }

        public Shader GizmoSolidComponent
        {
            get
            {
                if (_gizmoSolidComponent == null) _gizmoSolidComponent = Shader.Find("Gizmo Solid Component");
                return _gizmoSolidComponent;
            }
        }

        public Shader GizmoLine
        {
            get
            {
                if (_gizmoLine == null) _gizmoLine = Shader.Find("Gizmo Line");
                return _gizmoLine;
            }
        }

        public Shader GLLine
        {
            get
            {
                if (_GLLine == null) _GLLine = Shader.Find("GLLine");
                return _GLLine;
            }
        }

        public Shader GradientCameraBk
        {
            get
            {
                if (_gradientCameraBk == null) _gradientCameraBk = Shader.Find("Gradient Camera Bk");
                return _gradientCameraBk;
            }
        }

        public Shader XZGrid
        {
            get
            {
                if (_xzGrid == null) _xzGrid = Shader.Find("XZGrid");
                return _xzGrid;
            }
        }
        #endregion
    }
}
