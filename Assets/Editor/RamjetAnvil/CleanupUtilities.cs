using System.IO;
using UnityEngine;
using UnityEditor;

public class CleanupUtilities {
    [MenuItem("Tools/Remove Empty Project Folders")]
    static void RemoveEmptyProjectFolders() {
        string path = Application.dataPath;
        RemoveEmptyFoldersRecursively(path);
    }

    private static void RemoveEmptyFoldersRecursively(string path) {
        // First, traverse depth first so that deepest folders can be deleted first
        var files = Directory.GetFiles(path);
        var folders = Directory.GetDirectories(path);

        for (int i = 0; i < folders.Length; i++) {
            RemoveEmptyFoldersRecursively(folders[i]);
        }

        // Then, check if the current folder is now empty, if so remove it
        files = Directory.GetFiles(path);
        folders = Directory.GetDirectories(path);

        if (files.Length == 0 && folders.Length == 0 && !path.Contains(".hg")) {
            DeleteFolderAndMetaFile(path);
        }
    }

    private static void DeleteFolderAndMetaFile(string path) {
        Debug.LogWarning("Deleting empty folder: " + path + ", and associated meta file");
        string metaPath = path + ".meta";
        if (File.Exists(metaPath)) {
            File.Delete(metaPath);
        }
        Directory.Delete(path);
    }
}