using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class Triangle3D
    {
        #region Private Variables
        private Vector3[] _points = new Vector3[3];
        private Plane _plane;
        private float _area;
        #endregion

        #region Public Properties
        public Vector3 Point0 { get { return _points[0]; } }
        public Vector3 Point1 { get { return _points[1]; } }
        public Vector3 Point2 { get { return _points[2]; } }
        public Vector3 Normal { get { return _plane.normal; } }
        public Plane Plane { get { return _plane; } }
        public float Area { get { return _area; } }
        public bool IsDegenerate { get { return _area == 0.0f || float.IsNaN(_area); } }
        #endregion

        #region Constructors
        public Triangle3D(Triangle3D source)
        {
            _points = new Vector3[3];
            _points[0] = source.Point0;
            _points[1] = source.Point1;
            _points[2] = source.Point2;
            _plane = source._plane;
            _area = source._area;
        }

        public Triangle3D(Vector3 point0, Vector3 point1, Vector3 point2)
        {
            _points = new Vector3[3];
            _points[0] = point0;
            _points[1] = point1;
            _points[2] = point2;
            CalculateAreaAndPlane();
        }
        #endregion

        #region Public Methods
        public void TransformPoints(Matrix4x4 transformMatrix)
        {
            _points[0] = transformMatrix.MultiplyPoint(_points[0]);
            _points[1] = transformMatrix.MultiplyPoint(_points[1]);
            _points[2] = transformMatrix.MultiplyPoint(_points[2]);
            CalculateAreaAndPlane();
        }

        public Box GetEncapsulatingBox()
        {
            List<Vector3> points = GetPoints();

            Vector3 minPoint, maxPoint;
            Vector3Extensions.GetMinMaxPoints(points, out minPoint, out maxPoint);

            return new Box((minPoint + maxPoint) * 0.5f, maxPoint - minPoint);
        }

        public List<Segment3D> GetSegments()
        {
            var segments = new List<Segment3D>();
            segments.Add(new Segment3D(Point0, Point1));
            segments.Add(new Segment3D(Point1, Point2));
            segments.Add(new Segment3D(Point2, Point0));

            return segments;
        }

        public Plane GetSegmentPlane(int segmentIndex)
        {
            Segment3D segment = GetSegment(segmentIndex);

            Vector3 segmentPlaneNormal = Vector3.Cross(segment.Direction, _plane.normal);
            segmentPlaneNormal.Normalize();
            return new Plane(segmentPlaneNormal, segment.StartPoint);
        }

        public Segment3D GetSegment(int segmentIndex)
        {
            return new Segment3D(_points[segmentIndex], _points[(segmentIndex + 1) % 3]);
        }

        public bool Raycast(Ray ray, out float t)
        {
            if (_plane.Raycast(ray, out t))
            {
                Vector3 intersectionPoint = ray.GetPoint(t);
                return ContainsPoint(intersectionPoint);
            }
            else return false;
        }

        public bool Raycast(Ray3D ray, out float t)
        {
            if(ray.IntersectsPlane(_plane, out t))
            {
                Vector3 intersectionPoint = ray.GetPoint(t);
                return ContainsPoint(intersectionPoint);
            }

            return false;
        }

        public bool ContainsPoint(Vector3 point)
        {
            for(int segmentIndex = 0; segmentIndex < 3; ++segmentIndex)
            {
                Plane segmentPlane = GetSegmentPlane(segmentIndex);
                if (segmentPlane.IsPointInFront(point)) return false;
            }

            return true;
        }

        public Sphere3D GetEncapsulatingSphere()
        {
            return GetEncapsulatingBox().GetEncapsulatingSphere();
        }

        public Vector3 GetCenter()
        {
            Vector3 pointSum = Point0 + Point1 + Point2;
            return pointSum / 3.0f;
        }

        public List<Vector3> GetPoints()
        {
            return new List<Vector3> { Point0, Point1, Point2 };
        }

        public Vector3 GetPointClosestToPoint(Vector3 point)
        {
            return Vector3Extensions.GetPointClosestToPoint(GetPoints(), point);
        }
        #endregion

        #region Private Methods
        private void CalculateAreaAndPlane()
        {
            Vector3 edge0 = Point1 - Point0;
            Vector3 edge1 = Point2 - Point0;
            Vector3 normal = Vector3.Cross(edge0, edge1);

            if(normal.magnitude < 1e-5f)
            {
                _area = 0.0f;
                _plane = new Plane(Vector3.zero, Vector3.zero);
            }
            else
            {
                _area = normal.magnitude * 0.5f;

                normal.Normalize();
                _plane = new Plane(normal, Point0);
            }
        }
        #endregion
    }
}