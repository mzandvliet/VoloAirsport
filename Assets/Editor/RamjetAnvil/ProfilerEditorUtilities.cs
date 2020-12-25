using UnityEngine;
using UnityEditor;

public class ProfilerEditorUtilities {
    [MenuItem("Tools/Profiling/Load Existing Frames")]
    static void LoadLog() {
//        string path = EditorUtility.OpenFilePanelWithFilters("Browse", EditorApplication.applicationPath, new[] {"Unity Profiler Data", "data"});
        string path = EditorUtility.OpenFilePanel("Browse", EditorApplication.applicationPath, "data"); //new[] { "Unity Profiler Data", "data" }
        path = path.Replace(".data", "");
        Debug.Log("Loading profiler data: " + path);
        UnityEngine.Profiling.Profiler.AddFramesFromFile(path);
    }
}