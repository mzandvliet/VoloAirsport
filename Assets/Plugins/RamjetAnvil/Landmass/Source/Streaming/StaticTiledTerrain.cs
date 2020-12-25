using System;
using System.Collections.Generic;
using System.IO;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Landmass;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

///
/// Todo:
/// - Cleanup, group all lod level properties in a neat struct
/// - Manage all LOD levels, not just 0

[ExecuteInEditMode]
public class StaticTiledTerrain : MonoBehaviour {
    [Dependency("sunLight"), SerializeField] private Transform _sunlightTransform;
    [SerializeField] private bool _apply;

    // Todo: One central, clearly defined place for all this config data
    [SerializeField] private int _lod0TileSize = 1024;
    [SerializeField] private int _lod0PatchSize = 64;
    [SerializeField] private int _lod0NumTiles = 8;
    [SerializeField] private int _lod0HeightResolution = 513;
    [SerializeField] private int _lod0SplatResolution = 512;
    [SerializeField] private float _terrainHeight = 4000f;

    [SerializeField] private bool _generateTreeColliders = true;
    [SerializeField] private string _treeLayerName = "Tree";
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _treeColliderPrefab;

    [SerializeField] private TerrainConfiguration _terrainConfiguration;

    [SerializeField] private TerrainMaterialLODConfiguration[] _materialConfigs = {
        new TerrainMaterialLODConfiguration(),
        new TerrainMaterialLODConfiguration(), 
    };

    [SerializeField] private Vector2 _detailDistance = new Vector2(125.0f, 250.0f);

    [SerializeField] private float _globalOcclusionIntensity = 0.4f;
    [SerializeField] private float _globalOcclusionPow = 2f;

    [SerializeField] private Vector4 _heightGain = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Vector4 _heightPow = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

    [SerializeField] private float _snowBandWidth = 200f;

    [SerializeField] private Vector4 _fresnelGain = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    [SerializeField] private Vector4 _fresnelPower = new Vector4(2.0f, 2.0f, 2.0f, 2.0f);

    [SerializeField] private Color _fresnelColor0 = Color.white;
    [SerializeField] private Color _fresnelColor1 = Color.white;
    [SerializeField] private Color _fresnelColor2 = Color.white;
    [SerializeField] private Color _fresnelColor3 = Color.white;

    private IList<TreeLODHelper> _trees;
    private IDictionary<int, TreeLODConfiguration> _treeQualitySettings;

    private TiledTerrainConfig _config;

    public float GlobalOcclusionIntensity {
        get { return _globalOcclusionIntensity; }
        set { _globalOcclusionIntensity = value; }
    }

    public float GlobalOcclusionPow {
        get { return _globalOcclusionPow; }
        set { _globalOcclusionPow = value; }
    }

    public TerrainConfiguration TerrainConfiguration
    {
        get { return _terrainConfiguration; }
    }

    public TiledTerrainConfig Config {
        get { return _config; }
    }

    public TerrainMaterialLODConfiguration[] MaterialConfigs {
        get { return _materialConfigs; }
    }

    public Transform SunlightTransform {
        get { return _sunlightTransform; }
        set { _sunlightTransform = value; }
    }

    private void Awake() {
        Initialize();
    }

    private void Start() {
        ApplyMaterials();
        ApplyTerrainQualitySettings(_terrainConfiguration);
    }

    private IList<TerrainTile> _tilesList;
    private IDictionary<int, IDictionary<IntVector3, TerrainTile>> _tiles;
    private TerrainTile[,] _lod0Grid;

    private void Update() {
        if (_sunlightTransform != null) {
            Shader.SetGlobalVector("_SunDir", _sunlightTransform.forward);
            Shader.SetGlobalFloat("_SunIntensity", _sunlightTransform.GetComponent<Light>().intensity);
        }

        if (_apply) { // Todo: This is hacky, just make a custom inspector
#if UNITY_EDITOR
            ApplyMaterials();
            ApplyTerrainQualitySettings(_terrainConfiguration);
            ApplyTreeQualitySettings(_terrainConfiguration.TreeQuality);
            ApplyWeatherSettings(_terrainConfiguration);
#else
            ApplyTerrainQualitySettings(_terrainConfiguration);
            ApplyTreeQualitySettings(_terrainConfiguration.TreeQuality);
            ApplyWeatherSettings(_terrainConfiguration);
#endif
            _apply = false;
        }
    }

    private void Initialize() {
        if (_tilesList != null) {
            return;
        }

        _config = new TiledTerrainConfig(
            _lod0NumTiles,
            _lod0TileSize,
            _lod0PatchSize,
            _lod0HeightResolution,
            _lod0SplatResolution,
            _terrainHeight);

        _trees = new List<TreeLODHelper>(10000);
        _treeQualitySettings = new Dictionary<int, TreeLODConfiguration>() {
            {0, new TreeLODConfiguration(false, 1.0f, 0.75f, 0.5f, 0.02f)},
            {1, new TreeLODConfiguration(false, 0.75f, 0.5f, 0.33f, 0.01f)},
            {2, new TreeLODConfiguration(false, 0.5f, 0.25f, 0.12f, 0.005f)},
            {3, new TreeLODConfiguration(true, 0.33f, 0.15f, 0.05f, 0.0025f)}
        };

        _tiles = LandmasImporter.FindTerrainTilesInScene();
        _tilesList = new List<TerrainTile>(128);

        foreach (var lodGroup in _tiles) {
            foreach (var regionTile in lodGroup.Value) {
                _tilesList.Add(regionTile.Value);

                LandmasImporter.SetNeighbours(regionTile.Key, lodGroup.Value);

                if (Application.isPlaying && _generateTreeColliders) {
                    CreateTrees(regionTile.Value);
                    CreateTreeColliders(regionTile.Value);
                }
            }
        }

        _lod0Grid = new TerrainTile[8,8];
        for (int x = 0; x < 8; x++) {
            for (int z = 0; z < 8; z++) {
                _lod0Grid[x, z] = _tiles[0][new IntVector3(x, 0, z)];
            }
        }
    }

    public void ApplyMaterials() {
        Initialize();

        foreach (var tile in _tilesList) {
            ApplyBasemapSettings(tile.Terrain);
        }


        for (int i = 0; i < _materialConfigs.Length; i++) {
            ApplyTerrainMaterialSettings(_materialConfigs[i]);
        }
    }

    private void ApplyBasemapSettings(Terrain terrain) {
        terrain.basemapDistance = 999999f;
    }

    private void ApplyTerrainMaterialSettings(TerrainMaterialLODConfiguration config) {
        var material = config.Material;

        material.SetTexture("_GlobalOcclusionTex", config.GlobalColorMap);
        material.SetTexture("_GlobalNormalTex", config.GlobalNormalMap);

        float sizeInv = 1f / config.TerrainSize;
        material.SetVector("_UVScale", new Vector4(1f / config.SplatUvScale.x, 1f / config.SplatUvScale.y, sizeInv, 1f));
        material.SetVector("_GlobalTexUVCoords", config.GlobalTexUvCoords);

        material.SetVector("_DetailDistance", _detailDistance);

        material.SetVector("_HeightGain", _heightGain);
        material.SetVector("_HeightPow", _heightPow);

        material.SetFloat("_GlobalOcclusionIntensity", _globalOcclusionIntensity);
        material.SetFloat("_GlobalOcculusionPow", _globalOcclusionPow);

        material.SetVector("_FresnelGain", _fresnelGain);
        material.SetVector("_FresnelPower", _fresnelPower);
        material.SetColor("_FresnelColor0", _fresnelColor0);
        material.SetColor("_FresnelColor1", _fresnelColor1);
        material.SetColor("_FresnelColor2", _fresnelColor2);
        material.SetColor("_FresnelColor3", _fresnelColor3);
    }

    public TerrainTile GetTile(Vector3 worldPos) {
        var index = new IntVector3(
            Mathf.FloorToInt((worldPos.x + _config.TotalTerrainSize * 0.5f) / _lod0TileSize),
            0,
            Mathf.FloorToInt((worldPos.z + _config.TotalTerrainSize * 0.5f) / _lod0TileSize));

        return _lod0Grid[index.X, index.Z];
    }

    public void ApplyTerrainQualitySettings(TerrainConfiguration configuration) {
        Initialize();

        foreach (var lodGroup in _tiles) {
            foreach (var regionTile in lodGroup.Value) {
                LandmasImporter.ApplyTerrainQualitySettings(regionTile.Value.Terrain, configuration);
            }
        }

        _terrainConfiguration = configuration;
    }

    public void ApplyTreeQualitySettings(TreeQuality quality) {
        Initialize();

        var qualityLevel = _treeQualitySettings[(int)quality];
        var fadeMode = qualityLevel.SmoothTransitions ? LODFadeMode.SpeedTree : LODFadeMode.None;

        for (int i = 0; i < _trees.Count; i++) {
            var lodGroup = _trees[i].LodGroup;
            lodGroup.fadeMode = fadeMode;
            lodGroup.animateCrossFading = qualityLevel.SmoothTransitions;

            var lods = lodGroup.GetLODs();
            lods[0].screenRelativeTransitionHeight = qualityLevel.Lod0;
            lods[1].screenRelativeTransitionHeight = qualityLevel.Lod1;
            lods[2].screenRelativeTransitionHeight = qualityLevel.Lod2;
            lods[3].screenRelativeTransitionHeight = qualityLevel.Lod3;
            lodGroup.SetLODs(lods);
        }
    }

    public void ApplyWeatherSettings(TerrainConfiguration configuration) {
        Initialize();

        _terrainConfiguration = configuration;

        Shader.SetGlobalVector("_SnowAltitude", new Vector4(
            configuration.SnowAltitude - _snowBandWidth,
            configuration.SnowAltitude + _snowBandWidth));

        Shader.SetGlobalFloat("_Fogginess", configuration.Fogginess);
    }

    private void CreateTrees(TerrainTile tile) {
        var terrainSize = tile.Terrain.terrainData.size;

        var trees = tile.Terrain.terrainData.treeInstances;
        for (int i = 0; i < trees.Length; i++) {
            var tree = trees[i];
            GameObject go = (GameObject)Instantiate(
                _treePrefab,
                tile.Terrain.transform.position + Vector3.Scale(tree.position, terrainSize),
                Quaternion.Euler(0f, tree.rotation, 0f));
            go.hideFlags = HideFlags.DontSave;
            go.transform.localScale = Vector3.Scale(
                go.transform.localScale,
                new Vector3(tree.widthScale, tree.heightScale, tree.widthScale));
            go.transform.parent = tile.Terrain.transform;

            go.tag = _treeLayerName;
            go.isStatic = true;

            _trees.Add(go.GetComponent<TreeLODHelper>());
        }
    }

    private void CreateTreeColliders(TerrainTile tile) {
        var terrainSize = tile.Terrain.terrainData.size;
        
        var trees = tile.Terrain.terrainData.treeInstances;
        for (int i = 0; i < trees.Length; i++) {
            var tree = trees[i];
            GameObject go = (GameObject)Instantiate(
                _treeColliderPrefab,
                tile.Terrain.transform.position + Vector3.Scale(tree.position, terrainSize),
                Quaternion.Euler(0f, tree.rotation, 0f));
            go.hideFlags = HideFlags.DontSave;
            go.transform.localScale = Vector3.Scale(
                go.transform.localScale,
                new Vector3(tree.widthScale, tree.heightScale, tree.widthScale));
            go.transform.parent = tile.Terrain.transform;

            go.tag = _treeLayerName;
            go.isStatic = true;
        }
    }

    [System.Serializable]
    public struct TerrainMaterialLODConfiguration {
        [SerializeField] public Material Material;
        [SerializeField] public Texture2D GlobalColorMap;
        [SerializeField] public Texture2D GlobalNormalMap;
        [SerializeField] public Vector4 GlobalTexUvCoords;
        [SerializeField] public Vector2 SplatUvScale;
        [SerializeField] public float TerrainSize;
    };

    public struct TreeLODConfiguration {
        public bool SmoothTransitions;
        public float Lod0;
        public float Lod1;
        public float Lod2;
        public float Lod3;

        public TreeLODConfiguration(bool smoothTransitions, float lod0, float lod1, float lod2, float lod3) {
            SmoothTransitions = smoothTransitions;
            Lod0 = lod0;
            Lod1 = lod1;
            Lod2 = lod2;
            Lod3 = lod3;
        }
    }

#if UNITY_EDITOR
    [MenuItem("Window/Landmass/Scale selection")]
    public static void ScaleSelection() {
        const float scaleFactor = 1f / 1000f * 1024f;

        for (int i = 0; i < Selection.gameObjects.Length; i++) {
            Vector3 pos = Selection.gameObjects[i].transform.position;
            pos.x *= scaleFactor;
            pos.z *= scaleFactor;
            Selection.gameObjects[i].transform.position = pos;
        }
    }

    [MenuItem("Window/Landmass/Make Terrains PoT Size")]
    public static void MakeTerrainsPoTSize() {
        Debug.Log("Make Terrains POT");

        var tiles = LandmasImporter.FindTerrainTilesInScene();

        const float lod0Size = 1024f;
        const float lod1Size = 8192;
        const float lod0HalfWorld = 8f * lod0Size / 2f;
        const float lod1HalfWorld = 3f * lod1Size / 2f;

        foreach (var lodGroup in tiles) {
            if (lodGroup.Key == 0) {
                foreach (var regionTile in lodGroup.Value) {
                    Vector3 pos = new Vector3(regionTile.Key.X * lod0Size - lod0HalfWorld, 0f, regionTile.Key.Z * lod0Size - lod0HalfWorld);
                    regionTile.Value.transform.position = pos;
                    regionTile.Value.Terrain.terrainData.size = new Vector3(lod0Size, regionTile.Value.Terrain.terrainData.size.y, lod0Size);
                }
            }

            if (lodGroup.Key == 1) {
                foreach (var regionTile in lodGroup.Value) {
                    Vector3 pos = new Vector3(regionTile.Key.X * lod1Size - lod1HalfWorld, 0f, regionTile.Key.Z * lod1Size - lod1HalfWorld);
                    regionTile.Value.transform.position = pos;
                    regionTile.Value.Terrain.terrainData.size = new Vector3(lod1Size, regionTile.Value.Terrain.terrainData.size.y, lod1Size);
                }
            }
        }
    }

    [MenuItem("Window/Landmass/Export Terrain")]
    public static void ExportTerrain() {
        var cfg = new TiledTerrainConfig(8, 1024, 32, 513, 512, 4000f); // Todo: get from global instead of hardcoding
        int totalPatches = cfg.NumTiles * cfg.PatchesPerTile;

        var lodTiles = LandmasImporter.FindTerrainTilesInScene();

        var path = UPath.GetAbsolutePath("Assets/Terrains/SwissAlps/swissalps.land");
        Debug.Log("Exporting terrain to: " + path);

        using (var writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
            try {
                for (int x = 0; x < totalPatches; x++) {
                    for (int z = 0; z < totalPatches; z++) {
                        // Find tile this patch is from
                        var tileIndex = new IntVector3(x / cfg.PatchesPerTile, 0, z / cfg.PatchesPerTile);
                        var tile = lodTiles[0][tileIndex];

                        // Find out which pixels we need to read for this patch
                        var startPatchIndex = new IntVector2(tileIndex.X * cfg.PatchesPerTile, tileIndex.Z * cfg.PatchesPerTile);
                        var localPatchIndex = new IntVector2(x, z) - startPatchIndex;
                        var heightPixIndex = new IntVector2(localPatchIndex.X * (cfg.PatchHeightRes-1), localPatchIndex.Y * (cfg.PatchHeightRes - 1));
                        var splatPixIndex = new IntVector2(localPatchIndex.X * cfg.PatchSplatRes, localPatchIndex.Y * cfg.PatchSplatRes);

                        // Sample PoT+1 blocks, so that each patch's edge overlaps the first edge of its neighbour
                        float[,] heights = GetHeights(tile.Terrain.terrainData, heightPixIndex, cfg.PatchHeightRes);
                        Vector3[,] normals = GetNormals(tile.Terrain.terrainData, heightPixIndex, cfg.PatchHeightRes);
                        float[,,] splats = GetSplats(tile.Terrain.terrainData, splatPixIndex, cfg.PatchSplatRes);

                        // Write heights
                        for (int xPixel = 0; xPixel < cfg.PatchHeightRes; xPixel++) {
                            for (int zPixel = 0; zPixel < cfg.PatchHeightRes; zPixel++) {
                                ushort val = (ushort)(heights[xPixel, zPixel] * 65000f);
                                writer.Write(val);
                            }
                        }

                        // Write normals
                        // Todo: calculate normals from loaded height values instead of caching them on disk
                        // Todo: single byte per axis, implicit 3rd axis

                        for (int xPixel = 0; xPixel < cfg.PatchHeightRes; xPixel++) {
                            for (int zPixel = 0; zPixel < cfg.PatchHeightRes; zPixel++) {
                                Vector3 normal = normals[xPixel, zPixel];
                                byte valX = (byte)((normal.x * 0.5f + 0.5f) * 250f);
                                byte valY = (byte)((normal.y * 0.5f + 0.5f) * 250f);
                                byte valZ = (byte)((normal.z * 0.5f + 0.5f) * 250f);
                                writer.Write(valX);
                                writer.Write(valY);
                                writer.Write(valZ);
                            }
                        }

                        // Write splats (todo: byte)
                        for (int xPixel = 0; xPixel < cfg.PatchSplatRes; xPixel++) {
                            for (int zPixel = 0; zPixel < cfg.PatchSplatRes; zPixel++) {
                                // Swizzled writes because Unity's terrain data is store wrong
                                byte valR = (byte)(splats[zPixel, xPixel, 0] * 250f); // Grass
                                byte valA = (byte)(splats[zPixel, xPixel, 3] * 250f); // Snow
                                writer.Write(valR);
                                writer.Write(valA);
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }

            writer.Close();
        }
    }

    private static float[,] GetHeights(TerrainData terrainData, IntVector2 index, int resolution) {
        float[,] heights = terrainData.GetHeights((int)index.X, (int)index.Y, resolution, resolution);
        return heights;
    }

    private static Vector3[,] GetNormals(TerrainData terrainData, IntVector2 index, int resolution) {
        Vector3[,] normals = new Vector3[resolution, resolution];
        for (int x = 0; x < resolution; x++) {
            for (int z = 0; z < resolution; z++) {
                float xNormal = (index.X + x) / (float)terrainData.heightmapResolution;
                float zNormal = (index.Y + z) / (float)terrainData.heightmapResolution;
                normals[x, z] = terrainData.GetInterpolatedNormal(xNormal, zNormal);
            }
        }
        return normals;
    }

    private static float[,,] GetSplats(TerrainData terrainData, IntVector2 index, int resolution) {
        float[,,] allSplats = terrainData.GetAlphamaps((int) index.X, (int) index.Y, resolution, resolution);
//        float[,] splats = new float[resolution, resolution];
//
//        for (int x = 0; x < resolution; x++) {
//            for (int z = 0; z < resolution; z++) {
//                splats[x, z] = allSplats[z, x, 0];
//            }
//        }

        return allSplats;
    }

#endif
}

namespace RamjetAnvil.Unity.Landmass {
    // Todo: Perhaps better to have this as a class to prevent useless duplication in memory
    public struct TiledTerrainConfig {
        private int _numTiles;
        private int _tileSize;
        private int _totalTerrainSize;

        private int _patchSize;
        private int _patchesPerTile;

        private int _tileHeightRes;
        private int _tileSplatRes;

        private int _patchHeightRes;
        private int _patchSplatRes;

        private float _terrainHeight;

        public int NumTiles {
            get { return _numTiles; }
            set {
                _numTiles = value;
                RecalculateDerivatives();
            }
        }

        public int PatchSize {
            get { return _patchSize; }
            set {
                _patchSize = value;
                RecalculateDerivatives();
            }
        }

        public int TileSize {
            get { return _tileSize; }
            set {
                _tileSize = value;
                RecalculateDerivatives();
            }
        }

        public int TileHeightRes {
            get { return _tileHeightRes; }
            set {
                _tileHeightRes = value;
                RecalculateDerivatives();
            }
        }

        public int TileSplatRes {
            get { return _tileSplatRes; }
            set {
                _tileSplatRes = value;
                RecalculateDerivatives();
            }
        }

        public int TotalTerrainSize {
            get { return _totalTerrainSize; }
        }

        public int PatchesPerTile {
            get { return _patchesPerTile; }
        }

        public int PatchHeightRes {
            get { return _patchHeightRes; }
        }

        public int PatchSplatRes {
            get { return _patchSplatRes; }
        }

        public float TerrainHeight
            { get { return _terrainHeight; } }

        public TiledTerrainConfig(int numTiles, int tileSize, int patchSize, int tileHeightRes, int tileSplatRes, float terrainHeight) {
            _numTiles = numTiles;
            _tileSize = tileSize;
            _patchSize = patchSize;

            _tileHeightRes = tileHeightRes;
            _tileSplatRes = tileSplatRes;

            _totalTerrainSize = 0;
            _patchesPerTile = 0;
            _patchHeightRes = 0;
            _patchSplatRes = 0;

            _terrainHeight = terrainHeight;

            RecalculateDerivatives();
        }

        private void RecalculateDerivatives() {
            _totalTerrainSize = _numTiles * _tileSize;
            _patchesPerTile = _tileSize / _patchSize;
            _patchHeightRes = (_tileHeightRes / _patchesPerTile) + 1; // Note: This assumes _tileHeightRes = PoT+1, and makes sure _patchHeightRes = also PoT+1
            _patchSplatRes = _tileSplatRes / _patchesPerTile;
        }
    }
}