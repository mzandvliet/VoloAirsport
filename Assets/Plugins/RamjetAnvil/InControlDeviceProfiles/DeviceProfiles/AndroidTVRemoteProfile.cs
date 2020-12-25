using System;


namespace InControl
{
	// Tested with ADT-1
	// Profile by Artūras 'arturaz' Šlajus <arturas@tinylabproductions.com>
	//
	// @cond nodoc
	[AutoDiscover]
	public class AndroidTVRemoteProfile : UnityInputDeviceProfile
	{
		public AndroidTVRemoteProfile()
		{
			Name = "Android TV Remote";
			Meta = "Android TV Remotet on Android TV";

			SupportedPlatforms = new[] { 
				"Android"
			};

			JoystickNames = new[] { 
				"touch-input", 
				"navigation-input"
			};

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "A",
					Target = InputControlTarget.Action1,
					Source = Button0
				}
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
