using System;
using System.Xml.Serialization;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Unity.Landmass
{
    /* Todo:
     * - Split this into an object with state (config, progress report (used by gui), etc.), and static methods (filter folder, etc)
     * - Trigger import call outside of gui thread
     * - Use multithreading
     */
    public class LandmassEditorUtilities
    {
        #region Fields

        private const string ImportCfgPath = "Assets/Config/RamjetAnvil/Landmass/LandmassImporterSettings.xml";
        private static LandmassEditorUtilities _instance;

        #endregion

        #region Properties

        public static LandmassEditorUtilities Instance {
            get { return _instance ?? (_instance = new LandmassEditorUtilities()); }
        }

        public ImporterConfiguration ImportCfg { get; private set; }
        public bool IsProcessing { get; private set; }

        /* TODO:
         * 
         * This is a bit confusing. I currently have these here because of an issue
         * with serialization (converting instance reference to asset path).
         * 
         * Can probably make this simpler. Certainly, housing these lists here is strange.
         */
        public IList<SplatPrototype> SplatPrototypes { get; private set; }
        public IList<DetailPrototype> DetailPrototypes { get; private set; }
        public IList<TreePrototype> TreePrototypes { get; private set; }

        #endregion

        #region Methods

        private LandmassEditorUtilities() {
            SplatPrototypes = new List<SplatPrototype>();
            DetailPrototypes = new List<DetailPrototype>();
            TreePrototypes = new List<TreePrototype>();

            LoadSettings();
        }

        /// <summary>
        /// Static import of terrain files into project/scene. No streaming.
        /// </summary>
        /// <param name="folderPath"></param>
        public void ImportFolderIntoScene(int lodLevel, TaskProgressToken token) {
            /* Todo: 
             * - Unify more code with the streaming system (i.e. this terrain is streamed once at startup, but same code)
             * - Allow iterative work flow (i.e. just reimporting splatmaps, not everything)
             * - support running batches, parallel processing
             */

            Debug.Log("Importing lod level: " + lodLevel);

            try {
                var lod = ImportCfg.LodLevels[lodLevel];

                List<string> heightmapFiles = FilterFolder(lod.FolderPath, ImportCfg.HeightmapTag, ImportCfg.HeightmapExtention);

                var terrainDatas = CreateTerrainDatas(lod.FolderPath, heightmapFiles, lod);
                if (heightmapFiles == null || heightmapFiles.Count == 0) {
                    throw new ArgumentException(string.Format("No heightmaps found in folder after filtering: " + lod.FolderPath));
                }
                ProcessHeightmapAssets(heightmapFiles, terrainDatas, lod, token);

                List<string> splatmapFiles = FilterFolder(lod.FolderPath, ImportCfg.SplatmapTag, ImportCfg.SplatmapExtention);
                if (splatmapFiles == null || splatmapFiles.Count != heightmapFiles.Count) {
                    throw new ArgumentException(string.Format("The number of splatmaps {0} does not match the number of heightmaps {1}.", splatmapFiles.Count, heightmapFiles.Count));
                }
                ProcessSplatmapAssets(splatmapFiles, terrainDatas, token);

                if (lod.HasDetailMap) {
                    List<string> detailmapFiles = FilterFolder(lod.FolderPath, ImportCfg.DetailmapTag, ImportCfg.SplatmapExtention);
                    if (detailmapFiles == null || detailmapFiles.Count != heightmapFiles.Count) {
                        throw new ArgumentException( string.Format("The number of detailmaps {0} does not match the number of heightmaps {1}.", detailmapFiles.Count, heightmapFiles.Count));
                    }
                    ProcessDetailmapAssets(detailmapFiles, terrainDatas, token);
                }

                if (lod.HasTreeMap) {
                    List<string> treemapFiles = FilterFolder(lod.FolderPath, ImportCfg.TreemapTag, ImportCfg.SplatmapExtention);
                    if (treemapFiles == null || treemapFiles.Count != heightmapFiles.Count) {
                        throw new ArgumentException( string.Format("The number of treemaps {0} does not match the number of heightmaps {1}.", treemapFiles.Count, heightmapFiles.Count));
                    }
                    ProcessTreemapAssets(treemapFiles, terrainDatas, token);
                }

                var terrains = InstantiateTerrainInScene(heightmapFiles, terrainDatas, lod);

                SetupLandmassUnityTerrain(terrains, lod, CustomShaderId, MaterialPath);
            }
            catch (Exception e) {
                Debug.LogError("Failed to import terrains into scene.");
                Debug.LogException(e);
            }

            IsProcessing = false;
        }

        private const string DefaultShaderId = "Nature/Terrain/Standard";
        private const string CustomShaderId = "Nature/Terrain/StandardCustom";
        private const string MaterialPath = "Assets/Materials/Terrain/";

        [MenuItem("Window/Landmass/RemoveTreeColliders")]
        public static void RemoveTreeColliders() {
            var trees = GameObject.FindGameObjectsWithTag("Tree");

            for (int i = 0; i < trees.Length; i++) {
                Object.DestroyImmediate(trees[i]);
            }
        }

        [MenuItem("Window/Landmass/Setup Default Materials")]
        public static void SetupMaterialsDefault() {
            var tiles = LandmasImporter.FindTerrainTilesInScene();

            foreach (var lodLevel in tiles) {
                SetupLandmassUnityTerrain(lodLevel.Value, Instance.ImportCfg.LodLevels[lodLevel.Key], DefaultShaderId, MaterialPath);
            }
        }

        [MenuItem("Window/Landmass/Setup Landmass Materials")]
        public static void SetupMaterials() {
            if (!Directory.Exists(UPath.GetAbsolutePath(MaterialPath))) {
                Directory.CreateDirectory(MaterialPath);
            }

            var tiles = LandmasImporter.FindTerrainTilesInScene();

            foreach (var lodLevel in tiles) {
                SetupLandmassUnityTerrain(lodLevel.Value, Instance.ImportCfg.LodLevels[lodLevel.Key], CustomShaderId, MaterialPath);
            }
        }

        private static SplatPrototype[] CopiedSplatPrototypes;

        [MenuItem("Window/Landmass/Copy Splats")]
        public static void CopySplatNormals() {
            CopiedSplatPrototypes = Selection.activeGameObject.GetComponent<Terrain>().terrainData.splatPrototypes;
        }

        [MenuItem("Window/Landmass/Paste Splats")]
        public static void PasteSplatNormals() {
            var tiles = LandmasImporter.FindTerrainTilesInScene();

            foreach (var lodLevel in tiles) {
                foreach (var tile in lodLevel.Value) {
                    var terrainData = tile.Value.Terrain.terrainData;
                    terrainData.splatPrototypes = CopiedSplatPrototypes;
                }
            }
        }

        private static TreePrototype[] CopiedTreePrototypes;

        [MenuItem("Window/Landmass/Copy Trees")]
        public static void CopyTrees() {
            CopiedTreePrototypes = Selection.activeGameObject.GetComponent<Terrain>().terrainData.treePrototypes;
        }

        [MenuItem("Window/Landmass/Paste Trees")]
        public static void PasteTrees() {
            var tiles = LandmasImporter.FindTerrainTilesInScene();

            foreach (var lodLevel in tiles) {
                foreach (var tile in lodLevel.Value) {
                    var terrainData = tile.Value.Terrain.terrainData;
                    terrainData.treePrototypes = CopiedTreePrototypes;
                }
            }
        }

        private static DetailPrototype[] CopiedDetailPrototypes;

        [MenuItem("Window/Landmass/Copy Details")]
        public static void CopyDetails() {
            CopiedDetailPrototypes = Selection.activeGameObject.GetComponent<Terrain>().terrainData.detailPrototypes;
        }

        [MenuItem("Window/Landmass/Paste Details")]
        public static void PasteDetails() {
            var tiles = LandmasImporter.FindTerrainTilesInScene();

            foreach (var lodLevel in tiles) {
                foreach (var tile in lodLevel.Value) {
                    var terrainData = tile.Value.Terrain.terrainData;
                    terrainData.detailPrototypes = CopiedDetailPrototypes;
                }
            }
        }

        public void ProcessHeightmapAssets(IList<string> assets, IList<TerrainData> terrains, LodLevel lod, TaskProgressToken token)
        {
            if (IsProcessing)
                return;

            IsProcessing = true;

            token.Title = "Processing heightmaps...";

            // Todo: split out terrain data creation/retrieval, call it here to get List<TerrainData>, or something

            for (int i = 0; i < assets.Count; i++) {
                token.Progress = (float) (i + 1)/assets.Count;
                
                if (token.Cancel) {
                    break;
                }

                string heightmapPath = assets[i];

                TerrainData terrainData = terrains[i];
                var heightData = new float[lod.HeightmapResolution, lod.HeightmapResolution];
                LandmasImporter.LoadRawFileAsFloats(
                    heightmapPath,
                    ref heightData,
                    ImportCfg.HeightFormat,
                    false,
                    true);

                // For 8-bit PNGs
                //var heightTexture = LandmasImporter.LoadTexture(heightmapPath, ImportCfg.HeightmapResolution);
                //LandmasImporter.TextureToHeightMap(heightTexture, ref heightData, false, true);

                terrainData.SetHeights(0, 0, heightData);

                // Save assets to disk and flush them from memory
                if (i%ImportCfg.BatchLimit == 0) {
                    FlushAssetsToDisk();
                }
            }

            FlushAssetsToDisk();

            IsProcessing = false;
        }

        public void ProcessSplatmapAssets(IList<string> assets, IList<TerrainData> terrainDatas, TaskProgressToken token) {
            LandmasImporter.ValidateSplatPrototypes(SplatPrototypes);
                
            IsProcessing = true;

            for (int i = 0; i < assets.Count; i++) {
                // Display progress bar with cancel button
                token.Progress = (i + 1) / (float)assets.Count;
                
                if (token.Cancel) {
                    break;
                }

                string asset = assets[i];
                TerrainData terrainData = terrainDatas[i];

                LandmasImporter.Instance.ApplySplatPrototypes(terrainData, SplatPrototypes);

                Texture2D splatmapTexture = AssetDatabase.LoadAssetAtPath(asset, typeof(Texture2D)) as Texture2D;
                var splatmap = new float[terrainData.alphamapResolution,terrainData.alphamapResolution,4];
                LandmasImporter.TextureToSplatMap(splatmapTexture, ref splatmap, false, false);
                LandmasImporter.Instance.ParseSplatmapToTerrain(splatmap, terrainData);

                if (i%ImportCfg.BatchLimit == 0) {
                    FlushAssetsToDisk();
                }
            }

            FlushAssetsToDisk();

            IsProcessing = false;
        }

        // Todo: process detail maps (grass and such)
        public void ProcessDetailmapAssets(IList<string> assets, IList<TerrainData> terrainDatas, TaskProgressToken token) {
            IsProcessing = true;

            for (int i = 0; i < assets.Count; i++) {
                // Display progress bar with cancel button
                token.Progress = (i + 1) / (float)assets.Count;

                if (token.Cancel) {
                    break;
                }

                string asset = assets[i];
                TerrainData terrainData = terrainDatas[i];

                LandmasImporter.Instance.ApplyDetailPrototypes(terrainData, DetailPrototypes);

                Texture2D texture = AssetDatabase.LoadAssetAtPath(asset, typeof(Texture2D)) as Texture2D;
                int[][,] map = new int[DetailPrototypes.Count][,];
                for (int j = 0; j < map.Length; j++) {
                    map[j] = new int[texture.width,texture.height];
                }
                LandmasImporter.TextureToDetailMap(texture, ref map, false, false);
                LandmasImporter.Instance.ParseDetailmapToTerrain(map, terrainData);

                if (i % ImportCfg.BatchLimit == 0) {
                    FlushAssetsToDisk();
                }
            }

            FlushAssetsToDisk();

            IsProcessing = false;
        }

        public void ProcessTreemapAssets(IList<string> assets, IList<TerrainData> terrainDatas, TaskProgressToken token) {
            IsProcessing = true;

            for (int i = 0; i < assets.Count; i++) {
                // Display progress bar with cancel button
                token.Progress = (i + 1) / (float)assets.Count;

                if (token.Cancel) {
                    break;
                }

                string asset = assets[i];
                TerrainData terrainData = terrainDatas[i];

                LandmasImporter.Instance.ApplyTreePrototypes(terrainData, TreePrototypes);

                Texture2D treemapTexture = AssetDatabase.LoadAssetAtPath(asset, typeof(Texture2D)) as Texture2D;
                LandmasImporter.ParseTreemapTexturesToTerrain(treemapTexture, terrainData);

                if (i % ImportCfg.BatchLimit == 0) {
                    FlushAssetsToDisk();
                }
            }

            FlushAssetsToDisk();

            IsProcessing = false;
        }

        public void ApplySplatPrototypesToSelection()
        {
            IsProcessing = true;

            List<string> assets = FilterSelection<TerrainData>();
            if (assets == null)
                return;

            for (int i = 0; i < assets.Count; i++)
            {
                float progress = (float) (i + 1)/(float) assets.Count;
                bool cancel = EditorUtility.DisplayCancelableProgressBar("Applying SplatPrototypes",
                                                                         "Parsing: " + Path.GetFileName(assets[i]),
                                                                         progress);
                if (cancel)
                {
                    EditorUtility.DisplayCancelableProgressBar("Applying SplatPrototypes",
                                                               "Please wait, flushing assets to disk...", progress);
                    break;
                }

                TerrainData terrainData = AssetDatabase.LoadAssetAtPath(assets[i], typeof (TerrainData)) as TerrainData;
                if (terrainData == null)
                {
                    Debug.LogWarning(assets[i] + " could not be loaded");
                    continue;
                }

                LandmasImporter.Instance.ApplySplatPrototypes(terrainData, SplatPrototypes);

                // Save assets to flush them from memory
                if (i%ImportCfg.BatchLimit >= ImportCfg.BatchLimit - 1)
                {
                    EditorUtility.DisplayCancelableProgressBar("Applying SplatPrototypes",
                                                               "Please wait, flushing assets to disk...", progress);
                    FlushAssetsToDisk();
                }
            }

            FlushAssetsToDisk();
            EditorUtility.ClearProgressBar();

            IsProcessing = false;
        }

        public IDictionary<IntVector3, TerrainTile> InstantiateTerrainInScene(IList<string> assets, IList<TerrainData> terrainDatas, LodLevel lod) {
            var terrains = new Dictionary<IntVector3, TerrainTile>();

            var existingTerrains = Object.FindObjectsOfType<TerrainTile>();
            for (int i = 0; i < existingTerrains.Length; i++) {
                var tile = existingTerrains[i];
                if (tile.LodLevel == lod.Level) {
                    Object.DestroyImmediate(tile.gameObject);
                }
            }

            float halfGridWidth = (float)lod.GridSize * lod.TerrainWidth * 0.5f;
            Vector3 offset = new Vector3(-halfGridWidth, 0f, -halfGridWidth);

            for (int i = 0; i < terrainDatas.Count; i++) {
                string asset = assets[i];

                string name = GetTerrainNameFromAsset(asset);
                var region = LandmasImporter.GetTerrainRegionFromName(name, lod.GridSize);
                Vector3 position = offset + LandmasImporter.GetTerrainPosition(region, lod.TerrainWidth);
                
                TerrainData terrainData = terrainDatas[i];
                Terrain terrain = new GameObject(name).AddComponent<Terrain>();
                terrain.terrainData = terrainData;
                if (lod.Level < 1) {
                    terrain.gameObject.AddComponent<TerrainCollider>().terrainData = terrainData;
                }
                terrain.gameObject.isStatic = true;
                terrain.gameObject.name = name + "_lod" + lod.Level;
                terrain.transform.position = position;

                LandmasImporter.ApplyTerrainQualitySettings(terrain, ImportCfg.TerrainConfiguration);

                terrain.Flush();

                var tile = terrain.gameObject.AddComponent<TerrainTile>();
                tile.Region = region;
                tile.LodLevel = lod.Level;
                tile.Terrain = terrain;
                
                terrains.Add(region, tile);
            }

            return terrains;
        }

        public static void SetupLandmassUnityTerrain(IDictionary<IntVector3, TerrainTile> terrains, LodLevel lod, string shaderId, string folderPath) {
//            var matName = "terrain_lod" + lod.Level + "_material";
//            var matPath = UPath.Combine(folderPath, matName + ".mat");
//
//            Debug.Log(matPath);
//
//            var material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
//
//            if (!material) {
//                material = new Material(Shader.Find(shaderId));
//                AssetDatabase.CreateAsset(material, matPath);
//            }
//
//            string colorMapName = "terrain_c_atlas.png";
//            string normalMapName = "terrain_n_atlas.png";
//
//            var colorMap = AssetDatabase.LoadAssetAtPath(UPath.Combine(lod.FolderPath, colorMapName), typeof(Texture2D)) as Texture2D;
//            var normalMap = AssetDatabase.LoadAssetAtPath(UPath.Combine(lod.FolderPath, normalMapName), typeof(Texture2D)) as Texture2D;
        }

        [MenuItem("Window/Landmass/Apply Treemaps")]
        public static void ApplyTreemaps() {
            var tiles = LandmasImporter.FindTerrainTilesInScene();

            foreach (var lodLevel in tiles) {
                var lod = Instance.ImportCfg.LodLevels[lodLevel.Key];
                
                foreach (var pair in lodLevel.Value) {
                    var region = pair.Key;

                    int index = (int)(region.X + (lod.GridSize - 1 - region.Z) * lod.GridSize);
                    string treemapName = string.Format("terrain_t_{0:00}.png", index);
                    var treeMap = AssetDatabase.LoadAssetAtPath(UPath.Combine(lod.FolderPath, treemapName), typeof(Texture2D)) as Texture2D;
                    if (treeMap == null) {
                        Debug.LogError("Couldn't find treemap: " + treemapName);
                        continue;
                    }

                    LandmasImporter.ParseTreemapTexturesToTerrain(treeMap, pair.Value.Terrain.terrainData);
                }
            }
        }

//        [MenuItem("Window/Landmass/Create Global Normal Maps")]
//        public static void CreateGlobalNormalMaps() {
//            var tiles = LandmasImporter.FindTerrainTilesInScene();
//
//            const int res = 128;
//
//            Color[] normals = new Color[res * res];
//
//            foreach (var lodLevel in tiles) {
//                var lod = Instance.ImportCfg.LodLevels[lodLevel.Key];
//
//                foreach (var pair in lodLevel.Value) {
//                    var region = pair.Key;
//
//                    TerrainData data = pair.Value.Terrain.terrainData;
//
//                    int index = (int)(region.X + (lod.GridSize - 1 - region.Z) * lod.GridSize);
//                    string texPath =  UPath.Combine(lod.FolderPath, string.Format("terrain_n_{0:00}.png", index));
//                    Texture2D tex = null;
//                    if (File.Exists(texPath)) {
//                        //map = AssetDatabase.LoadAssetAtPath(mapPath, typeof (Texture2D)) as Texture2D;
//                        File.Delete(texPath);
//                    }
//                    else {
//                        tex = new Texture2D(res, res, TextureFormat.ARGB32, true, true);
//                    }
//
//                    if (tex == null) {
//                        Debug.LogError("Couldn't find map: " + texPath);
//                        continue;
//                    }
//
//                    LandmasImporter.MakeNormalMap(data, ref normals, res);
//                    tex.SetPixels(normals);
//                    tex.Apply(true);
//
//                    var pngBytes = tex.EncodeToPNG();
//                    File.WriteAllBytes(texPath, pngBytes);
//
//                    pair.Value.Terrain.materialTemplate.SetTexture("_GlobalNormalTex", tex);
//                }
//            }
//
//            AssetDatabase.Refresh(ImportAssetOptions.Default);
//
//            Debug.Log("Done");
//        }

        public void ApplyDimensionsToSelection()
        {
            //IsProcessing = true;

            //List<string> assets = FilterSelection<TerrainData>();
            //for (int i = 0; i < assets.Count; i++)
            //{
            //    float progress = (float) (i + 1)/(float) assets.Count;
            //    bool cancel = EditorUtility.DisplayCancelableProgressBar("Applying Terrain Dimensions",
            //                                                             "Parsing: " + Path.GetFileName(assets[i]),
            //                                                             progress);
            //    if (cancel)
            //    {
            //        EditorUtility.DisplayCancelableProgressBar("Applying Terrain Dimensions",
            //                                                   "Please wait, flushing assets to disk...", progress);
            //        break;
            //    }

            //    TerrainData terrainData = AssetDatabase.LoadAssetAtPath(assets[i], typeof (TerrainData)) as TerrainData;
            //    if (terrainData == null)
            //    {
            //        Debug.LogWarning(assets[i] + " could not be loaded");
            //        continue;
            //    }

            //    ApplyDimensions(terrainData);

            //    // Save assets to flush them from memory
            //    if (i%ImportCfg.BatchLimit >= ImportCfg.BatchLimit - 1)
            //    {
            //        EditorUtility.DisplayCancelableProgressBar("Applying Terrain Dimensions",
            //                                                   "Please wait, flushing assets to disk...", progress);
            //        FlushMemory();
            //    }
            //}

            //FlushMemory();
            //EditorUtility.ClearProgressBar();

            //IsProcessing = false;
        }

        public static void ApplyDimensions(TerrainData terrainData, LodLevel lod)
        {
            terrainData.size = new Vector3(lod.TerrainWidth, lod.TerrainHeight, lod.TerrainWidth);
            EditorUtility.SetDirty(terrainData);
        }

        public void ApplyTerrainLODSettingsToSelection()
        {
//            IsProcessing = true;
//
//            List<string> assets = FilterSelection<Object>(null, ImportCfg.SceneExtention);
//            if (assets == null)
//                return;
//
//            for (int i = 0; i < assets.Count; i++)
//            {
//                // Display progress bar with cancel button
//                float progress = (float) (i + 1)/(float) assets.Count;
//                bool cancel = EditorUtility.DisplayCancelableProgressBar("Applying Terrain LOD settings",
//                                                                         "Parsing: " + Path.GetFileName(assets[i]),
//                                                                         progress);
//                if (cancel)
//                    break;
//
//                string scenePath = assets[i];
//                if (!EditorApplication.OpenScene(scenePath))
//                    break;
//
//                // Apply terrain settings
//                Terrain terrain = Object.FindObjectOfType(typeof (Terrain)) as Terrain;
//                if (terrain == null)
//                    break;
//                LandmasImporter.ApplyTerrainQualitySettings(terrain, ImportCfg.TerrainConfiguration);
//
//                // Save scene
//                EditorApplication.SaveScene(scenePath);
//            }
//
//            EditorUtility.ClearProgressBar();
//
//            IsProcessing = false;
        }

        public void FlushAssetsToDisk()
        {
            AssetDatabase.SaveAssets();
            EditorUtility.UnloadUnusedAssetsImmediate();
            AssetDatabase.Refresh();
        }

        public string GetTerrainNameFromAsset(string asset)
        {
            string fileName = Path.GetFileNameWithoutExtension(asset);

            string[] separators = new[]
            {
                ImportCfg.HeightmapTag,
                ImportCfg.SplatmapTag,
                ImportCfg.DetailmapTag,
                ImportCfg.TreemapTag
            };

            string[] parts = fileName.Split(separators, StringSplitOptions.None);
            return parts[0] + parts[parts.Length-1];
        }

        public IList<TerrainData> CreateTerrainDatas(string sourceFolder, IList<string> names, LodLevel lod) {
            string destinationPath = UPath.Combine(sourceFolder, "TerrainData");
            string absPath = UPath.GetAbsolutePath(destinationPath);

            if (!Directory.Exists(absPath)) {
                Directory.CreateDirectory(absPath);
            }

            IList<TerrainData> terrainDatas = new List<TerrainData>();

            for (int i = 0; i < names.Count; i++) {
                // Create structure
                var terrainData = new TerrainData {
                    heightmapResolution = lod.HeightmapResolution + 1,
                    alphamapResolution = lod.SplatmapResolution,
                    size = new Vector3(lod.TerrainWidth, lod.TerrainHeight, lod.TerrainWidth)
                };

                if (lod.HasDetailMap) {
                    terrainData.SetDetailResolution(lod.DetailmapResolution, lod.DetailResolutionPerPatch);
                }

                terrainDatas.Add(terrainData);

                // Serialize it
                string terrainName = GetTerrainNameFromAsset(names[i]);
                string terrainDataPath = UPath.Combine(destinationPath, terrainName + "." + ImportCfg.TerrainDataExtention);

                AssetDatabase.CreateAsset(terrainData, terrainDataPath);
            }

            return terrainDatas;
        }

        private List<string> FilterSelection<T>() where T : Object
        {
            if (Selection.objects.Length > ImportCfg.BatchLimit)
            {
                Debug.Log(string.Format(
                        "Cannot handle selections large than {0} items. Use folder-based processing instead.",
                        ImportCfg.BatchLimit));
                return null;
            }

            EditorUtility.DisplayCancelableProgressBar("Please Wait", "Filtering selection...", 0f);
            // Get a list of all selected assets, including those in folders
            Object[] selection = Selection.GetFiltered(typeof (T), SelectionMode.Assets);
                // | SelectionMode.DeepAssets);

            List<string> assets = new List<string>(selection.Length);
            for (int i = 0; i < selection.Length; i++)
            {
                EditorUtility.DisplayCancelableProgressBar("Please Wait", "Filtering selection...",
                                                           (float) (i + 1)/(float) selection.Length);
                string assetPath = AssetDatabase.GetAssetPath(selection[i]);
                assets.Add(assetPath);
            }

            EditorUtility.ClearProgressBar();
            return assets;
        }

        public List<string> FilterSelection<T>(string tag, string extention) where T : Object
        {
            if (tag == null)
            {
                Debug.LogError("Tag can not be null, use empty string instead");
                return null;
            }

            if (Selection.objects.Length > ImportCfg.BatchLimit)
            {
                Debug.Log(
                    string.Format(
                        "Cannot handle selections large than {0} items. Use folder-based processing instead.",
                        ImportCfg.BatchLimit));
                return null;
            }

            EditorUtility.DisplayCancelableProgressBar("Please Wait", "Filtering selection...", 0f);

            // Get a list of all selected assets, including those in folders
            Object[] selection = Selection.GetFiltered(typeof (T), SelectionMode.Assets);
                // | SelectionMode.DeepAssets);

            List<string> assets = new List<string>(selection.Length);
            for (int i = 0; i < selection.Length; i++)
            {
                EditorUtility.DisplayCancelableProgressBar("Please Wait", "Filtering selection...",
                                                           (float) (i + 1)/(float) selection.Length);
                string assetPath = AssetDatabase.GetAssetPath(selection[i]);
                if ((tag == "" || assetPath.Contains(tag)) && assetPath.EndsWith(extention))
                    assets.Add(assetPath);
            }
            EditorUtility.ClearProgressBar();

            return assets;
        }

        public List<string> FilterFolder(string path, string typeTag, string extention)
        {
            string absPath = UPath.GetAbsolutePath(path);

            if (!Directory.Exists(absPath)) {
                throw new DirectoryNotFoundException("Directory does not exist: " + path);
            }

            string pattern = "*" + typeTag + "*" + extention;
            string[] files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
            List<string> assets = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = UPath.GetProjectPath(files[i]);
                assets.Add(fileName);
            }

            return assets;
        }

        public void SaveSettings()
        {
            // Parse prototypes to a format that can be serialized
            ImportCfg.SplatPrototypes.Clear();
            foreach (SplatPrototype prototype in SplatPrototypes)
                ImportCfg.SplatPrototypes.Add(SplatPrototypeConfiguration.Serialize(prototype));

            ImportCfg.DetailPrototypes.Clear();
            foreach (DetailPrototype prototype in DetailPrototypes)
                ImportCfg.DetailPrototypes.Add(DetailPrototypeConfiguration.Serialize(prototype));

            ImportCfg.TreePrototypes.Clear();
            foreach (TreePrototype prototype in TreePrototypes)
                ImportCfg.TreePrototypes.Add(TreePrototypeConfiguration.Serialize(prototype));

            // Serialize
            string filePath = UPath.GetAbsolutePath(ImportCfgPath);

            XmlSerializer serializer = new XmlSerializer(typeof(ImporterConfiguration));
            TextWriter writer = new StreamWriter(filePath);
            serializer.Serialize(writer, ImportCfg);
            writer.Close();

            ImportCfg.IsDirty = false;
        }

        public void LoadSettings()
        {
            // Deserialize
            string filePath = UPath.GetAbsolutePath(ImportCfgPath);
            if (File.Exists(filePath)) {
                try {
                    TextReader textReader = new StreamReader(filePath);
                    XmlSerializer serializer = new XmlSerializer(typeof (ImporterConfiguration));
                    ImportCfg = serializer.Deserialize(textReader) as ImporterConfiguration;
                    textReader.Close();
                }
                catch (Exception e) {
                    Debug.LogWarning("Import configuration could not be loaded, loading defaults.\n" + e.Message);
                    ImportCfg = new ImporterConfiguration();
                }
            }
            else {
                Debug.LogWarning("No configuration found, loading defaults");
                ImportCfg = new ImporterConfiguration();
            }

            // Parse splatPrototypeSettings to actual splatPrototype objects
            SplatPrototypes.Clear();
            foreach (SplatPrototypeConfiguration settings in ImportCfg.SplatPrototypes) {
                var prototype = SplatPrototypeConfiguration.Deserialize(settings);
                SplatPrototypes.Add(prototype);
            }

            DetailPrototypes.Clear();
            foreach (DetailPrototypeConfiguration settings in ImportCfg.DetailPrototypes) {
                var prototype = DetailPrototypeConfiguration.Deserialize(settings);
                DetailPrototypes.Add(prototype);
            }

            TreePrototypes.Clear();
            foreach (TreePrototypeConfiguration settings in ImportCfg.TreePrototypes) {
                var prototype = TreePrototypeConfiguration.Deserialize(settings);
                TreePrototypes.Add(prototype);
            }

            ImportCfg.IsDirty = false;
        }

        #endregion
    }
}