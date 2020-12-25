using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RamjetAnvil.Unity.Landmass;

using EG = UnityEditor.EditorGUI;
using EGL = UnityEditor.EditorGUILayout;

public class LandmassImporterEditorWindow : EditorWindow
{
    string readmeUrl = "http://www.ramjetanvil.com/software/landmass/readme.html";
    string version = "0.1";

    delegate void DrawMenuState();

    enum MenuState
    {
        Single,
        Tiled,
        Heightmaps,
        Splatmaps,
        Detailmaps,
        Treemaps,
        Terrains,
        Settings,
        Help
    }

    MenuState _currentState = MenuState.Single;
    Dictionary<MenuState, DrawMenuState> _menus;
    string[] menuTitles;

    Vector2 _globalScrollPos = Vector2.zero;
    Vector2 _prototypeScrollPos = Vector2.zero;
    Vector2 _lodScrollPos = Vector2.zero;

    // Used to trigger processing methods outside of the GUI loop
    Queue<ImporterTask> _queuedCalls;

    [MenuItem("Window/Landmass/Open Importer Window %&i")]
    public static void Initialize()
    {
        LandmassImporterEditorWindow window = GetWindow<LandmassImporterEditorWindow>(false, "Landmass", true);
        window.minSize = new Vector2(496f, 496f);
        window.Show();
    }

    public LandmassImporterEditorWindow()
    {
        // Setup menus
        _menus = new Dictionary<MenuState, DrawMenuState>();
        _menus.Add(MenuState.Single, ShowSingleImport);
        _menus.Add(MenuState.Tiled, ShowTiledImport);
        _menus.Add(MenuState.Heightmaps, ShowHeightmaps);
        _menus.Add(MenuState.Splatmaps, ShowSplatmaps);
        _menus.Add(MenuState.Detailmaps, ShowDetailMaps);
        _menus.Add(MenuState.Treemaps, ShowTreemaps);
        _menus.Add(MenuState.Terrains, ShowTerrainSettings);
        _menus.Add(MenuState.Settings, ShowSettings);
        _menus.Add(MenuState.Help, ShowHelp);

        menuTitles = new string[_menus.Count];
        for (int i = 0; i < _menus.Count; i++)
        {
            menuTitles[i] = ((MenuState)i).ToString();
        }

        _queuedCalls = new Queue<ImporterTask>();
    }

    void OnInspectorUpdate()
    {
        Repaint();

        LandmassEditorUtilities utils = LandmassEditorUtilities.Instance;
        
        while (_queuedCalls.Count > 0)
            _queuedCalls.Dequeue()();

        if (_doTiledImport) {
            utils.ImportFolderIntoScene(_tiledInputLodLevel, _taskProgressToken);
            _doTiledImport = false;
        }
    }

    void OnDestroy()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        if (editorUtilities.ImportCfg.IsDirty)
        {
            if (EditorUtility.DisplayDialog("You have unsaved settings", "Do you want to save?", "Yes", "No"))
            {
                editorUtilities.SaveSettings();
            }
        }
    }

    void OnGUI() {

        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        if (editorUtilities == null)
        {
            GUILayout.Label("No running instance of LandmassImporter found");
            return;
        }

        if (editorUtilities.IsProcessing) {
            ShowProgressBar();
        }

        GUI.enabled = !editorUtilities.IsProcessing;

        EGL.BeginVertical();
        {
            EGL.Separator();

            _currentState = (MenuState)GUILayout.Toolbar((int)_currentState, menuTitles);

            EGL.BeginHorizontal();
            {
                _globalScrollPos = EGL.BeginScrollView(_globalScrollPos, GuiUtils.Skin.box);
                {
                    EGL.BeginVertical();
                    {
                        EGL.Separator();

                        _menus[_currentState]();
                        GUILayout.FlexibleSpace();

                        EGL.Separator();
                    }
                    EGL.EndVertical();
                }
                EGL.EndScrollView();
            }
            EGL.EndHorizontal();

            EGL.Separator();

            ShowSaveButtons();

            GUILayout.Space(16);
        }
        EGL.EndVertical();

        GUI.enabled = true;
    }

    

    // Single import settings
    private Terrain _singleTargetTerrain;
    private string _singleHeightmapPath = "";
    private Texture2D _singleSplatmap;
    private Texture2D _singleTreemap;

    void ShowSingleImport() {
        ImporterConfiguration config = LandmassEditorUtilities.Instance.ImportCfg;

        EGL.BeginVertical();
        {
            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Target Terrain");
                _singleTargetTerrain = EGL.ObjectField(_singleTargetTerrain, typeof(Terrain), true, GUILayout.Width(240f)) as Terrain;

            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                /* ----- Heightmap ----- */

                GUILayout.Label("Heightmap");

                GUILayout.BeginHorizontal();
                {
                    EGL.LabelField("Path", _singleHeightmapPath);
                    if (GUILayout.Button("Browse", GUILayout.Width(60f))) {
                        _singleHeightmapPath = EditorUtility.OpenFilePanel("Browse to Heightmap file",
                                                                           _singleHeightmapPath, "r16");
                    }
                }
                GUILayout.EndHorizontal();

                GUI.enabled = _singleHeightmapPath != "";
                {
                    if (GUILayout.Button("Apply")) {
                        LandmasImporter.ParseHeightmapFileToTerrain(
                            _singleHeightmapPath,
                            _singleTargetTerrain.terrainData,
                            config.HeightFormat,
                            config.HeightmapFlipX,
                            config.HeightmapFlipY
                            );
                    }
                }
                GUI.enabled = true;
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                /* ----- Splatmaps ------ */

                GUILayout.Label("Splatmap");
                _singleSplatmap = EGL.ObjectField(_singleSplatmap, typeof(Texture2D), false, GUILayout.Width(100f), GUILayout.Height(100f)) as Texture2D;

                EGL.Separator();

                GUI.enabled = _singleSplatmap != null && _singleTargetTerrain != null;
                {
                    if (GUILayout.Button("Apply"))
                    {
                        var splatmap = new float[_singleSplatmap.width,_singleSplatmap.height, 4];
                        LandmasImporter.TextureToSplatMap(
                            _singleSplatmap,
                            ref splatmap,
                            false,
                            true);

                        LandmasImporter.Instance.NormalizeSplatmap(ref splatmap, config.NormalizationMode);
                        LandmasImporter.Instance.ParseSplatmapToTerrain(splatmap, _singleTargetTerrain.terrainData);
                    }
                }
                GUI.enabled = true;
            }
            EGL.EndVertical();

            EGL.Separator();
              
            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                /* ------ Treemaps ----- */

                GUILayout.Label("Treemap");
                _singleTreemap = EGL.ObjectField(_singleTreemap, typeof(Texture2D), false, GUILayout.Width(100f), GUILayout.Height(100f)) as Texture2D;
                
                GUI.enabled = _singleTreemap != null;
                {
                    if (GUILayout.Button("Apply"))
                    {
                        LandmasImporter.ParseTreemapTexturesToTerrain(_singleTreemap, _singleTargetTerrain.terrainData);
                    }
                }
                GUI.enabled = true;

                EGL.Separator();

                if (GUILayout.Button("Flush Terrain"))
                {
                    Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
                    if (terrain)
                    {
                        Debug.Log("Flushing Terrain");
                        terrain.Flush();
                    }
                }
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    private bool _doTiledImport;
    private int _tiledInputLodLevel;

    void ShowTiledImport() {
        EGL.BeginVertical();
        {
            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Terrain input folder");
                _tiledInputLodLevel = EGL.IntField("Lod Level", _tiledInputLodLevel);

                if (GUILayout.Button("Load into scene")) {
                    _doTiledImport = true;
                }

                EGL.Separator();
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    void ShowSettings()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;

        EGL.BeginVertical();
        {
            EGL.Separator();

            GUILayout.Label("LOD Levels");

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                if (importCfg.LodLevels.Count >= 4)
                    GUI.enabled = false;
                if (GUILayout.Button("Add LOD Level")) {
                    importCfg.LodLevels.Add(new LodLevel());
                    importCfg.IsDirty = true; // Todo: Nasty
                }
                GUI.enabled = true;

                // Show the list of LODS
                EGL.BeginVertical();
                {
                    _lodScrollPos = EGL.BeginScrollView(_lodScrollPos, GUILayout.MinHeight(96f), GUILayout.MaxHeight(Screen.height));
                    {
                        var removeThese = new List<LodLevel>();
                        int i = 0;
                        foreach (LodLevel lod in importCfg.LodLevels) {
                            if (ShowLodLevel(lod, i)) {
                                removeThese.Add(lod);
                                importCfg.IsDirty = true; // Nasty
                            }
                            i++;
                        }
                        foreach (LodLevel lod in removeThese) {
                            importCfg.LodLevels.Remove(lod);
                        }
                    }
                    EGL.EndScrollView();
                }
                EGL.EndVertical();

                EGL.Space();

                GUILayout.Label("Control how many assets are processed in one go.");
                importCfg.BatchLimit = EGL.IntField("Batch limit", importCfg.BatchLimit);
                GUILayout.Label("Larger batches mean faster processing but require\nmore memory. Change this with care, or Unity's\nmemory might run out!");
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    void ShowTerrainSettings()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;
        TerrainConfiguration terrainStreamingConfiguration = importCfg.TerrainConfiguration;

        EGL.BeginVertical();
        {
            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                if (GUILayout.Button("Apply to selected TerrainData assets"))
                    QueueCall(editorUtilities.ApplyDimensionsToSelection);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("In-game terrain LOD settings.");
                terrainStreamingConfiguration.HeightmapPixelError = EGL.Slider("Pixel Error", terrainStreamingConfiguration.HeightmapPixelError, 1f, 200f);
                terrainStreamingConfiguration.BasemapDistance = EGL.FloatField("Basemap Dist.", terrainStreamingConfiguration.BasemapDistance);
                terrainStreamingConfiguration.CastShadows = EGL.Toggle("Cast Shadows", terrainStreamingConfiguration.CastShadows);
                EGL.Separator();
                terrainStreamingConfiguration.DetailObjectDistance = EGL.Slider("Detail Distance", terrainStreamingConfiguration.DetailObjectDistance, 0f, 250f);
                terrainStreamingConfiguration.DetailObjectDensity = EGL.Slider("Detail Density", terrainStreamingConfiguration.DetailObjectDensity, 0f, 1f);
                terrainStreamingConfiguration.TreeDistance = EGL.Slider("Tree Distance", terrainStreamingConfiguration.TreeDistance, 0f, 2000f);
                terrainStreamingConfiguration.TreeBillboardDistance = EGL.Slider("Billboard Start", terrainStreamingConfiguration.TreeBillboardDistance, 50f, 2000f);
                terrainStreamingConfiguration.TreeCrossFadeLength = EGL.Slider("Fade Length", terrainStreamingConfiguration.TreeCrossFadeLength, 0f, 200f);
                terrainStreamingConfiguration.TreeMaximumFullLODCount = EGL.IntSlider("Max Mesh Trees", terrainStreamingConfiguration.TreeMaximumFullLODCount, 0, 500);
                EGL.Separator();

                if (GUILayout.Button("Apply to selected Scene assets"))
                    QueueCall(editorUtilities.ApplyTerrainLODSettingsToSelection);
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    void ShowHeightmaps()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;

        EGL.BeginVertical();
        {
            GUILayout.Label("The settings for processing heightmaps.");

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Postfix for heightmap files.");
                importCfg.HeightmapTag = EGL.TextField("Name postfix", importCfg.HeightmapTag);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Heightmap file specifications. Please use raw file\nwith x^2+1 dimensions.");
                importCfg.HeightmapExtention = EGL.TextField("File extention", importCfg.HeightmapExtention);
                importCfg.HeightmapFlipX = EGL.Toggle("Mirror X", importCfg.HeightmapFlipX);
                importCfg.HeightmapFlipY = EGL.Toggle("Mirror Y", importCfg.HeightmapFlipY);
                importCfg.HeightFormat = (HeightfileFormat)EditorGUILayout.EnumPopup("Byte Format", importCfg.HeightFormat);
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    void ShowSplatmaps()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;

        EGL.BeginVertical();
        {
            GUILayout.Label("The settings for processing splatmaps.");

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Postfix for splatmap files.");
                importCfg.SplatmapTag = EGL.TextField("Name postfix", importCfg.SplatmapTag);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Splatmap file specifications. Please use x^2.");
                importCfg.SplatmapExtention = EGL.TextField("File extention", importCfg.SplatmapExtention);
                importCfg.SplatmapFlipX = EGL.Toggle("Mirror X", importCfg.SplatmapFlipX);
                importCfg.SplatmapFlipY = EGL.Toggle("Mirror Y", importCfg.SplatmapFlipY);
                importCfg.TrimEmptyChannels = EGL.Toggle("Trim empty", importCfg.TrimEmptyChannels);
                importCfg.NormalizationMode = (NormalizationMode)EditorGUILayout.EnumPopup("Normalize mode", importCfg.NormalizationMode);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("The textures to assign. (Current count: " + editorUtilities.SplatPrototypes.Count + ")");

                EGL.Separator();

                EGL.BeginHorizontal();
                {
                    if (editorUtilities.SplatPrototypes.Count >= 8)
                        GUI.enabled = false;
                    if (GUILayout.Button("Add Prototype"))
                    {
                        editorUtilities.SplatPrototypes.Add(new SplatPrototype());
                        importCfg.IsDirty = true; // Todo: Nasty
                    }
                    GUI.enabled = true;

                    if (GUILayout.Button("Grab from Selected Terrain Object or Asset"))
                    {
                        GetPrototypesFromSelectedTerrain();
                        importCfg.IsDirty = true; // Todo: Nasty
                    }
                }
                EGL.EndHorizontal();

                // Show the list
                EGL.BeginVertical();
                {
                    _prototypeScrollPos = EGL.BeginScrollView(_prototypeScrollPos, GUILayout.MinHeight(96f), GUILayout.MaxHeight(Screen.height));
                    {
                        List<SplatPrototype> removeThese = new List<SplatPrototype>();
                        int i = 0;
                        foreach (SplatPrototype splatPrototype in editorUtilities.SplatPrototypes)
                        {
                            if (ShowSplatPrototype(splatPrototype, i))
                            {
                                removeThese.Add(splatPrototype);
                                importCfg.IsDirty = true; // Nasty
                            }
                            i++;
                        }
                        foreach (SplatPrototype splatPrototype in removeThese)
                        {
                            editorUtilities.SplatPrototypes.Remove(splatPrototype);
                        }
                    }
                    EGL.EndScrollView();
                }
                EGL.EndVertical();
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    void ShowDetailMaps() {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;

        EGL.BeginVertical();
        {
            GUILayout.Label("The settings for processing detail maps.");

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Postfix for detailmap files.");
                importCfg.SplatmapTag = EGL.TextField("Name postfix", importCfg.SplatmapTag);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Splatmap file specifications. Please use x^2.");
                importCfg.SplatmapExtention = EGL.TextField("File extention", importCfg.SplatmapExtention);
                importCfg.SplatmapFlipX = EGL.Toggle("Mirror X", importCfg.SplatmapFlipX);
                importCfg.SplatmapFlipY = EGL.Toggle("Mirror Y", importCfg.SplatmapFlipY);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("The textures to assign. (Current count: " + editorUtilities.DetailPrototypes.Count + ")");

                EGL.Separator();

                EGL.BeginHorizontal();
                {
                    if (editorUtilities.DetailPrototypes.Count >= 8)
                        GUI.enabled = false;
                    if (GUILayout.Button("Add Prototype")) {
                        editorUtilities.DetailPrototypes.Add(new DetailPrototype());
                        importCfg.IsDirty = true; // Todo: Nasty, because the above prototypes still need to be converted to a serializable format, which is not directly done here
                    }
                    GUI.enabled = true;
                }
                EGL.EndHorizontal();

                // Show the list
                EGL.BeginVertical();
                {
                    _prototypeScrollPos = EGL.BeginScrollView(_prototypeScrollPos, GUILayout.MinHeight(96f), GUILayout.MaxHeight(Screen.height));
                    {
                        List<DetailPrototype> removeThese = new List<DetailPrototype>();
                        int i = 0;
                        foreach (DetailPrototype prototype in editorUtilities.DetailPrototypes) {
                            if (ShowDetailPrototype(prototype, i)) {
                                removeThese.Add(prototype);
                                importCfg.IsDirty = true; // Nasty
                            }
                            i++;
                        }
                        foreach (DetailPrototype prototype in removeThese) {
                            editorUtilities.DetailPrototypes.Remove(prototype);
                        }
                    }
                    EGL.EndScrollView();
                }
                EGL.EndVertical();
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    void ShowTreemaps() {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;

        EGL.BeginVertical();
        {
            GUILayout.Label("The settings for processing tree maps.");

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Postfix for treemap files.");
                importCfg.TreemapTag = EGL.TextField("Name postfix", importCfg.TreemapTag);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("Treemap file specifications. Please use x^2.");
                importCfg.SplatmapExtention = EGL.TextField("File extention", importCfg.SplatmapExtention);
            }
            EGL.EndVertical();

            EGL.Separator();

            EGL.BeginVertical(GuiUtils.Skin.box);
            {
                GUILayout.Label("The prototypes to assign. (Current count: " + editorUtilities.TreePrototypes.Count + ")");

                EGL.Separator();

                EGL.BeginHorizontal();
                {
                    if (editorUtilities.TreePrototypes.Count >= 8) {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button("Add Prototype")) {
                        editorUtilities.TreePrototypes.Add(new TreePrototype());
                        importCfg.IsDirty = true; // Todo: Nasty
                    }
                    GUI.enabled = true;
                }
                EGL.EndHorizontal();

                // Show the list
                EGL.BeginVertical();
                {
                    _prototypeScrollPos = EGL.BeginScrollView(_prototypeScrollPos, GUILayout.MinHeight(96f), GUILayout.MaxHeight(Screen.height));
                    {
                        List<TreePrototype> removeThese = new List<TreePrototype>();
                        int i = 0;
                        foreach (TreePrototype prototype in editorUtilities.TreePrototypes) {
                            if (ShowTreePrototype(prototype, i)) {
                                removeThese.Add(prototype);
                                importCfg.IsDirty = true; // Nasty
                            }
                            i++;
                        }
                        foreach (TreePrototype prototype in removeThese) {
                            editorUtilities.TreePrototypes.Remove(prototype);
                        }
                    }
                    EGL.EndScrollView();
                }
                EGL.EndVertical();
            }
            EGL.EndVertical();
        }
        EGL.EndVertical();
    }

    bool ShowLodLevel(LodLevel lod, int index) {
        bool removeThis = false;

        EGL.BeginVertical(GuiUtils.Skin.box);
        {
            GUILayout.Label("[" + index + "]");

            lod.Level = EGL.IntField("Lod Level", lod.Level);
            GUILayout.BeginHorizontal();
            {
                lod.FolderPath = EGL.TextField("Folder Path", lod.FolderPath);
                if (GUILayout.Button("Browse", GUILayout.Width(50f))) {
                    lod.FolderPath = UPath.GetProjectPath(EditorUtility.OpenFolderPanel("Browse", lod.FolderPath, Application.dataPath));
                }
            }
            GUILayout.EndHorizontal();

            lod.GridSize = EGL.IntField("Grid Size", lod.GridSize);

            lod.HeightmapResolution = EGL.IntField("Heightmap Resolution (px)", lod.HeightmapResolution);
            lod.SplatmapResolution = EGL.IntField("Splatmap Resolution (px)", lod.SplatmapResolution);

            lod.HasDetailMap = EGL.Toggle("Use Detail Map?", lod.HasDetailMap);
            lod.HasTreeMap = EGL.Toggle("Use Tree Map?", lod.HasTreeMap);

            if (lod.HasDetailMap) {
                lod.DetailmapResolution = EGL.IntField("Detailmap Resolution (px)", lod.DetailmapResolution);
                lod.DetailResolutionPerPatch = EGL.IntField("Detailmap Patch Size (px)", lod.DetailResolutionPerPatch);
            }

            EGL.Space();

            GUILayout.Label("In-game terrain dimensions.");
            lod.TerrainWidth = EGL.FloatField("Width & Length (m)", lod.TerrainWidth);
            lod.TerrainHeight = EGL.FloatField("Height (m)", lod.TerrainHeight);

            EGL.Space();

            GUILayout.Label("Relief Terrain Configuration");
            lod.ColormapResolution = EGL.IntField("Colormap Resolution", lod.ColormapResolution);
            lod.NormalmapResolution = EGL.IntField("Normalmap Resolution", lod.NormalmapResolution);


            EGL.Space();

            if (GUILayout.Button("Remove", GUILayout.Width(64f), GUILayout.Height(64f)))
                removeThis = true;
        }
        EGL.EndVertical();

        return removeThis;
    }

    /* TODO: Show the below struct editors using standard inspector drawing tools */

    bool ShowSplatPrototype(SplatPrototype splatPrototype, int id)
    {
        bool removeThis = false;

        EGL.BeginVertical(GuiUtils.Skin.box);
        {
            GUILayout.Label(id.ToString() + ". " + (splatPrototype.texture != null ? splatPrototype.texture.name : ""));

            EGL.BeginHorizontal();
            {
                splatPrototype.texture = EGL.ObjectField(splatPrototype.texture, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f)) as Texture2D;

                EGL.BeginVertical();
                {
                    splatPrototype.tileOffset = EGL.Vector2Field("Tile Offset", splatPrototype.tileOffset);
                    splatPrototype.tileSize = EGL.Vector2Field("Tile Size", splatPrototype.tileSize);
                }
                EGL.EndVertical();

                if (GUILayout.Button("Remove", GUILayout.Width(64f), GUILayout.Height(64f)))
                    removeThis = true;
            }
            EGL.EndHorizontal();
        }
        EGL.EndVertical();

        return removeThis;
    }

    bool ShowDetailPrototype(DetailPrototype prototype, int id) {
        bool removeThis = false;

        EGL.BeginVertical(GuiUtils.Skin.box);
        {
            EGL.BeginHorizontal();
            {
                EGL.BeginVertical();
                {
                    prototype.usePrototypeMesh = EGL.Toggle("Use Mesh", prototype.usePrototypeMesh);
                    prototype.prototype = EGL.ObjectField(prototype.prototype, typeof (GameObject), false) as GameObject;
                    prototype.prototypeTexture = EGL.ObjectField(prototype.prototypeTexture, typeof (Texture2D), false, GUILayout.Width(64f),
                                        GUILayout.Height(64f)) as Texture2D;
                }
                EGL.EndVertical();

                EGL.BeginVertical();
                {
                    prototype.bendFactor = EGL.FloatField("Bend Factor", prototype.bendFactor);
                    prototype.dryColor = EGL.ColorField("Dry Color", prototype.dryColor);
                    prototype.healthyColor = EGL.ColorField("Healthy Color", prototype.healthyColor);
                    prototype.maxHeight = EGL.FloatField("Max Height", prototype.maxHeight);
                    prototype.minHeight = EGL.FloatField("Min Height", prototype.minHeight);
                    prototype.maxWidth = EGL.FloatField("Max Width", prototype.maxWidth);
                    prototype.minWidth = EGL.FloatField("Min Width", prototype.minWidth);
                    prototype.noiseSpread = EGL.FloatField("Noise Spread", prototype.noiseSpread);
                    prototype.renderMode = (DetailRenderMode)EGL.EnumPopup("Noise Spread", prototype.renderMode);
                }
                EGL.EndVertical();

                if (GUILayout.Button("Remove", GUILayout.Width(64f), GUILayout.Height(64f)))
                    removeThis = true;
            }
            EGL.EndHorizontal();
        }
        EGL.EndVertical();

        return removeThis;
    }

    bool ShowTreePrototype(TreePrototype treePrototype, int id) {
        bool removeThis = false;

        EGL.BeginVertical(GuiUtils.Skin.box);
        {
            GUILayout.Label(id.ToString() + ". " + (treePrototype.prefab != null ? treePrototype.prefab.name : ""));

            EGL.BeginHorizontal();
            {
                treePrototype.prefab = EGL.ObjectField(treePrototype.prefab, typeof(GameObject), false) as GameObject;

                EGL.BeginVertical();
                {
                    treePrototype.bendFactor = EGL.FloatField("Bend Factor", treePrototype.bendFactor);
                }
                EGL.EndVertical();

                if (GUILayout.Button("Remove", GUILayout.Width(64f), GUILayout.Height(64f)))
                    removeThis = true;
            }
            EGL.EndHorizontal();
        }
        EGL.EndVertical();

        return removeThis;
    }

    void ShowSaveButtons()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;
        ImporterConfiguration importCfg = editorUtilities.ImportCfg;

        EGL.BeginVertical();
        {
            EGL.BeginHorizontal();
            {
                EGL.Separator();

                if (GUILayout.Button("Save Settings"))
                {
                    editorUtilities.SaveSettings();
                }
                if (GUILayout.Button("Load Settings"))
                {
                    editorUtilities.LoadSettings();
                }
                GUI.enabled = true;

                EGL.Separator();
            }
            EGL.EndHorizontal();

            EGL.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (importCfg.IsDirty)
                    GUILayout.Label("You have unsaved settings.");
                GUILayout.FlexibleSpace();
            }
            EGL.EndHorizontal();
        }
        EGL.EndVertical();
    }

    void ShowHelp()
    {
        EGL.BeginVertical();
        {
            GUILayout.Label("Landmass version: " + version); // Todo: Version number from xml?
            EGL.Separator();
            GUILayout.Label("Made by Martijn Zandvliet");
            EGL.Separator();
            if (GUILayout.Button("Show Documentation"))
                Application.OpenURL(readmeUrl);
        }
        EGL.EndVertical();
    }

    private readonly TaskProgressToken _taskProgressToken = new TaskProgressToken();

    void ShowProgressBar() {
        // Display progress bar with cancel button
        bool cancel = EditorUtility.DisplayCancelableProgressBar(_taskProgressToken.Title,
                                                                 _taskProgressToken.Info,
                                                                 _taskProgressToken.Progress);
        if (cancel) {
            _taskProgressToken.Cancel = true;
        }
    }

    // Todo: Or get them from a selected terrain in the scene.
    void GetPrototypesFromSelectedTerrain()
    {
        LandmassEditorUtilities editorUtilities = LandmassEditorUtilities.Instance;

        TerrainData terrainData = null;

        // See if the selected object is a Terrain object. If so, get its terrainData.
        GameObject terrainObject = Selection.activeGameObject;

        if (terrainObject != null)
        {
            Terrain terrain = terrainObject.GetComponent<Terrain>();

            if (terrain != null)
                terrainData = terrain.terrainData;
        }
        else
        {
            // See if the selected object is a TerrainData asset	
            terrainData = Selection.activeObject as TerrainData;
        }

        if (terrainData != null)
        {
            editorUtilities.SplatPrototypes.Clear();
            foreach (SplatPrototype prototype in terrainData.splatPrototypes) {
                editorUtilities.SplatPrototypes.Add(prototype);
            }
        }
    }

    void QueueCall(ImporterTask task)
    {
        _queuedCalls.Enqueue(task);
    }
}

namespace RamjetAnvil.Unity.Landmass {
    delegate void ImporterTask();

    public class TaskProgressToken {
        public string Title;
        public string Info;
        public float Progress;
        public bool Cancel;
    }
}

