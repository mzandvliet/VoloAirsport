namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  public enum RoomType : uint
  {
    [Description("UNKNOWN")]
    Unknown=0,

    [Description("MATCHMAKING")]
    Matchmaking,

    [Description("MODERATED")]
    Moderated,

    [Description("PRIVATE")]
    Private,

    [Description("SOLO")]
    Solo,
  };

}
