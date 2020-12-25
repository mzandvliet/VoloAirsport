namespace Oculus.Platform
{
  using UnityEngine;
  using System.Collections;
  using System;

  public class AndroidPlatform
  {
    public bool Initialize(string appId)
    {
#if UNITY_ANDROID
      return CAPI.ovr_UnityInitWrapper(appId);
#else
      return false;
#endif
    }
  }
}
