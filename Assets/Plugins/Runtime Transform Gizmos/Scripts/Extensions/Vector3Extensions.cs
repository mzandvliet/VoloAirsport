using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Vector3' extension methods.
    /// </summary>
    public static class Vector3Extensions
    {
        #region Public Static Functions
        /// <summary>
        /// The function can be used to check if the specified point lies inside the
        /// triangle made up of the specified points. 
        /// </summary>
        /// <remarks>
        /// The function does not check if the point lies on the triangle plane. If you imagine
        /// extruding the triangle edges infinitely, the function checks if the point lies within
        /// the resulting volume.
        /// </remarks>
        /// <param name="point">
        /// The point involved in the containment test.
        /// </param>
        /// <param name="trianglePoints">
        /// The triangle points. The function assumes the points are specified in clockwise
        /// winding order.
        /// </param>
        /// <returns>
        /// True if the point lies inside the traingle volume and false otherwise.
        /// </returns>
        public static bool IsInsideTriangle(this Vector3 point, Vector3[] trianglePoints)
        {
            // We will need the triangle plane normal. So we will construct 2 vectors which
            // represent the edges of the triangle and then perform the cross product between
            // them to get the triangle normal. The normal is generated in such a way that if
            // the triangle were to lie inside the XY plane, the normal would be pointing along
            // the negative global Z vector. 
            // Note: You can check this page for more details about the cross product:
            //       http://docs.unity3d.com/ScriptReference/Vector3.Cross.html
            Vector3 toSecondPoint = trianglePoints[1] - trianglePoints[0];
            Vector3 toThirdPoint = trianglePoints[2] - trianglePoints[0];
            Vector3 trianglePlaneNormal = Vector3.Cross(toSecondPoint, toThirdPoint);       // Order is important!

            // The idea of the algorithm is to loop through each triangle edge and construct a normal
            // to the plane which contains the edge and which is perpendicular to the triangle plane.
            // If the query point lies behind all these planes, it means it also lies inside the triangle.
            for (int edgeIndex = 0; edgeIndex < 3; ++edgeIndex)
            {
                // Construct the edge vector. This is a vector which goes from the edge start point to the
                // second point. We will use the '%' operator to be able to wrap around for the last edge
                // which connects the last point to the first point in the triangle points array.
                Vector3 edgeVector = trianglePoints[(edgeIndex + 1) % 3] - trianglePoints[edgeIndex];

                // We will now generate the normal of the edge plane such that it points outside 
                // of the triangle volume/area. 
                Vector3 edgePlaneNormal = Vector3.Cross(edgeVector, trianglePlaneNormal);

                // Construct a vector which goes from the start point of the edge to the query point.
                // If the dot product between this vector and the calculated plane normal is greater than 
                // 0.0f (i.e. the point described by 'fromEdgeStartToPoint' is in front of the plane),
                // it means the query point lies outside the triangle.
                Vector3 fromEdgeStartToPoint = point - trianglePoints[edgeIndex];
                float dotProduct = Vector3.Dot(edgePlaneNormal, fromEdgeStartToPoint);

                // Outside? 
                // Note: If we had generated the plane normal (edgePlaneNormal) using a different order of passing
                //       parameters, this sign would have had to be reversed.
                if (dotProduct > 0.0f) return false;
            }

            // The point lies behind all planes, so it means it lies inside the triangle volume
            return true;
        }

        public static Vector3 GetInverse(this Vector3 vector)
        {
            return new Vector3(1.0f / vector.x, 1.0f / vector.y, 1.0f / vector.z);
        }

        public static Vector3 GetVectorWithAbsComponents(this Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        public static float GetAbsDot(this Vector3 v1, Vector3 v2)
        {
            return Mathf.Abs(Vector3.Dot(v1, v2));
        }
        #endregion

        #region Utilities
        public static void GetMinMaxPoints(List<Vector3> points, out Vector3 min, out Vector3 max)
        {
            min = points[0];
            max = points[0];

            for (int pointIndex = 0; pointIndex < points.Count; ++pointIndex)
            {
                min = Vector3.Min(min, points[pointIndex]);
                max = Vector3.Max(max, points[pointIndex]);
            }
        }

        public static Box GetPointCloudBox(List<Vector3> points)
        {
            Vector3 minPoint, maxPoint;
            GetMinMaxPoints(points, out minPoint, out maxPoint);

            return new Box((minPoint + maxPoint) * 0.5f, (maxPoint - minPoint));
        }

        public static List<Vector3> GetTransformedPoints(List<Vector3> pointsToTransform, Matrix4x4 transformMatrix)
        {
            if (pointsToTransform.Count == 0) return new List<Vector3>();
            var transformedPoints = new List<Vector3>(pointsToTransform.Count);

            foreach (Vector3 point in pointsToTransform)
            {
                transformedPoints.Add(transformMatrix.MultiplyPoint(point));
            }

            return transformedPoints;
        }

        public static List<Vector3> ApplyOffsetToPoints(List<Vector3> pointsToOffset, Vector3 offsetVector)
        {
            if (pointsToOffset.Count == 0) return new List<Vector3>();
            var newPoints = new List<Vector3>(pointsToOffset.Count);

            foreach (Vector3 point in pointsToOffset)
            {
                newPoints.Add(point + offsetVector);
            }

            return newPoints;
        }

        public static Vector3 GetAveragePoint(List<Vector3> pointsToAverage)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 point in pointsToAverage)
            {
                sum += point;
            }

            if (pointsToAverage.Count != 0) return sum * (1.0f / pointsToAverage.Count);
            return Vector3.zero;
        }

        public static Vector3 GetPointClosestToPoint(List<Vector3> points, Vector3 point)
        {
            float minDistance = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;

            foreach (var pt in points)
            {
                float distance = (pt - point).magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = pt;
                }
            }

            return closestPoint;
        }
        #endregion
    }
}
