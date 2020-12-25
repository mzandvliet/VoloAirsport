using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public static class GraphicExtensions {

        public static readonly List<Component> ComponentCache = new List<Component>(); 

        public static bool Raycast(this Graphic graphic, Ray ray) {
            bool isRaycastValid;
            if (graphic.isActiveAndEnabled) {
                var transform = graphic.transform;
                isRaycastValid = true;
                var ignoreParentGroups = false;

                // Bubble up from here to parent
                while (transform != null && isRaycastValid) {
                    ComponentCache.Clear();
                    transform.GetComponents(ComponentCache);

                    for (var i = 0; i < ComponentCache.Count; i++) {
                        var component = ComponentCache[i];
                        var filter = ComponentCache[i] as ICanvasRaycastFilter;

                        if (filter != null) {
                            var group = component as CanvasGroup;
                            if (group != null) {
                                if (!ignoreParentGroups && group.ignoreParentGroups) {
                                    ignoreParentGroups = true;
                                    isRaycastValid = IsRaycastLocationValid(group, ray);
                                } else if (!ignoreParentGroups) {
                                    isRaycastValid = IsRaycastLocationValid(group, ray);
                                } else {
                                    isRaycastValid = true;
                                }
                            } else {
                                isRaycastValid = IsRaycastLocationValid(filter, ray);
                            }
                        }
                    }

                    transform = transform.parent;
                }
            } else {
                isRaycastValid = false;
            }
            return isRaycastValid;
        }

        // TODO Handle types separately to improve readability
        public static bool IsRaycastLocationValid<T>(T component, Ray ray) where T : ICanvasRaycastFilter {
            bool isRaycastValid;
            if (component is Mask || component is RectMask2D) {
                RectTransform rectTransform;
                if (component is Mask) {
                    rectTransform = (component as Mask).rectTransform;
                } else {
                    rectTransform = (component as RectMask2D).rectTransform;
                }

                var mask = component as UIBehaviour;
                isRaycastValid = !mask.isActiveAndEnabled || rectTransform.IntersectPlane(ray).HasValue;
            } else if (component is CanvasGroup) {
                var canvasGroup = component as CanvasGroup;
                isRaycastValid = canvasGroup.blocksRaycasts;
            } else if (component is Image) {
                var image = component as Image;
                if (image.alphaHitTestMinimumThreshold < 1.0f && image.overrideSprite != null) {
                    var hitPoint = image.rectTransform.IntersectPlane(ray);
                    if (hitPoint.HasValue) {
                        var localHitPoint = image.rectTransform.InverseTransformDirection(hitPoint.Value);

                        Rect pixelAdjustedRect = image.GetPixelAdjustedRect();
                        localHitPoint.x += image.rectTransform.pivot.x * pixelAdjustedRect.width;
                        localHitPoint.y += image.rectTransform.pivot.y * pixelAdjustedRect.height;
                        localHitPoint = image.MapCoordinate(localHitPoint, pixelAdjustedRect);
                        Rect textureRect = image.overrideSprite.textureRect;
                        Vector2 vector2 = new Vector2(localHitPoint.x / textureRect.width,
                            localHitPoint.y / textureRect.height);
                        float u = Mathf.Lerp(textureRect.x, textureRect.xMax, vector2.x) /
                                  image.overrideSprite.texture.width;
                        float v = Mathf.Lerp(textureRect.y, textureRect.yMax, vector2.y) /
                                  image.overrideSprite.texture.height;

                        try {
                            isRaycastValid = image.overrideSprite.texture.GetPixelBilinear(u, v).a >=
                                             image.alphaHitTestMinimumThreshold;
                        } catch (UnityException e) {
                            Debug.LogError(
                                "Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " +
                                e.Message + " Also make sure to disable sprite packing for this sprite.", image);
                            isRaycastValid = true;
                        }
                    } else {
                        // TODO Is this ok??
                        isRaycastValid = true;
                    }
                } else {
                    isRaycastValid = true;
                }
            } else {
                throw new ArgumentException("Cannot handle type: " + component.GetType());
            }
            return isRaycastValid;
        }
    }
}
