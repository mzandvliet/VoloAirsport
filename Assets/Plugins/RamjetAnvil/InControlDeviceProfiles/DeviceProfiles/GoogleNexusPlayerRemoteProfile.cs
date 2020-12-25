using System;


namespace InControl
{
	[AutoDiscover]
	public class GoogleNexusPlayerRemoteProfile : UnityInputDeviceProfile
	{
		public GoogleNexusPlayerRemoteProfile()
		{
			Name = "Google Nexus Player Remote";
			Meta = "Google Nexus Player Remote";

			SupportedPlatforms = new[] {
				"Android"
			};

			JoystickNames = new[] {
				"Google Nexus Remote"
			};

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "A",
					Target = InputControlTarget.Action1,
					Source = Button0
				},
//				new InputControlMapping {
//					Handle = "Back",
//					Target = InputControlTarget.Back,
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
