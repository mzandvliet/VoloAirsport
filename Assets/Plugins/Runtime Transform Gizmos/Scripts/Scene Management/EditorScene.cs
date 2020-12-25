using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    public class EditorScene : SingletonBase<EditorScene>
    {
        #region Private Variables
        private GameObjectSphereTree _gameObjectSphereTree = new GameObjectSphereTree(2);
        #endregion

        #region Public Methods
        public void Update()
        {
            _gameObjectSphereTree.Update();
        }

        public List<GameObjectRayHit> RaycastAllBox(Ray ray)
        {
            return _gameObjectSphereTree.RaycastAllBox(ray);
        }

        public List<GameObjectRayHit> RaycastAllSprite(Ray ray)
        {
            return _gameObjectSphereTree.RaycastAllSprite(ray);
        }

        public List<GameObjectRayHit> RaycastAllMesh(Ray ray)
        {
            return _gameObjectSphereTree.RaycastAllMesh(ray);
        }

        public List<GameObject> OverlapSphere(Sphere3D sphere, ObjectOverlapPrecision overlapPrecision = ObjectOverlapPrecision.ObjectBox)
        {
            return _gameObjectSphereTree.OverlapSphere(sphere, overlapPrecision);
        }

        public List<GameObject> OverlapBox(OrientedBox box, ObjectOverlapPrecision overlapPrecision = ObjectOverlapPrecision.ObjectBox)
        {
            return _gameObjectSphereTree.OverlapBox(box, overlapPrecision);
        }

        public List<GameObject> OverlapBox(Box box, ObjectOverlapPrecision overlapPrecision = ObjectOverlapPrecision.ObjectBox)
        {
            return _gameObjectSphereTree.OverlapBox(box, overlapPrecision);
        }
        #endregion
    }
}