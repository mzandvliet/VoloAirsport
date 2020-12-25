using UnityEngine;

namespace RTEditor
{
    public static class ColliderExtensions
    {
        #region Public Static Functions
        public static bool RaycastReverseIfFail(this Collider collider, Ray ray, out RaycastHit rayHit)
        {
            Vector3 initialOrigin = ray.origin;

            ray.origin -= ray.direction * 0.1f;
            if (collider.Raycast(ray, out rayHit, float.MaxValue)) return true;
            else
            {
                ray.direction *= -1.0f;
                ray.origin = initialOrigin - ray.direction * 0.1f;
                return collider.Raycast(ray, out rayHit, float.MaxValue);
            }
        }
        #endregion
    }
}
