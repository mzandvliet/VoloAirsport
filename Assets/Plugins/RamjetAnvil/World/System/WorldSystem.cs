using System;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class WorldSystem : MonoBehaviour {
    private static WorldSystem _instance;
    public static WorldSystem Instance {
        get {
            if (!_instance) {
                _instance = FindObjectOfType<WorldSystem>();
            }
            return _instance;
        }
    }

    [SerializeField] private WorldTransform _perspective;
    [SerializeField] private double _sceneOriginStartX = 0d;
    [SerializeField] private double _sceneOriginStartY = 0d;
    [SerializeField] private double _sceneOriginStartZ = 0d;
    [SerializeField] private double _regionSize = 1000d;

    private IList<WorldTransform> _transforms;
    private IntVector3 _region;
    private DVector3 _sceneOrigin;

    public event Action<IntVector3, Vector3> OnRegionChanged;

    private void Awake() {
        _transforms = new List<WorldTransform>();
        _sceneOrigin = new DVector3(
            _sceneOriginStartX,
            _sceneOriginStartY,
            _sceneOriginStartZ);
        _region = WorldPointToRegion(_sceneOrigin);
    }

    private void Start() {
        if (OnRegionChanged != null) {
            OnRegionChanged(_region, Vector3.zero);
        }
    }

    public void Add(WorldTransform worldTransform) {
        if (_transforms.Contains(worldTransform)) {
            throw new ArgumentException("Add failed. The given world transform is already registered");
        }

        _transforms.Add(worldTransform);
    }

    public void Remove(WorldTransform worldTransform) {
        if (!_transforms.Contains(worldTransform)) {
            throw new ArgumentException("Remove failed. The given world transform is not currently registered");
        }

        _transforms.Remove(worldTransform);
    }

    public DVector3 LocalPointToWorld(Vector3 local) {
        return _sceneOrigin + new DVector3(local);
    }

    public Vector3 WorldPointToLocal(DVector3 world) {
        return DVector3.ToVector3(world - _sceneOrigin);
    }

    public IntVector3 WorldPointToRegion(DVector3 worldPosition) {
        return new IntVector3(
            (int)(worldPosition.X / _regionSize),
            (int)(worldPosition.Y / _regionSize),
            (int)(worldPosition.Z / _regionSize));
    }

    public DVector3 RegionToWorldPoint(IntVector3 region) {
        return new DVector3(
            region.X * _regionSize,
            region.Y * _regionSize,
            region.Z * _regionSize);
    }

    private void Update() {
        Vector3 scenePosition = _perspective.transform.position;
        DVector3 worldPosition = LocalPointToWorld(scenePosition);
        IntVector3 region = WorldPointToRegion(worldPosition);

        if (region != _region) {
            MoveToRegion(region);
        }
            
    }

    private void MoveToRegion(IntVector3 newRegion) {
//        DVector3 delta = GetDelta(newRegion, _region);
//        _region = newRegion;
//        _sceneOrigin += delta;
//
//        Vector3 translation = DVector3.ToVector3(-delta);
//
//        for (int i = 0; i < _transforms.Count; i++) {
//            _transforms[i].transform.Translate(translation, Space.World);
//        }

        if (OnRegionChanged != null) {
            OnRegionChanged(newRegion, Vector3.zero);
        }
    }

    private DVector3 GetDelta(IntVector3 a, IntVector3 b) {
        IntVector3 delta = a - b;
        return new DVector3(
            delta.X * _regionSize,
            delta.Y * _regionSize,
            delta.Z * _regionSize);
    }
}
