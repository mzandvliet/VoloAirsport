using UnityEngine;

namespace RTEditor
{
    public class SpriteRayHit
    {
        #region Private Variables
        private Ray _ray;
        private float _hitEnter;
        private SpriteRenderer _hitSpriteRenderer;
        private Vector3 _hitPoint;
        private Vector3 _hitNormal;
        #endregion

        #region Public Properties
        public Ray Ray { get { return _ray; } }
        public float HitEnter { get { return _hitEnter; } }
        public SpriteRenderer HitSpriteRenderer {get{return _hitSpriteRenderer;}}
        public Vector3 HitPoint { get { return _hitPoint; } }
        public Vector3 HitNormal { get { return _hitNormal; } }
        #endregion

        #region Constructors
        public SpriteRayHit(Ray ray, float hitEnter, SpriteRenderer hitSpriteRenderer, Vector3 hitPoint, Vector3 hitNormal)
        {
            _ray = ray;
            _hitEnter = hitEnter;
            _hitSpriteRenderer = hitSpriteRenderer;
            _hitPoint = hitPoint;

            _hitNormal = hitNormal;
            _hitNormal.Normalize();
        }
        #endregion
    }
}