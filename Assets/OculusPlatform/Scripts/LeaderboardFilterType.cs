namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  // These should be kept in sync with the enum in OVR_LeaderboardFilterType.h
  public enum LeaderboardFilterType : uint
  {
    // OVR_ENUM_START
    [Description("NONE")]
    None = 0,

    [Description("FRIENDS")]
    Friends,
    // OVR_ENUM_END
  };

}
