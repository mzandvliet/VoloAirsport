using System;


namespace InControl
{
	// @cond nodoc
	[AutoDiscover]
	public class AmazonFireTVRemote : UnityInputDeviceProfile
	{
		public AmazonFireTVRemote()
		{
			Name = "Amazon Fire TV Remote";
			Meta = "Amazon Fire TV Remote on Amazon Fire TV";

			SupportedPlatforms = new[] {
				"Amazon AFTB",
				"Amazon AFTM"
			};

			JoystickNames = new[] {
				"",
				"Amazon Fire TV Remote"
			};

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "A",
					Target = InputControlTarget.Action1,
					Source = Button0
				},
//				new InputControlMapping {
//					Handle = "Back",
//					Target = InputControlTarget.Select,
//					Source = KeyCodeButton( UnityEngine.KeyCode.Escape )
//				}
			};

			AnalogMappings = new[] {
				new InputControlMapping {
					Handle = "DPad X",
					Target = InputControlTarget.DPadX,
					Source = Analog4,
					SourceRange = InputControlMapping.Range.Complete,
					TargetRange = InputControlMapping.Range.Complete
				},
				new InputControlMapping {
					Handle = "DPad Y",
					Target = InputControlTarget.DPadY,
					Source = Analog5,
					SourceRange = InputControlMapping.Range.Complete,
					TargetRange = InputControlMapping.Range.Complete,
				},
			};
		}
	}
}

