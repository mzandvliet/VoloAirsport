using UnityEngine;

namespace RTEditor
{
    public class MeshRayHit
    {
        #region Private Variables
        private Ray _ray;
        private float _hitEnter;
        private int _hitTraingleIndex;
        private Vector3 _hitPoint;
        private Vector3 _hitNormal;
        #endregion

        #region Public Properties
        public Ray Ray { get { return _ray; } }
        public float HitEnter { get { return _hitEnter; } }
        public int HitTriangleIndex { get { return _hitTraingleIndex; } }
        public Vector3 HitPoint { get { return _hitPoint; } }
        public Vector3 HitNormal { get { return _hitNormal; } }
        #endregion

        #region Constructors
        public MeshRayHit(Ray ray, float hitEnter, int hitTriangleIndex, Vector3 hitPoint, Vector3 hitNormal)
        {
            _ray = ray;
            _hitEnter = hitEnter;
            _hitTraingleIndex = hitTriangleIndex;
            _hitPoint = hitPoint;

            _hitNormal = hitNormal;
            _hitNormal.Normalize();
        }
        #endregion
    }
}