using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public static class CanvasExtensions {

        private static readonly List<GraphicRaycastHit> GraphicElementsCache = new List<GraphicRaycastHit>();

        public static void Raycast(
            this Canvas canvas, 
            Ray ray, 
            List<RaycastResult> resultAppendList, 
            int layerMask,
            float rayLength = float.MaxValue) {

            float hitDistance = float.MaxValue;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength, layerMask)) {
                hitDistance = hit.distance;
            }

            GraphicElementsCache.Clear();
            canvas.RaycastGraphicElements(ray, GraphicElementsCache);

            for (var i = 0; i < GraphicElementsCache.Count; i++) {
                var graphicRaycastHit = GraphicElementsCache[i];
                var graphicElement = graphicRaycastHit.Graphic;

                var shouldAppendGraphic = Vector3.Dot(ray.direction, graphicElement.transform.forward) > 0;
                if (shouldAppendGraphic) {

                    var tranform = graphicElement.gameObject.transform;
                    // http://geomalgorithms.com/a06-_intersect-2.html
                    var distance = (Vector3.Dot(tranform.forward, tranform.position - ray.origin) / Vector3.Dot(tranform.forward, ray.direction));
                    var isInRange = distance >= 0 && distance < hitDistance;
                    if (isInRange) {
                        resultAppendList.Add(new RaycastResult {
                            gameObject = graphicElement.gameObject,
                            //module = this, // TODO Module is required?
                            distance = distance,
                            worldPosition = graphicRaycastHit.WorldPosition,
                            worldNormal = graphicRaycastHit.WorldNormal,
                            index = resultAppendList.Count,
                            depth = graphicElement.depth,
                            sortingLayer =  canvas.sortingLayerID,
                            sortingOrder = canvas.sortingOrder });
                    }
                }
            }
        }

        private static void RaycastGraphicElements(this Canvas canvas, Ray ray, List<GraphicRaycastHit> sortedGraphics) {
            var graphicElements = GraphicRegistry.GetGraphicsForCanvas(canvas);
            for (int i = 0; i < graphicElements.Count; ++i) {
                var graphicElement = graphicElements[i];

                var isEligible = graphicElement.depth > -1 && graphicElement.raycastTarget;
                var raycastHit = graphicElement.rectTransform.IntersectingPoint(ray);
                if (isEligible && raycastHit.HasValue) {
                    // TODO Fill in normal
                    sortedGraphics.Add(new GraphicRaycastHit(
                        graphicElement,
                        worldPosition: raycastHit.Value,
                        worldNormal: Vector3.zero));
                }
            }

            sortedGraphics.Sort((g1, g2) => g2.Graphic.depth.CompareTo(g1.Graphic.depth));
        }

        private struct GraphicRaycastHit {
            public readonly Graphic Graphic;
            public readonly Vector3 WorldPosition;
            public readonly Vector3 WorldNormal;

            public GraphicRaycastHit(Graphic graphic, Vector3 worldPosition, Vector3 worldNormal) {
                Graphic = graphic;
                WorldPosition = worldPosition;
                WorldNormal = worldNormal;
            }
        }
    }
}
