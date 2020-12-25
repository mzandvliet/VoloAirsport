using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public static class RectTransformExtensions {

        public static Vector3? IntersectingPoint(this RectTransform rect, Ray ray) {
            float distance;
            Vector3? intersectingPoint;
            if (new Plane(rect.forward, rect.position).Raycast(ray, out distance)) {
                Vector3 point = ray.GetPoint(distance);
                point = rect.InverseTransformPoint(point);
                if (point.x > rect.rect.xMin &&
                    point.x < rect.rect.xMax &&
                    point.y > rect.rect.yMin &&
                    point.y < rect.rect.yMax) {
                    intersectingPoint = rect.TransformPoint(point);
                } else {
                    intersectingPoint = null;
                }
            } else {
                intersectingPoint = null;
            }
            return intersectingPoint;
        }

        public static Vector3? IntersectPlane(this RectTransform rect, Ray ray) {
            float hitDistance;
            if (new Plane(rect.rotation * Vector3.back, rect.position).Raycast(ray, out hitDistance)) {
                return ray.GetPoint(hitDistance);
            }
            return null;
        }
    }
}
