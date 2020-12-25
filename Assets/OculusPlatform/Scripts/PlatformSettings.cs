namespace Oculus.Platform
{
  using UnityEngine;
  using System.Collections;

#if UNITY_EDITOR
  [UnityEditor.InitializeOnLoad]
#endif
  public sealed class PlatformSettings : ScriptableObject
  {
    public static string AppID
    {
      get { return Instance.ovrAppID; }
      set { Instance.ovrAppID = value; }
    }

    [SerializeField]
    private string ovrAppID = "";

    private static PlatformSettings instance;
    public static PlatformSettings Instance
    {
      get
      {
        if (instance == null)
        {
          instance = Resources.Load<PlatformSettings>("OculusPlatformSettings");
        }
        return instance;
      }

      set
      {
        instance = value;
      }
    }
  }
}
