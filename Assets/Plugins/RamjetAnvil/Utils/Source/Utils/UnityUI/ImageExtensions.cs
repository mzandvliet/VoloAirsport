using System;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public static class ImageExtensions {

        public static Vector2 MapCoordinate(this Image image, Vector2 local, Rect rect) {
            Rect spriteRect = image.sprite.rect;
            if (image.type == Image.Type.Simple || image.type == Image.Type.Filled)
                return new Vector2(local.x * spriteRect.width / rect.width, local.y * spriteRect.height / rect.height);

            Vector4 border = image.sprite.border;
            Vector4 adjustedBorder = GetAdjustedBorders(border / image.pixelsPerUnit, rect);

            for (int i = 0; i < 2; i++) {
                if (local[i] <= adjustedBorder[i])
                    continue;

                if (rect.size[i] - local[i] <= adjustedBorder[i + 2]) {
                    local[i] -= (rect.size[i] - spriteRect.size[i]);
                    continue;
                }

                if (image.type == Image.Type.Sliced) {
                    float lerp = Mathf.InverseLerp(adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i]);
                    local[i] = Mathf.Lerp(border[i], spriteRect.size[i] - border[i + 2], lerp);
                }
                local[i] -= adjustedBorder[i];
                local[i] = Mathf.Repeat(local[i], spriteRect.size[i] - border[i] - border[i + 2]);
                local[i] += border[i];
            }

            return local;
        }

        public static Vector4 GetAdjustedBorders(Vector4 border, Rect rect) {
            for (int axis = 0; axis <= 1; axis++) {
                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (rect.size[axis] < combinedBorders && combinedBorders != 0) {
                    float borderScaleRatio = rect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }
    }
}