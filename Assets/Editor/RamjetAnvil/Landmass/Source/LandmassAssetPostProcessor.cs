using System;
using UnityEngine;
using UnityEditor;
using System.IO;

// Todo: Instead of hardcoding these values, store them in the settings and expose them in the GUI

namespace RamjetAnvil.Unity.Landmass
{
//    public class LandmassAssetPostProcessor : AssetPostprocessor
//    {
//        private void OnPreprocessTexture() {
//            string fileName = Path.GetFileName(assetPath);
//
//            if (!assetPath.Contains("Assets/Terrains/")) {
//                return;
//            }
//
//            try {
//                string[] parts = assetPath.Split(new[] {"lod_", "/"}, StringSplitOptions.None);
//                //foreach (string s in parts) {
//                //    Debug.Log(s);
//                //}
//                int lod = int.Parse(parts[parts.Length-2]);
//                HandleLodLevel(fileName, lod);
//            }
//            catch (Exception e) {
//                Debug.LogError("Couldn't parse file, skipping: " + fileName);
//                Debug.LogException(e);
//            }
//        }
//
//        public static void OnPostprocessAllAssets(
//            string[] importedAssets,
//            string[] deletedAssets,
//            string[] movedAssets,
//            string[] movedFromAssetPath)
//        {
//            if (OnImportDone != null)
//                OnImportDone();
//        }
//
//        private void HandleLodLevel(string fileName, int level) {
//            var importer = assetImporter as TextureImporter;
//            LodLevel lod = LandmassEditorUtilities.Instance.ImportCfg.LodLevels[level];
//
//            if (MatchMapTag(fileName, LandmassEditorUtilities.Instance.ImportCfg.SplatmapTag)) {
//
//                Debug.Log("Importing splatmap: " + assetPath);
//
//                importer.wrapMode = TextureWrapMode.Repeat;
//                importer.generateCubemap = TextureImporterGenerateCubemap.None;
//                importer.isReadable = true;
//                importer.grayscaleToAlpha = false;
//                importer.mipmapEnabled = false;
//                importer.convertToNormalmap = false;
//                importer.normalmap = false;
//                importer.lightmap = false;
//                importer.maxTextureSize = lod.SplatmapResolution;
//                importer.textureType = TextureImporterType.Advanced;
//                importer.textureFormat = TextureImporterFormat.ARGB32;
//            } else if (MatchMapTag(fileName, LandmassEditorUtilities.Instance.ImportCfg.DetailmapTag)) {
//
//                Debug.Log("Importing detailmap: " + assetPath);
//
//                importer.wrapMode = TextureWrapMode.Repeat;
//                importer.generateCubemap = TextureImporterGenerateCubemap.None;
//                importer.isReadable = true;
//                importer.grayscaleToAlpha = false;
//                importer.mipmapEnabled = false;
//                importer.convertToNormalmap = false;
//                importer.normalmap = false;
//                importer.lightmap = false;
//                importer.maxTextureSize = lod.DetailmapResolution;
//                importer.textureType = TextureImporterType.Advanced;
//                importer.textureFormat = TextureImporterFormat.ARGB32;
//            } else if (MatchMapTag(fileName, LandmassEditorUtilities.Instance.ImportCfg.TreemapTag)) {
//
//                Debug.Log("Importing treemap: " + assetPath);
//
//
//                importer.wrapMode = TextureWrapMode.Clamp;
//                importer.generateCubemap = TextureImporterGenerateCubemap.None;
//                importer.isReadable = true;
//                importer.grayscaleToAlpha = false;
//                importer.mipmapEnabled = false;
//                importer.convertToNormalmap = false;
//                importer.normalmap = false;
//                importer.lightmap = false;
//                importer.maxTextureSize = lod.DetailmapResolution;
//                importer.textureType = TextureImporterType.Advanced;
//                importer.textureFormat = TextureImporterFormat.ARGB32;
//            } else if (MatchMapTag(fileName, "_c")) {
//
//                Debug.Log("Importing colormap: " + assetPath);
//
//                importer.wrapMode = TextureWrapMode.Clamp;
//                importer.generateCubemap = TextureImporterGenerateCubemap.None;
//                importer.isReadable = true;
//                importer.grayscaleToAlpha = false;
//                importer.mipmapEnabled = false;
//                importer.convertToNormalmap = false;
//                importer.normalmap = false;
//                importer.lightmap = false;
//                importer.maxTextureSize = lod.ColormapResolution;
//                importer.textureType = TextureImporterType.Advanced;
//                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
//            } else if (MatchMapTag(fileName, "_n")) {
//
//                Debug.Log("Importing normalmap: " + assetPath);
//
//                importer.wrapMode = TextureWrapMode.Clamp;
//                importer.generateCubemap = TextureImporterGenerateCubemap.None;
//                importer.isReadable = true;
//                importer.grayscaleToAlpha = false;
//                importer.mipmapEnabled = true;
//                importer.convertToNormalmap = false;
//                importer.normalmap = true;
//                importer.lightmap = false;
//                importer.maxTextureSize = lod.NormalmapResolution;
//                importer.textureType = TextureImporterType.Advanced;
//                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
//            }
//        }
//
//        private bool MatchMapTag(string fileName, string tag)
//        {
//            return
//                fileName.EndsWith(".png") &&
//                (
//                    fileName.Contains(tag)
//                );
//        }
//
//        /// <summary>
//        /// Event used to subscribe to loading notifications. Could be used to auto-update assets in scene.
//        /// </summary>
//        public static event System.Action OnImportDone;
//    }
}