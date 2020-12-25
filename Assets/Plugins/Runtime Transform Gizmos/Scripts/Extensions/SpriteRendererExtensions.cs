using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public static class SpriteRendererExtensions
    {
        #region Public Static Functions
        public static List<Vector3> GetWorldCenterAndCornerPoints(this SpriteRenderer spriteRenderer)
        {
            List<Vector3> allPoints = new List<Vector3> { spriteRenderer.GetWorldCenterPoint() };
            List<Vector3> cornerPoints = spriteRenderer.GetWorldCornerPoints();
            allPoints.AddRange(cornerPoints);

            return allPoints;
        }

        public static List<Vector3> GetWorldCornerPoints(this SpriteRenderer spriteRenderer)
        {
            Vector3 worldSpaceSize = Vector3.Scale(spriteRenderer.GetModelSpaceSize(), spriteRenderer.transform.lossyScale);
            Vector3 worldSpaceExtents = worldSpaceSize * 0.5f;

            Transform spriteTransform = spriteRenderer.transform;
            Vector3 worldPos = spriteRenderer.GetWorldCenterPoint();
            Vector3 spriteRight = spriteTransform.right;
            Vector3 spriteUp = spriteTransform.up;

            List<Vector3> cornerPoints = new List<Vector3>();
            cornerPoints.Add(worldPos - spriteRight * worldSpaceExtents.x + spriteUp * worldSpaceExtents.y);
            cornerPoints.Add(worldPos + spriteRight * worldSpaceExtents.x + spriteUp * worldSpaceExtents.y);
            cornerPoints.Add(worldPos + spriteRight * worldSpaceExtents.x - spriteUp * worldSpaceExtents.y);
            cornerPoints.Add(worldPos - spriteRight * worldSpaceExtents.x - spriteUp * worldSpaceExtents.y);

            return cornerPoints;
        }

        public static Vector3 GetWorldCenterPoint(this SpriteRenderer spriteRenderer)
        {
            return spriteRenderer.transform.TransformPoint(spriteRenderer.GetModelSpaceBounds().center);
        }

        public static Vector3 GetModelSpaceSize(this SpriteRenderer spriteRenderer)
        {
            return spriteRenderer.GetModelSpaceBounds().size;
        }

        public static Bounds GetModelSpaceBounds(this SpriteRenderer spriteRenderer)
        {
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null) return BoundsExtensions.GetInvalidBoundsInstance();

            #if !UNITY_5
            Vector3 modelSpaceCenter = spriteRenderer.transform.InverseTransformPoint(spriteRenderer.bounds.center);
            modelSpaceCenter.z = 0.0f;
            return new Bounds(modelSpaceCenter, sprite.rect.size / sprite.pixelsPerUnit);
            #else
            List<Vector2> spriteVerts = new List<Vector2>(sprite.vertices);
            return Vector2Extensions.GetPointCloudAABB(spriteVerts);
            #endif
        }

        // Works only when the Read/Write enabled flag is set inside the sprite texture properties. 
        // Otherwise, it will always return false.
        public static bool IsPixelFullyTransparent(this SpriteRenderer spriteRenderer, Vector3 worldPos)
        {
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null) return true;
            Texture2D spriteTexture = sprite.texture;
            if (spriteTexture == null) return true;

            Transform spriteTransform = spriteRenderer.transform;
            Vector3 modelSpacePos = spriteTransform.InverseTransformPoint(worldPos);

            Plane xyPlane = new Plane(Vector3.forward, 0.0f);
            Vector3 projectedPos = xyPlane.ProjectPoint(modelSpacePos);
            Bounds modelSpaceAABB = spriteRenderer.GetModelSpaceBounds();
            modelSpaceAABB.size = new Vector3(modelSpaceAABB.size.x, modelSpaceAABB.size.y, 1.0f);
            if (!modelSpaceAABB.Contains(projectedPos)) return true;

            Vector3 bottomLeft = xyPlane.ProjectPoint(modelSpaceAABB.min);
            Vector3 fromTopLeftToPos = projectedPos - bottomLeft;

            Vector2 pixelCoords = new Vector2(fromTopLeftToPos.x * sprite.pixelsPerUnit, fromTopLeftToPos.y * sprite.pixelsPerUnit);
            pixelCoords += sprite.textureRectOffset;

            try
            {
                float alpha = spriteTexture.GetPixel((int)(pixelCoords.x + 0.5f), (int)(pixelCoords.y + 0.5f)).a;
                return alpha <= 1e-3f;
            }
            catch (UnityException e)
            {
                // Ternary operator needed to avoid 'variable not used' warning
                return e != null ? false : false;
            }
        }
        #endregion
    }
}
