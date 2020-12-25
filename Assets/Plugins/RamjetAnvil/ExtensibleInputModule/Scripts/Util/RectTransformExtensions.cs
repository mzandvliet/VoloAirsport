using UnityEngine;

namespace RamjetAnvil.InputModule {

    public static class RectTransformExtensions {

        private static readonly Vector3[] Corners = new Vector3[4];
        public static Vector2? ToRectSpace(this RectTransform rect, Vector3 worldPosition) {
            rect.GetWorldCorners(Corners);
            var bottomEdge = Corners[3] - Corners[0];
                var leftEdge = Corners[1] - Corners[0];
                var bottomDot = Vector3.Dot(worldPosition - Corners[0], bottomEdge);
                var leftDot = Vector3.Dot(worldPosition - Corners[0], leftEdge);
            if ((bottomDot < bottomEdge.sqrMagnitude) && // Can use sqrMag because BottomEdge is not normalized
                (leftDot < leftEdge.sqrMagnitude) &&
                (bottomDot >= 0) &&
                (leftDot >= 0)) {

                return rect.transform.InverseTransformPoint(worldPosition);
            }
            return null;
        }
    }
}
