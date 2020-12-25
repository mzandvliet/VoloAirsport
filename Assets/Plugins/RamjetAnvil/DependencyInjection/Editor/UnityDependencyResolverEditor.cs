using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityDependencyResolver))]
public class UnityDependencyResolverEditor : Editor {

    public override void OnInspectorGUI() {
        var instance = target as UnityDependencyResolver;
        if (instance != null && instance.Dependencies != null) {
            for (int i = 0; i < instance.Dependencies.Count; i++) {
                var dependency = instance.Dependencies[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(dependency.Name);
                EditorGUILayout.ObjectField(dependency.Reference, dependency.GetType(), true);
                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Resolve dependencies") && instance != null) {
            instance.Resolve();
        }
    }
}
