using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public abstract class RayBasedRaycaster : BaseRaycaster {

        /// <summary>
        /// Perform a raycast using the worldSpaceRay in eventData.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="results"></param>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> results) {
            if (eventCamera != null) {
                Raycast(((RayPointerEventData) eventData).Ray, results);
            }
        }

        protected abstract void Raycast(Ray ray, IList<RaycastResult> results);

        /// <summary>
        /// Get screen position of this world position as seen by the event camera of this OVRPhysicsRaycaster
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public Vector2 GetScreenPos(Vector3 worldPosition) {
            // In future versions of Uinty RaycastResult will contain screenPosition so this will not be necessary
            return eventCamera.WorldToScreenPoint(worldPosition);
        }
    }
}
