using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class TerrainTile : MonoBehaviour {
    [SerializeField] private SerializableRegion _region;
    [SerializeField] private int _lodLevel;
    [SerializeField] private Terrain _terrain;

    public IntVector3 Region {
        get { return new IntVector3(_region.X, _region.Y, _region.Z); }
        set { _region = new SerializableRegion(value.X, value.Y, value.Z); }
    }

    public int LodLevel {
        get { return _lodLevel; }
        set { _lodLevel = value; }
    }

    public Terrain Terrain {
        get { return _terrain; }
        set { _terrain = value; }
    }

    private void Awake() {
        if (_terrain == null) {
            Debug.LogWarning("The terrain reference for this terrain tile was not correctly set by the importer.", this);
            _terrain = GetComponent<Terrain>();
        }
    }

    [System.Serializable]
    public class SerializableRegion {
        [SerializeField] public int X;
        [SerializeField] public int Y;
        [SerializeField] public int Z;

        public SerializableRegion(long x, long y, long z) {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }
    }
}
