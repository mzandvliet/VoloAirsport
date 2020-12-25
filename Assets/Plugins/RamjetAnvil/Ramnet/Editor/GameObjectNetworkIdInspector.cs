using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameObjectNetworkId))]
public class GameObjectNetworkIdEditor : Editor {

    public override void OnInspectorGUI() {
        var instance = target as GameObjectNetworkId;
        if (instance != null && instance.Id != null) {
            EditorGUILayout.LabelField("Id", instance.Id.ToString());    
        }

        if (GUILayout.Button("Generate all Guids")) {
            var networkedInstances = FindObjectsOfType<GameObjectNetworkId>();
            for (int i = 0; i < networkedInstances.Length; i++) {
                var networkedInstance = networkedInstances[i];
                networkedInstance.GenerateGuid();
                UnityEditor.EditorUtility.SetDirty(networkedInstance);
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
    }
}