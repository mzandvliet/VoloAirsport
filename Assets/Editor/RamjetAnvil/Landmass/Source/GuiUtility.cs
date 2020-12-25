using UnityEngine;
using UnityEditor;
using System.IO;

using EG = UnityEditor.EditorGUI;
using EGL = UnityEditor.EditorGUILayout;

public class GuiUtils
{
	static GUISkin editorSkin;
	
	// Todo: Can we avoid having to to this? It's unclean.
    public static GUISkin Skin
    {
        get
        {
            if (editorSkin == null)
                editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            return editorSkin;
        }
    }
	
	public static string ProjectFolderBrowser(string title, string description, string folder, string defaultName)
    {
        string path = folder; 
        EGL.BeginHorizontal();
        {
            EGL.PrefixLabel(title);
            GUILayout.Label(folder, Skin.textField);

            string newPath = folder;
            if (GUILayout.Button("Browse", GUILayout.Width(64f))) {
                newPath = EditorUtility.SaveFolderPanel(description, folder, defaultName);
                if (newPath != string.Empty)
                    newPath = UPath.GetProjectPath(newPath);
                else
                    newPath = folder;
            }
            path = newPath;
        }
        EGL.EndHorizontal();

        return path;
    }

    public static string ProjectFileBrowser(string title, string description, string file, string extention)
    {
        string path = file;
        EGL.BeginHorizontal();
        {
            EGL.PrefixLabel(title);
            GUILayout.Label(file, Skin.textField);

            string newPath = file;
            if (GUILayout.Button("Browse", GUILayout.Width(64f))) {
                newPath = EditorUtility.OpenFilePanel(description, Path.GetFullPath(UPath.GetAbsolutePath(file)), extention);
                if (newPath != string.Empty)
                    newPath = UPath.GetProjectPath(newPath);
                else
                    newPath = file;
            }
            path = newPath;
        }
        EGL.EndHorizontal();

        return path;
    }
}

