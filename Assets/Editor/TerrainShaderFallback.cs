using UnityEditor;
using UnityEngine;

namespace RamjetAnvil.Volo.EditorTools {

    // Temporary fallback while Assets/Shaders/Terrain/Standard-FirstPass-Custom.shader
    // (triplanar + snow/detail blend on top of Unity's TerrainSplatmapCommon.cginc) is
    // broken under Unity 6's shader compiler. Swaps the SwissAlps terrain materials to
    // Unity's stock built-in terrain shader so the terrain is visible again while the
    // custom shader gets fixed properly.
    public static class TerrainShaderFallback {
        private static readonly string[] MaterialPaths = {
            "Assets/Materials/Terrain/terrain_lod0_material.mat",
            "Assets/Materials/Terrain/terrain_lod1_material.mat"
        };

        [MenuItem("Volo/Terrain/Use Built-in Terrain Shader (temporary fallback)")]
        public static void UseBuiltInTerrainShader() {
            var fallbackShader = Shader.Find("Nature/Terrain/Standard");
            if (fallbackShader == null) {
                Debug.LogError("Could not find built-in shader 'Nature/Terrain/Standard'.");
                return;
            }

            foreach (var path in MaterialPaths) {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) {
                    Debug.LogWarning("Could not load material at " + path);
                    continue;
                }
                material.shader = fallbackShader;
                EditorUtility.SetDirty(material);
                Debug.Log("Set " + path + " to use " + fallbackShader.name);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
