using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class MouseCursor : SingletonBase<MouseCursor>
    {
        #region Private Variables
        private Vector2 _previousPosition;
        private Vector2 _offsetSinceLastMouseMove;

        private Stack<MouseCursorObjectPickFlags> _objectPickMaskFlagsStack = new Stack<MouseCursorObjectPickFlags>();
        #endregion

        #region Public Properties
        public Vector2 PreviousPosition { get { return _previousPosition; } }
        public Vector2 OffsetSinceLastMouseMove { get { return _offsetSinceLastMouseMove; } }
        public Vector2 Position { get { return Event.current.mousePosition; } }
        public MouseCursorObjectPickFlags ObjectPickMaskFlags { get { return _objectPickMaskFlagsStack.Count != 0 ? _objectPickMaskFlagsStack.Peek() : MouseCursorObjectPickFlags.None; } }
        #endregion

        #region Public Methods
        public bool IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags flag)
        {
            return (ObjectPickMaskFlags & flag) != 0;
        }

        public void PushObjectPickMaskFlags(MouseCursorObjectPickFlags flags)
        {
            _objectPickMaskFlagsStack.Push(flags);
        }

        public Ray GetWorldRay()
        {
            return EditorCamera.Instance.Camera.ScreenPointToRay(Input.mousePosition);
        }

        public MouseCursorRayHit GetRayHit()
        {
            var hit = new MouseCursorRayHit(GetGridCellRayHit(), GetObjectRayHitInstances());
            PopObjectPickMaskFlags();

            return hit;
        }

        public MouseCursorRayHit GetRayHit(int layerMask)
        {
            List<GameObjectRayHit> gameObjectRayHits = GetObjectRayHitInstances();
            gameObjectRayHits.RemoveAll(item => !LayerHelper.IsLayerBitSet(layerMask, item.HitObject.layer));

            var hit = new MouseCursorRayHit(GetGridCellRayHit(), gameObjectRayHits);
            PopObjectPickMaskFlags();

            return hit;
        }

        public MouseCursorRayHit GetCursorRayHitForGridCell()
        {
            GridCellRayHit gridCellHit = GetGridCellRayHit();
            if (gridCellHit == null) return null;

            return new MouseCursorRayHit(gridCellHit, new List<GameObjectRayHit>());
        }

        public MouseCursorRayHit GetCursorRayHitForTerrainObject(GameObject gameObject)
        {
            if (!gameObject.HasTerrain()) return new MouseCursorRayHit(null, new List<GameObjectRayHit>());

            GameObjectRayHit gameObjectRayHit;
            if (gameObject.RaycastTerrain(GetWorldRay(), out gameObjectRayHit)) return new MouseCursorRayHit(null, new List<GameObjectRayHit> { gameObjectRayHit });

            return new MouseCursorRayHit(null, new List<GameObjectRayHit>());
        }

        public MouseCursorRayHit GetCursorRayHitForMeshObject(GameObject gameObject)
        {
            if (!gameObject.HasMesh()) return new MouseCursorRayHit(null, new List<GameObjectRayHit>());

            GameObjectRayHit gameObjectRayHit;
            if (gameObject.RaycastMesh(GetWorldRay(), out gameObjectRayHit)) return new MouseCursorRayHit(null, new List<GameObjectRayHit> { gameObjectRayHit });

            return new MouseCursorRayHit(null, new List<GameObjectRayHit>());
        }

        public GridCellRayHit GetGridCellRayHit()
        {
            Ray ray = GetWorldRay();

            float minT;
            XZGrid closestGrid = GetClosestHitGridAndMinT(new List<XZGrid> { RuntimeEditorApplication.Instance.XZGrid }, ray, out minT);

            if (closestGrid != null) return GetGridCellHit(closestGrid, ray, minT);
            else return null;
        }

        public bool IntersectsPlane(Plane plane, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.zero;

            Ray ray = GetWorldRay();
            float t;
            if(plane.Raycast(ray, out t))
            {
                intersectionPoint = ray.GetPoint(t);
                return true;
            }

            return false;
        }

        public void HandleMouseMoveEvent(Event e)
        {
            _offsetSinceLastMouseMove = e.mousePosition - _previousPosition;
            _previousPosition = e.mousePosition;
        }
        #endregion

        #region Private Methods
        private MouseCursorObjectPickFlags PopObjectPickMaskFlags()
        {
            if (_objectPickMaskFlagsStack.Count != 0) return _objectPickMaskFlagsStack.Pop();
            return MouseCursorObjectPickFlags.None;
        }

        private List<GameObjectRayHit> GetObjectRayHitInstances()
        {
            Ray ray = GetWorldRay();
            var gameObjectHits = new List<GameObjectRayHit>();

            RaycastAllTerrainObjects(ray, gameObjectHits);
            RaycastAllObjectsNoTerrains(ray, gameObjectHits);
            SortObjectRayHitListByHitDistanceFromCamera(gameObjectHits);

            return gameObjectHits;
        }

        private void RaycastAllTerrainObjects(Ray ray, List<GameObjectRayHit> terrainHits)
        {
            // Can we pick terrains?
            if (!IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectTerrain))
            {
                // We will use Unity's 'Physics' API for terrain picking because it is reasonable enough
                // to expect users to attach terrain colliders to their terrain objects.
                RaycastHit[] rayHits = Physics.RaycastAll(ray);
                if (rayHits.Length != 0)
                {
                    // Identify all terrain colliders which were picked
                    foreach (RaycastHit rayHit in rayHits)
                    {
                        // Picked a terrain collider?
                        if (rayHit.collider.GetType() == typeof(TerrainCollider))
                        {
                            // Create a game object hit instance and add it to the list
                            var terrainRayHit = new TerrainRayHit(ray, rayHit);
                            var gameObjectRayHit = new GameObjectRayHit(ray, rayHit.collider.gameObject, null, null, terrainRayHit, null);
                            terrainHits.Add(gameObjectRayHit);
                        }
                    }
                }
            }
        }

        private void RaycastAllObjectsNoTerrains(Ray ray, List<GameObjectRayHit> objectHits)
        {
            bool canPickMeshObjects = !IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectMesh);
            bool canPickBoxes = !IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectBox);
            bool canPickSprites = !IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectSprite);

            if (canPickMeshObjects && canPickBoxes && canPickSprites)
            {
                List<GameObjectRayHit> objectMeshHits = EditorScene.Instance.RaycastAllMesh(ray);
                if (objectMeshHits.Count != 0) objectHits.AddRange(objectMeshHits);

                List<GameObjectRayHit> objectBoxHits = EditorScene.Instance.RaycastAllBox(ray);
                objectBoxHits.RemoveAll(item => item.HitObject.HasMesh() || item.HitObject.HasSpriteRendererWithSprite());
                if (objectBoxHits.Count != 0) objectHits.AddRange(objectBoxHits);

                List<GameObjectRayHit> objectSpriteHits = EditorScene.Instance.RaycastAllSprite(ray);
                objectSpriteHits.RemoveAll(item => item.HitObject.HasMesh() || item.HitObject.GetComponent<SpriteRenderer>().IsPixelFullyTransparent(item.HitPoint));
                if (objectSpriteHits.Count != 0) objectHits.AddRange(objectSpriteHits);
            }
            else
            {
                if (!IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectMesh))
                {
                    List<GameObjectRayHit> objectMeshHits = EditorScene.Instance.RaycastAllMesh(ray);
                    if (objectMeshHits.Count != 0) objectHits.AddRange(objectMeshHits);
                }

                if(!IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectSprite))
                {
                    List<GameObjectRayHit> objectSpriteHits = EditorScene.Instance.RaycastAllSprite(ray);
                    objectSpriteHits.RemoveAll(item => objectHits.Contains(item) || item.HitObject.GetComponent<SpriteRenderer>().IsPixelFullyTransparent(item.HitPoint));
                    if (objectSpriteHits.Count != 0) objectHits.AddRange(objectSpriteHits);
                }

                if (!IsObjectPickMaskFlagSet(MouseCursorObjectPickFlags.ObjectBox))
                {
                    List<GameObjectRayHit> objectBoxHits = EditorScene.Instance.RaycastAllBox(ray);
                    objectBoxHits.RemoveAll(item => objectHits.Contains(item));
                    if (objectBoxHits.Count != 0) objectHits.AddRange(objectBoxHits);
                }
            }
        }

        private XZGrid GetClosestHitGridAndMinT(List<XZGrid> allSnapGrids, Ray ray, out float minT)
        {
            minT = float.MaxValue;

            XZGrid closestSnapGrid = null;
            foreach (XZGrid snapGrid in allSnapGrids)
            {
                float t;
                if (snapGrid.Plane.Raycast(ray, out t) & t < minT)
                {
                    minT = t;
                    closestSnapGrid = snapGrid;
                }
            }

            return closestSnapGrid;
        }

        private GridCellRayHit GetGridCellHit(XZGrid hitGrid, Ray ray, float t)
        {
            XZGridCell hitGridCell = hitGrid.GetCellFromWorldPoint(ray.GetPoint(t));
            return new GridCellRayHit(ray, t, hitGridCell);
        }
                
        private void SortObjectRayHitListByHitDistanceFromCamera(List<GameObjectRayHit> objectRayHitInstances)
        {
            Vector3 sceneCameraPosition = EditorCamera.Instance.Camera.transform.position;
            objectRayHitInstances.Sort(delegate(GameObjectRayHit firstObjectHit, GameObjectRayHit secondObjectHit)
            {
                float firstPickPointDistanceFromCamera = (firstObjectHit.HitPoint - sceneCameraPosition).magnitude;
                float secondPickPointDistanceFromCamera = (secondObjectHit.HitPoint - sceneCameraPosition).magnitude;

                return firstPickPointDistanceFromCamera.CompareTo(secondPickPointDistanceFromCamera);
            });
        }
        #endregion
    }
}