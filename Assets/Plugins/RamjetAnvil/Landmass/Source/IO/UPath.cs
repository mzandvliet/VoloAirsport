using System.IO;
using UnityEngine;

public static class UPath
{
    static string projectPath;
    static string directorySeparatorUnity = "/";
    static string directorySeparatorSystem;

    static UPath()
    {
        projectPath = Application.dataPath.Replace("/Assets", "/");
        directorySeparatorSystem = Path.DirectorySeparatorChar.ToString();
    }

    // Because Path.Combine uses "\" on windows, which doesn't work with Unity's local paths
    public static string Combine(string path1, string path2)
    {
        return path1 + (path1.EndsWith("/") ? "" : "/") +
            (path2.StartsWith("/") ? path2.Substring(1) : path2) +
            (!path2.Contains(".")  ? (path2.EndsWith("/") ? "" : "/") : "");
    }

    public static string GetAbsolutePath(string path)
    {
        string absPath = Combine(projectPath, path);
        absPath = ConvertPathSeparatorsToSystem(absPath);
        return absPath;
    }

    public static string GetProjectPath(string path)
    {
        string relPath = ConvertPathSeparatorsToUnity(path);
        int startIndex = path.IndexOf("Assets");
        relPath = relPath.Remove(0, startIndex);
        if (relPath.StartsWith("/"))
            relPath = relPath.Remove(0, 1);
        return relPath;
    }

    public static string ConvertPathSeparatorsToSystem(string path)
    {
        string uPath = path.Replace("/", directorySeparatorSystem);
        uPath = uPath.Replace("\\", directorySeparatorSystem);
        return uPath;
    }

    public static string ConvertPathSeparatorsToUnity(string path)
    {
        string uPath = path.Replace("/", directorySeparatorUnity);
        uPath = uPath.Replace("\\", directorySeparatorUnity);
        return uPath;
    }

    public static bool ProjectFolderExists(string folderPath)
    {
        string absPath = GetAbsolutePath(folderPath);
        return Directory.Exists(absPath);
    }

    public static void CreateProjectFolder(string folderPath)
    {
        //string absPath = GetAbsolutePath(folderPath);
        Directory.CreateDirectory(folderPath);
    }
}
