using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    public static class PhysicsHelper
    {
        #region Public Static Functions
        public static Collider RaycastAllClosest(Ray ray, out RaycastHit rayHit, List<Type> allowedColliderTypes, HashSet<GameObject> ignoreObjects)
        {
            rayHit = new RaycastHit();

            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue);
            if (hits.Length != 0)
            {
                float minDistance = float.MaxValue;
                Collider closestCollider = null;

                foreach(var hit in hits)
                {
                    Collider hitCollider = hit.collider;
                    if(allowedColliderTypes.Contains(hitCollider.GetType()))
                    {
                        GameObject colliderObject = hitCollider.gameObject;
                        if(!ignoreObjects.Contains(colliderObject))
                        {
                            if(hit.distance < minDistance)
                            {
                                rayHit = hit;
                                minDistance = hit.distance;
                                closestCollider = hitCollider;
                            }
                        }
                    }
                }

                return closestCollider;
            }

            return null;
        }

        public static Collider RaycastAllClosest(Ray ray, out RaycastHit rayHit)
        {
            rayHit = new RaycastHit();

            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue);
            if (hits.Length != 0)
            {
                float minDistance = float.MaxValue;
                Collider closestCollider = null;

                foreach (var hit in hits)
                {
                    Collider hitCollider = hit.collider;
                    if (hit.distance < minDistance)
                    {
                        rayHit = hit;
                        minDistance = hit.distance;
                        closestCollider = hitCollider;
                    }
                }

                return closestCollider;
            }

            return null;
        }

        public static List<RaycastHit> RaycastAllSorted(Ray ray, int layerMask)
        {
            RaycastHit[] colliderHits = Physics.RaycastAll(ray, float.MaxValue, layerMask);
            if (colliderHits.Length == 0) return new List<RaycastHit>();

            List<RaycastHit> sortedColliderHits = new List<RaycastHit>(colliderHits);
            sortedColliderHits.Sort(delegate(RaycastHit firstHit, RaycastHit secondHit)
            {
                return firstHit.distance.CompareTo(secondHit.distance);
            });

            return sortedColliderHits;
        }
        #endregion
    }
}
