using UnityEngine;

namespace RTEditor
{
    public struct Segment3D
    {
        #region Private Variables
        private Vector3 _startPoint;
        private Vector3 _normalizedDirection;
        private Vector3 _direction;
        private float _length;
        private float _sqrLength;
        #endregion

        #region Public Properties
        public Vector3 StartPoint { get { return _startPoint; }  }
        public Vector3 EndPoint { get { return _startPoint + _normalizedDirection * _length; } }
        public Vector3 NormalizedDirection { get { return _normalizedDirection; } }
        public Vector3 Direction { get { return _direction; } }
        public float Length { get { return _length; } }
        public float SqrLength { get { return _sqrLength; } }
        public float HalfLength { get { return _length * 0.5f; } }
        #endregion

        #region Constructors
        public Segment3D(Vector3 startPoint, Vector3 endPoint)
        {
            _startPoint = startPoint;
            _direction = endPoint - _startPoint;
            _length = _direction.magnitude;
            _sqrLength = _direction.sqrMagnitude;
            _normalizedDirection = _direction;
            _normalizedDirection.Normalize();
        }
        #endregion

        #region Public Methods
        public Vector3 GetPoint(float t)
        {
            return _startPoint + t * _direction;
        }
        #endregion
    }
}