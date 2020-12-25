using UnityEngine;

public class WorldTransformDebug : MonoBehaviour {
    [SerializeField] private WorldTransform _worldTransform;

    private void OnGUI() {
        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Label("Position: " + _worldTransform.Position);
            GUILayout.Label("Region: " + WorldSystem.Instance.WorldPointToRegion(_worldTransform.Position));
        }
        GUILayout.EndVertical();
    }
}
