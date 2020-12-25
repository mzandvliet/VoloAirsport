using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Gui {
    public struct CameraProperties {
        public float FieldOfView;
        public float Aspect;
        public ImmutableTransform Transform;

        public CameraProperties(float fieldOfView, float aspect, ImmutableTransform transform) {
            FieldOfView = fieldOfView;
            Aspect = aspect;
            Transform = transform;
        }
    }

    public static class GuiPlacement {
        /// <summary>
        /// Creates a 2D rectangle in 3D space in front of a camera.
        /// </summary>
        /// <param name="cameraProperties"></param>
        /// <param name="distance">the distance of the rectangle from the camera</param>
        /// <returns>A point in world space with a size that's just big enough to cover the entire screen</returns>
        public static ImmutableTransform CameraViewportRectangle(CameraProperties cameraProperties, float distance = 1f) {
            var position = cameraProperties.Transform.Position + cameraProperties.Transform.Forward * distance;
            var rotation = cameraProperties.Transform.Rotation;
            var height = Mathf.Tan(cameraProperties.FieldOfView * Mathf.Deg2Rad * 0.5f) * distance * 2f;
            var scale = new Vector3(height * cameraProperties.Aspect, height, 0f);
            return new ImmutableTransform(position, rotation, scale);
        }

        /// <summary>
        /// Projects a 2D coordinate on a 2D rectangle in 3D (world) space.
        /// Useful for positioning things in front of the camera.
        /// </summary>
        /// <param name="plane">The rectangle to project the coordinate on</param>
        /// <param name="planeCoordinates">Vector2(0,0) is bottom-left and Vector2(1,1) is top-right</param>
        /// <returns>A point on the rectangle in 3D world space</returns>
        public static ImmutableTransform ProjectPointOnPlane(ImmutableTransform plane, Vector2 planeCoordinates) {
            var relativePlaneCoordinates = planeCoordinates - new Vector2(0.5f, 0.5f);
            var planePosition = new Vector3(relativePlaneCoordinates.x * plane.Scale.x, relativePlaneCoordinates.y * plane.Scale.y, 0f);
            return plane.TranslateLocally(planePosition);
        }

        public static float FocalLength(float fieldOfView) {
            return 2 * Mathf.Tan(fieldOfView / 2 * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Determines the relative scale based on the distance between the origin and the subject's position.
        /// </summary>
        public static float RelativeScale(Transform origin, Vector3 subjectPosition) {
            var plane = new Plane(origin.forward, origin.position);
            return plane.GetDistanceToPoint(subjectPosition);
        }

        /// <summary>
        /// Faces a transform towards another.
        /// </summary>
        public static void FaceTransform(Transform subjectTransform, Transform lookAtTransform) {
            subjectTransform.LookAt(lookAtTransform.transform.position, lookAtTransform.up);
        }
    }
}
