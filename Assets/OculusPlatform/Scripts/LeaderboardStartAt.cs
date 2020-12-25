namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  // These should be kept in sync with the enum in OVR_LeaderboardStartAt.h
  public enum LeaderboardStartAt: uint
  {
    // OVR_ENUM_START
    [Description("TOP")]
    Top = 0,

    [Description("CENTERED_ON_VIEWER")]
    CenteredOnViewer,
    // OVR_ENUM_END
  };

}
