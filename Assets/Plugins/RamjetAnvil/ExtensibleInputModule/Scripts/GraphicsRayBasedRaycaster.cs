using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.InputModule {

    [RequireComponent(typeof(Canvas))]
    public class GraphicsRayBasedRaycaster : RayBasedRaycaster {

        private Canvas _canvas;

        protected override void Awake() {
            base.Awake();
            _canvas = GetComponent<Canvas>();
        }

        /// <summary>
        ///     Perform a raycast into the screen and collect all graphics underneath it.
        /// </summary>
        [NonSerialized] private readonly List<RaycastHit> _sortedGraphics = new List<RaycastHit>();
        [NonSerialized] private readonly List<RaycastHit> _raycastResults = new List<RaycastHit>();
        [NonSerialized] private readonly Vector3[] _corners = new Vector3[4];

        public override Camera eventCamera {
            get { return _canvas.worldCamera; }
        }

        protected override void Raycast(Ray ray, IList<RaycastResult> results) {
            if (_canvas == null) {
                return;
            }

            _raycastResults.Clear();
            GraphicRaycast(_canvas, ray, _raycastResults);

            for (var index = 0; index < _raycastResults.Count; index++) {
                var go = _raycastResults[index].Graphic.gameObject;

                // If we have a camera compare the direction against the cameras forward.
                var cameraFoward = ray.direction;
                var dir = go.transform.rotation * Vector3.forward;
                var isBehindCamera = eventCamera.transform.InverseTransformPoint(_raycastResults[index].WorldPosition).z <= 0;
                var appendGraphic = Vector3.Dot(cameraFoward, dir) > 0 && !isBehindCamera;

                if (appendGraphic) {
                    results.Add(new RaycastResult {
                        gameObject = go,
                        module = this,
                        distance = Vector3.Distance(ray.origin, _raycastResults[index].WorldPosition),
                        index = results.Count,
                        depth = _raycastResults[index].Graphic.depth,
                        worldPosition = _raycastResults[index].WorldPosition
                    });
                }
            }
        }

        private void GraphicRaycast(Canvas canvas, Ray ray, List<RaycastHit> results) {
            //This function is based closely on :
            // void GraphicRaycaster.Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, List<Graphic> results)
            // But modified to take a Ray instead of a canvas pointer, and also to explicitly ignore
            // the graphic associated with the pointer

            // Necessary for the event system
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            _sortedGraphics.Clear();
            for (var i = 0; i < foundGraphics.Count; ++i) {
                var graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                // TODO Don't raycast against own pointer
                if ((graphic.depth == -1) /*|| (pointer == graphic.gameObject)*/) {
                    continue;
                }

                var intersectionPoint = RayIntersectsRectTransform(graphic.rectTransform, ray);
                if (intersectionPoint.HasValue) {
                    //Work out where this is on the screen for compatibility with existing Unity UI code
                    Vector2 screenPos = canvas.worldCamera.WorldToScreenPoint(intersectionPoint.Value);
                    // mask/image intersection - See Unity docs on eventAlphaThreshold for when this does anything
                    if (graphic.Raycast(screenPos, canvas.worldCamera)) {
                        _sortedGraphics.Add(new RaycastHit(graphic, intersectionPoint.Value));
                    }
                }
            }

            _sortedGraphics.Sort();

            for (var i = 0; i < _sortedGraphics.Count; ++i) {
                results.Add(_sortedGraphics[i]);
            }
        }

        /// <summary>
        ///     Detects whether a ray intersects a RectTransform and if it does also
        ///     returns the world position of the intersection.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        private Vector3? RayIntersectsRectTransform(RectTransform rectTransform, Ray ray) {
            rectTransform.GetWorldCorners(_corners);
            var plane = new Plane(_corners[0], _corners[1], _corners[2]);

            float enter;
            Vector3? intersectionPoint = null;
            if (plane.Raycast(ray, out enter)) {
                var intersection = ray.GetPoint(enter);

                var bottomEdge = _corners[3] - _corners[0];
                var leftEdge = _corners[1] - _corners[0];
                var bottomDot = Vector3.Dot(intersection - _corners[0], bottomEdge);
                var leftDot = Vector3.Dot(intersection - _corners[0], leftEdge);
                if ((bottomDot < bottomEdge.sqrMagnitude) && // Can use sqrMag because BottomEdge is not normalized
                    (leftDot < leftEdge.sqrMagnitude) &&
                    (bottomDot >= 0) &&
                    (leftDot >= 0)) {

                    intersectionPoint = _corners[0] + leftDot * leftEdge / leftEdge.sqrMagnitude +
                                        bottomDot * bottomEdge / bottomEdge.sqrMagnitude;

//                    Debug.Log("rect space: " + rectTransform.ToRectSpace(intersection) + ", world space " + intersectionPoint);
                }
            }
            return intersectionPoint;
        }

        private struct RaycastHit : IComparable<RaycastHit> {
            public readonly Graphic Graphic;
            public readonly Vector3 WorldPosition;

            public RaycastHit(Graphic graphic, Vector3 worldPosition) {
                Graphic = graphic;
                WorldPosition = worldPosition;
            }

            public int CompareTo(RaycastHit other) {
                return other.Graphic.depth.CompareTo(this.Graphic.depth);
            }
        }
    }
}