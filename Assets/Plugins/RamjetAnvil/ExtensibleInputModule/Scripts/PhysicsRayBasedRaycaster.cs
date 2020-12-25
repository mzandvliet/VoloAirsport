using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public class PhysicsRayBasedRaycaster : RayBasedRaycaster {

        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _layerMask;

        public override Camera eventCamera {
            get { return _camera; }
        }

        public void SetCamera(Camera c) {
            _camera = c;
        }
        
        protected override void Raycast(Ray ray, IList<RaycastResult> results) {
            float distance = eventCamera.farClipPlane - eventCamera.nearClipPlane;

//            var hits = Physics.RaycastAll(ray, dist, _layerMask);
//
//            if (hits.Length > 1) {
//                Array.Sort(hits, RaycastHitComparer.Default);
//            }
//
//            if (hits.Length != 0) {
//                for (int b = 0, bmax = hits.Length; b < bmax; ++b) {
//                    results.Add(new RaycastResult {
//                        gameObject = hits[b].collider.gameObject,
//                        module = this,
//                        distance = hits[b].distance,
//                        index = results.Count,
//                        worldPosition = hits[0].point,
//                        worldNormal = hits[0].normal,
//                    });
//                }
//            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance,  _layerMask)) {
                //Debug.Log("hit this: " + hit.collider.name);
                results.Add(new RaycastResult {
                    gameObject = hit.collider.gameObject,
                    module = this,
                    distance = hit.distance,
                    index = results.Count,
                    depth = int.MaxValue,
                    worldPosition = hit.point,
                    worldNormal = hit.normal,
                });
            }
        }

//        /// <summary>
//        ///  Perform a Spherecast using the worldSpaceRay in eventData.
//        /// </summary>
//        /// <param name="eventData"></param>
//        /// <param name="resultAppendList"></param>
//        /// <param name="radius">Radius of the sphere</param>
//        public void Spherecast(PointerEventData eventData, List<RaycastResult> resultAppendList, float radius)
//        {
//            if (eventCamera == null)
//                return;
//
//            OVRRayPointerEventData rayPointerEventData = eventData as OVRRayPointerEventData;
//            if (rayPointerEventData == null)
//                return;
//
//            var ray = rayPointerEventData.worldSpaceRay;
//
//            float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;
//
//            var hits = Physics.SphereCastAll(ray, radius, dist, finalEventMask);
//
//            if (hits.Length > 1)
//                System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));
//
//            if (hits.Length != 0)
//            {
//                for (int b = 0, bmax = hits.Length; b < bmax; ++b)
//                {
//                    var result = new RaycastResult
//                    {
//                        gameObject = hits[b].collider.gameObject,
//                        module = this,
//                        distance = hits[b].distance,
//                        index = resultAppendList.Count,
//                        worldPosition = hits[0].point,
//                        worldNormal = hits[0].normal,
//                    };
//                    resultAppendList.Add(result);
//                }
//            }
//        }
//
//        private class RaycastHitComparer : IComparer<RaycastHit> {
//            public static readonly IComparer<RaycastHit> Default = new RaycastHitComparer();
//
//            private RaycastHitComparer() {}
//
//            public int Compare(RaycastHit r1, RaycastHit r2) {
//                return r1.distance.CompareTo(r2.distance);
//            }
//        }
    }
}
