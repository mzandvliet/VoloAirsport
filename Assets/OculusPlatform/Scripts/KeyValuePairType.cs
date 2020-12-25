namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  public enum KeyValuePairType : uint
  {
    // OVR_ENUM_START
    [Description("STRING")]
    String=0,

    [Description("INT")]
    Int,

    [Description("DOUBLE")]
    Double,
    // OVR_ENUM_END
  };

}
