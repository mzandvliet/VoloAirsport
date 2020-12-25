using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public static class Vector2Extensions
    {
        #region Public Static Functions
        public static Bounds GetPointCloudAABB(List<Vector2> pointCloud)
        {
            if (pointCloud.Count == 0) return BoundsExtensions.GetInvalidBoundsInstance();
            Vector2 min = pointCloud[0];
            Vector2 max = pointCloud[0];

            for(int ptIndex = 1; ptIndex < pointCloud.Count; ++ptIndex)
            {
                Vector2 pt = pointCloud[ptIndex];
                min = Vector2.Min(pt, min);
                max = Vector2.Max(pt, max);
            }

            return new Bounds((min + max) * 0.5f, max - min);
        }
        #endregion
    }
}

