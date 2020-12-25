using UnityEngine;

namespace RTEditor
{
    public class OrientedBoxRayHit
    {
        #region Private Variables
        private Ray _ray;
        private float _hitEnter;
        private OrientedBox _hitBox;
        private Vector3 _hitPoint;
        private Vector3 _hitNormal;
        private BoxFace _hitFace;
        #endregion

        #region Public Properties
        public Ray Ray { get { return _ray; } }
        public float HitEnter { get { return _hitEnter; } }
        public OrientedBox HitBox { get { return new OrientedBox(_hitBox); } }
        public Vector3 HitPoint { get { return _hitPoint; } }
        public Vector3 HitNormal { get { return _hitNormal; } }
        public BoxFace HitFace { get { return _hitFace; } }
        #endregion

        #region Constructors
        public OrientedBoxRayHit(Ray ray, float hitEnter, OrientedBox hitBox)
        {
            _ray = ray;
            _hitEnter = hitEnter;
            _hitBox = new OrientedBox(hitBox);
            _hitPoint = ray.GetPoint(hitEnter);

            _hitFace = hitBox.GetBoxFaceClosestToPoint(_hitPoint);
            _hitNormal = hitBox.GetBoxFacePlane(_hitFace).normal;
        }
        #endregion
    }
}