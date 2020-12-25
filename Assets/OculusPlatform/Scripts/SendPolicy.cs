namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  public enum SendPolicy : uint
  {
    [Description("UNRELIABLE")]
    Unreliable = 0,

    [Description("RELIABLE")]
    Reliable,
  }
};
