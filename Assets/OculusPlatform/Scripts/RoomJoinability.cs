namespace Oculus.Platform
{
	using UnityEngine;
	using System;
	using System.Collections;
	using System.ComponentModel;

	public enum RoomJoinability : uint
	{
		// OVR_ENUM_START
		[Description("UNKNOWN")]
		Unknown=0,

		[Description("ARE_IN")]
		AreIn,

		[Description("ARE_KICKED")]
		AreKicked,

		[Description("CAN_JOIN")]
		CanJoin,

		[Description("IS_FULL")]
		IsFull,

		[Description("NO_VIEWER")]
		NoViewer,

		[Description("POLICY_PREVENTS")]
		PolicyPrevents,
		// OVR_ENUM_END
	};

}
