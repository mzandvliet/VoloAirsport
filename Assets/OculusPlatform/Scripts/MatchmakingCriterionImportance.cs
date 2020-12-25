namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  public enum MatchmakingCriterionImportance : uint
  {
    // OVR_ENUM_START
    [Description("REQUIRED")]
    Required=0,

    [Description("HIGH")]
    High,

    [Description("MEDIUM")]
    Medium,

    [Description("LOW")]
    Low,
    // OVR_ENUM_END
  };

}
