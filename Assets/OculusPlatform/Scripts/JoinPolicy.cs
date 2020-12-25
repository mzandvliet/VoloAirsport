namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.ComponentModel;

  public enum JoinPolicy : uint
  {
    // OVR_ENUM_START
    [Description("NONE")]
    None=0,

    [Description("EVERYONE")]
    Everyone,

    [Description("FRIENDS_OF_MEMBERS")]
    FriendsOfMembers,

    [Description("FRIENDS_OF_OWNER")]
    FriendsOfOwner,

    [Description("INVITED_USERS")]
    InvitedUsers,
    // OVR_ENUM_END
  };

}
