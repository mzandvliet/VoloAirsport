using UnityEngine;
using System.Collections;

namespace RTEditor
{
    /// <summary>
    /// This is a static class that implements some useful 'Rect' extension methods.
    /// </summary>
    public static class RectExtensions
    {
        #region Public Static Functions
        /// <summary>
        /// Given a rectangle, the function returns the rectangle point (one of the
        /// corner points or the rectangle center) that is closest to 'point'.
        /// </summary>
        public static Vector2 GetClosestPointToPoint(this Rect rectangle, Vector2 point)
        {
            // Store the rectangle points
            Vector2[] rectanglePoints = rectangle.GetCornerAndCenterPoints();

            // Find the rectangle point which is closest to 'point'
            int indexOfClosestPoint = 0;
            float minDistance = float.MaxValue;
            for (int pointIndex = 0; pointIndex < rectanglePoints.Length; ++pointIndex)
            {
                // Calculate the distance between 'point' and the current rectangle point
                float distanceToPoint = (rectanglePoints[pointIndex] - point).magnitude;
                if (distanceToPoint < minDistance)
                {
                    // The distance is smaller than what we found so far. Store the new
                    // minimum index and the index of the closest point found so far.
                    minDistance = distanceToPoint;
                    indexOfClosestPoint = pointIndex;
                }
            }

            // Return the closest point
            return rectanglePoints[indexOfClosestPoint];
        }

        /// <summary>
        /// Returns an array which holds the specified rectangle's corner points and center. The points
        /// are stored in the array in the following order: top left, top right, bottom right, bottom left,
        /// center.
        /// </summary>
        public static Vector2[] GetCornerAndCenterPoints(this Rect rectangle)
        {
            return new Vector2[]
        {
            new Vector2(rectangle.xMin, rectangle.yMin),
            new Vector2(rectangle.xMax, rectangle.yMin),
            new Vector2(rectangle.xMax, rectangle.yMax),
            new Vector2(rectangle.xMin, rectangle.yMax),
            rectangle.center
        };
        }
        #endregion
    }
}
