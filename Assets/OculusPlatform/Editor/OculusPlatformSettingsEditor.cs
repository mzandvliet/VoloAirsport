namespace Oculus.Platform
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using UnityEditor;
  using UnityEngine;

  [CustomEditor(typeof(PlatformSettings))]
  public class OculusPlatformSettingsEditor : Editor
  {
    [UnityEditor.MenuItem("Oculus Platform/Edit Settings")]
    public static void Edit()
    {
      var settings = PlatformSettings.Instance;
      if (settings == null)
      {
        settings = ScriptableObject.CreateInstance<PlatformSettings>();
        string properPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(properPath))
        {
          AssetDatabase.CreateFolder("Assets", "Resources");
        }

        string fullPath = Path.Combine(
          Path.Combine("Assets", "Resources"),
          "OculusPlatformSettings.asset"
        );
        AssetDatabase.CreateAsset(settings, fullPath);
        PlatformSettings.Instance = settings;
      }
      UnityEditor.Selection.activeObject = settings;
    }

    private bool showBuildSettings = true;
    private bool showUnityEditorSettings = true;

    public override void OnInspectorGUI()
    {
      EditorGUILayout.Separator();
      if (String.IsNullOrEmpty(PlatformSettings.AppID))
      {
        EditorGUILayout.HelpBox("Add your Oculus App Id", MessageType.Error);
      }
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Oculus App Id");
      GUI.changed = false;
      PlatformSettings.AppID = EditorGUILayout.TextField(PlatformSettings.AppID);
      SetDirtyOnGUIChange();
      EditorGUILayout.EndHorizontal();
      if (GUILayout.Button("Create / Find your app on https://dashboard.oculus.com"))
      {
        Application.OpenURL("https://dashboard.oculus.com/");
      }
      EditorGUILayout.Separator();

      showBuildSettings = EditorGUILayout.Foldout(showBuildSettings, "Build Settings");
      if (showBuildSettings)
      {
        if (!PlayerSettings.virtualRealitySupported)
        {
          EditorGUILayout.HelpBox("VR Support isn't enabled in the Player Settings", MessageType.Warning);
        }
        else
        {
          EditorGUILayout.HelpBox("VR Support is enabled", MessageType.Info);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Virtual Reality Support");
        PlayerSettings.virtualRealitySupported = EditorGUILayout.Toggle(PlayerSettings.virtualRealitySupported);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Bundle Identifier");
        PlayerSettings.bundleIdentifier = EditorGUILayout.TextField(PlayerSettings.bundleIdentifier);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Bundle Version");
        PlayerSettings.bundleVersion = EditorGUILayout.TextField(PlayerSettings.bundleVersion);
        EditorGUILayout.EndHorizontal();
      }

      EditorGUILayout.Separator();

      GUI.enabled = !String.IsNullOrEmpty(PlatformSettings.AppID);
      showUnityEditorSettings = EditorGUILayout.Foldout(showUnityEditorSettings, "Unity Editor Settings");
      if (showUnityEditorSettings)
      {
        if (String.IsNullOrEmpty(StandalonePlatformSettings.OculusPlatformAccessToken))
        {
          if (GUILayout.Button("Get User Token"))
          {
            Application.OpenURL("https://developer2.oculus.com/application/" + PlatformSettings.AppID + "/api");
          }
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Oculus User Token");
        StandalonePlatformSettings.OculusPlatformAccessToken = EditorGUILayout.TextField(StandalonePlatformSettings.OculusPlatformAccessToken);
        EditorGUILayout.EndHorizontal();
      }
      GUI.enabled = true;
    }

    private void SetDirtyOnGUIChange()
    {
      if (GUI.changed)
      {
        EditorUtility.SetDirty(PlatformSettings.Instance);
        GUI.changed = false;
      }
    }
  }
}
