using System;
using UnityEngine;


namespace InControl
{
	public class InputControlMapping
	{
		public class Range
		{
			public static Range Complete = new Range { Minimum = -1.0f, Maximum = 1.0f };
			public static Range Positive = new Range { Minimum =  0.0f, Maximum = 1.0f };
			public static Range Negative = new Range { Minimum = -1.0f, Maximum = 0.0f };

			public float Minimum;
			public float Maximum;
		}


		public InputControlSource Source;
		public InputControlTarget Target;

		// Invert the final mapped value.
		public bool Invert;

		// Analog values will be multiplied by this number before processing.
		public float Scale = 1.0f;

		// Raw inputs won't be processed except for scaling (mice and trackpads).
		public bool Raw;

		// This is primarily to fix a bug with the wired Xbox controller on Mac.
		public bool IgnoreInitialZeroValue;

		public Range SourceRange = Range.Complete;
		public Range TargetRange = Range.Complete;

		private string _handle;

		public string Handle
		{
			get { return (string.IsNullOrEmpty( _handle )) ? Target.ToString() : _handle; }
			set { _handle = value; }
		}

		bool IsYAxis
		{
			get
			{
				return Target == InputControlTarget.LeftStickY   ||
					   Target == InputControlTarget.RightStickY;
			}
		}
	}
}
