using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    public class MouseCursorRayHit
    {
        #region Private Variables
        private GridCellRayHit _gridCellRayHit;
        private List<GameObjectRayHit> _sortedObjectRayHits;
        #endregion

        #region Public Properties
        public GridCellRayHit GridCellRayHit { get { return _gridCellRayHit; } }
        public List<GameObjectRayHit> SortedObjectRayHits { get { return _sortedObjectRayHits; } }
        public GameObjectRayHit ClosestObjectRayHit { get { return _sortedObjectRayHits.Count != 0 ? _sortedObjectRayHits[0] : null; } }
        
        public bool WasACellHit { get { return _gridCellRayHit != null; } }
        public bool WasAnObjectHit 
        { 
            get 
            {
                _sortedObjectRayHits.RemoveAll(item => item.HitObject == null);
                return _sortedObjectRayHits.Count != 0; 
            } 
        }
        public bool WasAnythingHit { get { return WasACellHit || WasAnObjectHit; } }
        #endregion

        #region Constructors
        public MouseCursorRayHit(GridCellRayHit gridCellRayHit, List<GameObjectRayHit> sortedObjectRayHits)
        {
            _gridCellRayHit = gridCellRayHit;
            _sortedObjectRayHits = sortedObjectRayHits != null ? new List<GameObjectRayHit>(sortedObjectRayHits) : new List<GameObjectRayHit>();
   
        }
        #endregion

        #region Public Methods
        public List<GameObject> GetAllObjectsSortedByHitDistance()
        {
            if (!WasAnObjectHit) return new List<GameObject>();

            var allObjects = new List<GameObject>();
            foreach (var objectHit in _sortedObjectRayHits) allObjects.Add(objectHit.HitObject);

            return allObjects;
        }
        #endregion
    }
}