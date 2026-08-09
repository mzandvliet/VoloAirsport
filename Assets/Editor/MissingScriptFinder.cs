using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RamjetAnvil.Volo.EditorTools {

    // Diagnostic: RamNet's ObjectMessageRouter throws "One or more components found in
    // passed GameObject are null" when GetComponentsInChildren finds a Missing Script slot
    // anywhere under a GameObjectNetworkId-marked object. This scans every loaded scene for
    // any GameObject with a missing script and logs its full hierarchy path, so we don't have
    // to eyeball the whole Hierarchy window by hand.
    public static class MissingScriptFinder {
        [MenuItem("Volo/Diagnostics/Find Missing Scripts In Loaded Scenes")]
        public static void FindMissingScripts() {
            var foundAny = false;
            for (int sceneIndex = 0; sceneIndex < EditorSceneManager.sceneCount; sceneIndex++) {
                var scene = EditorSceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded) {
                    continue;
                }
                foreach (var root in scene.GetRootGameObjects()) {
                    foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true)) {
                        var go = transform.gameObject;
                        var components = go.GetComponents<Component>();
                        for (int i = 0; i < components.Length; i++) {
                            if (components[i] == null) {
                                foundAny = true;
                                Debug.LogError("Missing script on '" + GetPath(go) + "' (scene: " + scene.name + "), component slot " + i, go);
                            }
                        }
                    }
                }
            }
            if (!foundAny) {
                Debug.Log("No missing scripts found in any loaded scene.");
            }
        }

        private static string GetPath(GameObject go) {
            var path = go.name;
            var t = go.transform.parent;
            while (t != null) {
                path = t.name + "/" + path;
                t = t.parent;
            }
            return path;
        }
    }
}
