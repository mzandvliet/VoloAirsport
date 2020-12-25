using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace RamjetAnvil.Unity.Landmass
{
    //public interface IDirtTracker {
    //    void MakeDirty();
    //}

    [System.Serializable]
    public class ImporterConfiguration
    {
        #region Fields

        [XmlIgnore]
        private bool _isDirty;

        private int _batchLimit = 48;

        private string _heightmapTag = "_h";
        private string _splatmapTag = "_s";
        private string _detailmapTag = "_d";
        private string _treemapTag = "_t";
        private string _heightmapExtention = "png";
        private string _splatmapExtention = "png";
        private string _sceneExtention = "unity";
        private string _terrainDataExtention = "asset";
        private HeightfileFormat _heightFormat = HeightfileFormat.Windows;
        private NormalizationMode _normalizationMode = NormalizationMode.Equal;

        private List<LodLevel> _lodLevels;
        private List<SplatPrototypeConfiguration> _splatPrototypes;
        private List<DetailPrototypeConfiguration> _detailPrototypes;
        private List<TreePrototypeConfiguration> _treePrototypes;
        private TerrainConfiguration _terrainConfiguration;

        private bool _heightmapFlipX;
        private bool _heightmapFlipY;
        private bool _splatmapFlipX;
        private bool _splatmapFlipY;
        private bool _trimEmptyChannels;

        #endregion

        #region Properties

        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public int BatchLimit
        {
            get { return _batchLimit; }
            set
            {
                if (_batchLimit == value) return;
                _batchLimit = value;
                IsDirty = true;
            }
        }

        public string HeightmapExtention
        {
            get { return _heightmapExtention; }
            set
            {
                if (_heightmapExtention != value)
                {
                    _heightmapExtention = FormatFileExtention(value);
                    IsDirty = true;
                }
            }
        }

        public string SplatmapExtention
        {
            get { return _splatmapExtention; }
            set
            {
                if (_splatmapExtention != value)
                {
                    _splatmapExtention = FormatFileExtention(value);
                    IsDirty = true;
                }
            }
        }

        public string TerrainDataExtention
        {
            get { return _terrainDataExtention; }
            set
            {
                if (_terrainDataExtention != value)
                {
                    _terrainDataExtention = FormatFileExtention(value);
                    IsDirty = true;
                }
            }
        }

        public string SceneExtention
        {
            get { return _sceneExtention; }
            set
            {
                if (_sceneExtention != value)
                {
                    _sceneExtention = FormatFileExtention(value);
                    IsDirty = true;
                }
            }
        }

        public string HeightmapTag
        {
            get { return _heightmapTag; }
            set
            {
                if (_heightmapTag != value && ValidateFilePostfix(value))
                {
                    _heightmapTag = value;
                    IsDirty = true;
                }
            }
        }

        public string SplatmapTag
        {
            get { return _splatmapTag; }
            set
            {
                if (_splatmapTag != value && ValidateFilePostfix(value))
                {
                    _splatmapTag = value;
                    IsDirty = true;
                }
            }
        }

        public string DetailmapTag
        {
            get { return _detailmapTag; }
            set
            {
                if (_detailmapTag != value && ValidateFilePostfix(value))
                {
                    _detailmapTag = value;
                    IsDirty = true;
                }
            }
        }

        public string TreemapTag
        {
            get { return _treemapTag; }
            set
            {
                if (_treemapTag != value && ValidateFilePostfix(value))
                {
                    _treemapTag = value;
                    IsDirty = true;
                }
            }
        }

        public HeightfileFormat HeightFormat
        {
            get { return _heightFormat; }
            set
            {
                if (_heightFormat != value)
                {
                    _heightFormat = value;
                    IsDirty = true;
                }
            }
        }

        public NormalizationMode NormalizationMode
        {
            get { return _normalizationMode; }
            set
            {
                if (_normalizationMode != value)
                {
                    _normalizationMode = value;
                    IsDirty = true;
                }
            }
        }

        public List<LodLevel> LodLevels {
            get { return _lodLevels; }
            set {
                if (_lodLevels != value) {
                    _lodLevels = value;
                    IsDirty = true;
                }
            }
        }

        public List<SplatPrototypeConfiguration> SplatPrototypes
        {
            get { return _splatPrototypes; }
            set
            {
                if (_splatPrototypes != value)
                {
                    _splatPrototypes = value;
                    IsDirty = true;
                }
            }
        }

        public List<DetailPrototypeConfiguration> DetailPrototypes {
            get { return _detailPrototypes; }
            set {
                if (_detailPrototypes != value) {
                    _detailPrototypes = value;
                    IsDirty = true;
                }
            }
        }

        public List<TreePrototypeConfiguration> TreePrototypes {
            get { return _treePrototypes; }
            set {
                if (_treePrototypes != value) {
                    _treePrototypes = value;
                    IsDirty = true;
                }
            }
        }

        public TerrainConfiguration TerrainConfiguration
        {
            get { return _terrainConfiguration; }
            set
            {
                if (_terrainConfiguration != value)
                {
                    _terrainConfiguration = value;
                    IsDirty = true;
                }
            }
        }

        public bool HeightmapFlipX
        {
            get { return _heightmapFlipX; }
            set
            {
                if (_heightmapFlipX != value)
                {
                    _heightmapFlipX = value;
                    IsDirty = true;
                }
            }
        }

        public bool HeightmapFlipY
        {
            get { return _heightmapFlipY; }
            set
            {
                if (_heightmapFlipY != value)
                {
                    _heightmapFlipY = value;
                    IsDirty = true;
                }
            }
        }

        public bool SplatmapFlipX
        {
            get { return _splatmapFlipX; }
            set
            {
                if (_splatmapFlipX != value)
                {
                    _splatmapFlipX = value;
                    IsDirty = true;
                }
            }
        }

        public bool SplatmapFlipY
        {
            get { return _splatmapFlipY; }
            set
            {
                if (_splatmapFlipY != value)
                {
                    _splatmapFlipY = value;
                    IsDirty = true;
                }
            }
        }

        public bool TrimEmptyChannels
        {
            get { return _trimEmptyChannels; }
            set
            {
                if (_trimEmptyChannels != value)
                {
                    _trimEmptyChannels = value;
                    IsDirty = true;
                }
            }
        }

        #endregion

        public ImporterConfiguration()
        {
            _lodLevels = new List<LodLevel>();
            _splatPrototypes = new List<SplatPrototypeConfiguration>();
            _detailPrototypes = new List<DetailPrototypeConfiguration>();
            _treePrototypes = new List<TreePrototypeConfiguration>();

            if (_terrainConfiguration == null) {
                _terrainConfiguration = new TerrainConfiguration();
            }
        }

        // Todo: Use regex for a more comprehensive check. Use GUI to reflect state, not console.
        string FormatFileExtention(string extention)
        {
            if (!extention.StartsWith("."))
                extention = "." + extention;
            return extention;
        }

        // Todo: Use regex for a more comprehensive check. Use GUI to reflect state, not console.
        bool ValidateFilePostfix(string postfix)
        {
            bool valid = postfix != string.Empty;
            if (!valid)
                Debug.LogWarning("Postfix cannot be empty");
            return valid;
        }
    }

    [System.Serializable]
    public class LodLevel {
        private string _folderPath = "Assets/Terrains/Dormant/lod0/";
        
        private int _level = 0;
        private int _gridSize = 3;

        private bool _hasDetailMap = true;
        private bool _hasTreeMap = true;

        private int _heightmapResolution = 513;
        private int _splatmapResolution = 256;
        private int _detailmapResolution = 256;
        private int _detailResolutionPerPatch = 16;
        private int _colormapResolution = 256;
        private int _normalmapResolution = 256;
        private float _terrainWidth = 1000f;
        private float _terrainHeight = 1000f;

        public string FolderPath {
            get { return _folderPath; }
            set {
                if (_folderPath != value) {
                    _folderPath = value;
                }
            }
        }

        public int Level {
            get { return _level; }
            set {
                if (_level != value) {
                    _level = value;
                }
            }
        }

        public int GridSize {
            get { return _gridSize; }
            set {
                if (_gridSize != value) {
                    _gridSize = value;
                }
            }
        }

        public bool HasDetailMap {
            get { return _hasDetailMap; }
            set {
                if (_hasDetailMap != value) {
                    _hasDetailMap = value;
                }
            }
        }

        public bool HasTreeMap {
            get { return _hasTreeMap; }
            set {
                if (_hasTreeMap != value) {
                    _hasTreeMap = value;
                }
            }
        }

        public int HeightmapResolution {
            get { return _heightmapResolution; }
            set {
                if (_heightmapResolution != value) {
                    _heightmapResolution = Mathf.ClosestPowerOfTwo(value) + 1;
                }
            }
        }

        public int SplatmapResolution {
            get { return _splatmapResolution; }
            set {
                if (_splatmapResolution != value) {
                    _splatmapResolution = Mathf.ClosestPowerOfTwo(value);
                }
            }
        }

        public int DetailmapResolution {
            get { return _detailmapResolution; }
            set {
                if (_detailmapResolution != value) {
                    _detailmapResolution = Mathf.ClosestPowerOfTwo(value);
                }
            }
        }

        public int DetailResolutionPerPatch {
            get { return _detailResolutionPerPatch; }
            set {
                if (_detailResolutionPerPatch != value) {
                    _detailResolutionPerPatch = Mathf.ClosestPowerOfTwo(value);
                }
            }
        }

        public int NormalmapResolution {
            get { return _normalmapResolution; }
            set {
                if (_normalmapResolution != value) {
                    _normalmapResolution = Mathf.ClosestPowerOfTwo(value);
                }
            }
        }

        public int ColormapResolution {
            get { return _colormapResolution; }
            set {
                if (_colormapResolution != value) {
                    _colormapResolution = Mathf.ClosestPowerOfTwo(value);
                }
            }
        }

        public float TerrainWidth {
            get { return _terrainWidth; }
            set {
                if (_terrainWidth != value) {
                    _terrainWidth = value;
                }
            }
        }

        public float TerrainHeight {
            get { return _terrainHeight; }
            set {
                if (_terrainHeight != value) {
                    _terrainHeight = value;
                }
            }
        }
    }

    [System.Serializable]
    public class SplatPrototypeConfiguration
    {
        private string _diffusePath = "";
        private string _normalPath = "";
        private string _heightPath = "";
        private Vector2 _tileOffset = Vector3.zero;
        private Vector2 _tileSize = Vector3.one;

        public string DiffusePath
        {
            get { return _diffusePath; }
            set { _diffusePath = value; }
        }

        public string NormalPath {
            get { return _normalPath; }
            set { _normalPath = value; }
        }

        public string HeightPath {
            get { return _heightPath; }
            set { _heightPath = value; }
        }

        public Vector2 TileOffset
        {
            get { return _tileOffset; }
            set { _tileOffset = value; }
        }

        public Vector2 TileSize
        {
            get { return _tileSize; }
            set { _tileSize = value; }
        }

        public static SplatPrototypeConfiguration Serialize(SplatPrototype prototype) {
            return new SplatPrototypeConfiguration() {
                _diffusePath = AssetDatabase.GetAssetPath(prototype.texture),
                _tileOffset = prototype.tileOffset,
                _tileSize = prototype.tileSize
            };
        }

        public static SplatPrototype Deserialize(SplatPrototypeConfiguration config) {
            return new SplatPrototype() {
                texture = AssetDatabase.LoadAssetAtPath(config.DiffusePath, typeof (Texture2D)) as Texture2D,
                tileOffset = config.TileOffset,
                tileSize = config.TileSize
            };
        }
    }

    [System.Serializable]
    public struct DetailPrototypeConfiguration {
        private string _prototypePath;
        private string _prototypeTexturePath;
        
        private float _minWidth;
        private float _maxWidth;
        private float _minHeight;
        private float _maxHeight;
        private float _noiseSpread;
        private float _bendFactor;
        private Color _healthyColor;
        private Color _dryColor;
        private DetailRenderMode _renderMode;
        private bool _usePrototypeMesh;

        public string PrototypePath {
            get { return _prototypePath; }
            set { _prototypePath = value; }
        }

        public string PrototypeTexturePath {
            get { return _prototypeTexturePath; }
            set { _prototypeTexturePath = value; }
        }

        public float MinWidth {
            get { return _minWidth; }
            set { _minWidth = value; }
        }

        public float MaxWidth {
            get { return _maxWidth; }
            set { _maxWidth = value; }
        }

        public float MinHeight {
            get { return _minHeight; }
            set { _minHeight = value; }
        }

        public float MaxHeight {
            get { return _maxHeight; }
            set { _maxHeight = value; }
        }

        public float NoiseSpread {
            get { return _noiseSpread; }
            set { _noiseSpread = value; }
        }

        public float BendFactor {
            get { return _bendFactor; }
            set { _bendFactor = value; }
        }

        public Color HealthyColor {
            get { return _healthyColor; }
            set { _healthyColor = value; }
        }

        public Color DryColor {
            get { return _dryColor; }
            set { _dryColor = value; }
        }

        public DetailRenderMode RenderMode {
            get { return _renderMode; }
            set { _renderMode = value; }
        }

        public bool UsePrototypeMesh {
            get { return _usePrototypeMesh; }
            set { _usePrototypeMesh = value; }
        }

        public static DetailPrototypeConfiguration Serialize(DetailPrototype prototype) {
            return new DetailPrototypeConfiguration() {
                _bendFactor = prototype.bendFactor,
                _dryColor = prototype.dryColor,
                _healthyColor = prototype.healthyColor,
                _maxHeight = prototype.maxHeight,
                _maxWidth = prototype.maxWidth,
                _minHeight = prototype.minHeight,
                _minWidth = prototype.minWidth,
                _noiseSpread = prototype.noiseSpread,
                _prototypePath = prototype.prototype ? AssetDatabase.GetAssetPath(prototype.prototype) : "",
                _prototypeTexturePath = prototype.prototypeTexture ? AssetDatabase.GetAssetPath(prototype.prototypeTexture) : "",
                _renderMode = prototype.renderMode,
                _usePrototypeMesh = prototype.usePrototypeMesh
            };
        }

        public static DetailPrototype Deserialize(DetailPrototypeConfiguration config) {
            return new DetailPrototype() {
                bendFactor = config.BendFactor,
                dryColor = config.DryColor,
                healthyColor = config.HealthyColor,
                maxHeight = config.MaxHeight,
                maxWidth = config.MaxWidth,
                minHeight = config.MinHeight,
                minWidth = config.MinWidth,
                noiseSpread = config.NoiseSpread,
                usePrototypeMesh = config.UsePrototypeMesh,
                prototype = config.UsePrototypeMesh ? AssetDatabase.LoadAssetAtPath(config.PrototypePath, typeof(GameObject)) as GameObject : null,
                prototypeTexture = !config.UsePrototypeMesh ? AssetDatabase.LoadAssetAtPath(config.PrototypeTexturePath, typeof(Texture2D)) as Texture2D : null,
                renderMode = config.RenderMode,
                
            };
        }
    }

    [System.Serializable]
    public class TreePrototypeConfiguration {
        private float _bendFactor;
        private string _prefabPath;

        public float BendFactor {
            get { return _bendFactor; }
            set { _bendFactor = value; }
        }

        public string PrefabPath {
            get { return _prefabPath; }
            set { _prefabPath = value; }
        }

        public static TreePrototypeConfiguration Serialize(TreePrototype prototype) {
            return new TreePrototypeConfiguration() {
                _prefabPath = AssetDatabase.GetAssetPath(prototype.prefab),
                _bendFactor = prototype.bendFactor
            };
        }

        public static TreePrototype Deserialize(TreePrototypeConfiguration config) {
            return new TreePrototype() {
                prefab = AssetDatabase.LoadAssetAtPath(config.PrefabPath, typeof (GameObject)) as GameObject,
                bendFactor = config.BendFactor
            };
        }
    }
}