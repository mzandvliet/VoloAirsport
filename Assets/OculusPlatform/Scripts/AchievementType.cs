namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  // These should be kept in sync with the enum in OVR_AchievementType.h
  public enum AchievementType : uint
  {
    // OVR_ENUM_START
    [Description("UNKNOWN")]
    Unknown=0,

    [Description("SIMPLE")]
    Simple,

    [Description("BITFIELD")]
    Bitfield,

    [Description("COUNT")]
    Count,
    // OVR_ENUM_END
  };

}
