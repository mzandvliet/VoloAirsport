using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


namespace InControl
{
	public sealed class AutoDiscover : Attribute
	{
	}


	public abstract class UnityInputDeviceProfile
	{
		public string Name { get; protected set; }
		public string Meta { get; protected set; }

		public InputControlMapping[] AnalogMappings { get; protected set; }
		public InputControlMapping[] ButtonMappings { get; protected set; }

		public string[] SupportedPlatforms;
		public string[] JoystickNames;
		public string[] JoystickRegex;

		public string LastResortRegex;

		public VersionInfo MinUnityVersion { get; protected set; }
		public VersionInfo MaxUnityVersion { get; protected set; }

		static HashSet<Type> hideList = new HashSet<Type>();

		float sensitivity;
		float lowerDeadZone;
		float upperDeadZone;


		public UnityInputDeviceProfile()
		{
			Name = "";
			Meta = "";

			sensitivity = 1.0f;
			lowerDeadZone = 0.2f;
			upperDeadZone = 0.9f;

			AnalogMappings = new InputControlMapping[0];
			ButtonMappings = new InputControlMapping[0];

			MinUnityVersion = new VersionInfo( 3 );
			MaxUnityVersion = new VersionInfo( 9 );
		}


		public float Sensitivity
		{ 
			get { return sensitivity; }
			protected set { sensitivity = Mathf.Clamp01( value ); }
		}


		public float LowerDeadZone
		{ 
			get { return lowerDeadZone; }
			protected set { lowerDeadZone = Mathf.Clamp01( value ); }
		}


		public float UpperDeadZone
		{ 
			get { return upperDeadZone; }
			protected set { upperDeadZone = Mathf.Clamp01( value ); }
		}

		public bool IsSupportedOnThisPlatform
		{
			get
			{
				if (!IsSupportedOnThisVersionOfUnity)
				{
					return false;
				}

				if (SupportedPlatforms == null || SupportedPlatforms.Length == 0)
				{
					return true;
				}

				foreach (var platform in SupportedPlatforms)
				{
					if (SystemInfo.operatingSystem.Contains( platform.ToUpper()) || 
                        SystemInfo.deviceModel.Contains(platform.ToUpper()))
					{
						return true;
					}
				}

				return false;
			}
		}


		public bool IsSupportedOnThisVersionOfUnity
		{
			get
			{
				var unityVersion = VersionInfo.UnityVersion();
				return unityVersion >= MinUnityVersion && unityVersion <= MaxUnityVersion;
			}
		}


		public bool IsJoystick
		{ 
			get
			{ 
				return (LastResortRegex != null) ||
				(JoystickNames != null && JoystickNames.Length > 0) ||
				(JoystickRegex != null && JoystickRegex.Length > 0);
			} 
		}


		public bool IsNotJoystick
		{ 
			get { return !IsJoystick; } 
		}


		public bool HasJoystickName( string joystickName )
		{
			if (IsNotJoystick)
			{
				return false;
			}

			if (JoystickNames != null)
			{
				if (JoystickNames.Contains( joystickName, StringComparer.OrdinalIgnoreCase ))
				{
					return true;
				}
			}

			if (JoystickRegex != null)
			{
				for (int i = 0; i < JoystickRegex.Length; i++)
				{
					if (Regex.IsMatch( joystickName, JoystickRegex[i], RegexOptions.IgnoreCase ))
					{
						return true;
					}
				}
			}

			return false;
		}


		public bool HasLastResortRegex( string joystickName )
		{
			if (IsNotJoystick)
			{
				return false;
			}

			if (LastResortRegex == null)
			{
				return false;
			}

			return Regex.IsMatch( joystickName, LastResortRegex, RegexOptions.IgnoreCase );
		}


		public bool HasJoystickOrRegexName( string joystickName )
		{
			return HasJoystickName( joystickName ) || HasLastResortRegex( joystickName );
		}


		public static void Hide( Type type )
		{
			hideList.Add( type );
		}
		
		
		public bool IsHidden
		{
			get { return hideList.Contains( GetType() ); }
		}


		public virtual bool IsKnown
		{
			get { return true; }
		}


		public int AnalogCount
		{
			get { return AnalogMappings.Length; }
		}


		public int ButtonCount
		{
			get { return ButtonMappings.Length; }
		}


		#region InputControlSource Helpers

		protected static InputControlSource Button( int index )
		{
			return new InputControlSource.Button(index);
		}

		protected static InputControlSource Analog( int index )
		{
			return new InputControlSource.Axis( index );
		}

		protected static readonly InputControlSource Button0 = Button( 0 );
		protected static readonly InputControlSource Button1 = Button( 1 );
		protected static readonly InputControlSource Button2 = Button( 2 );
		protected static readonly InputControlSource Button3 = Button( 3 );
		protected static readonly InputControlSource Button4 = Button( 4 );
		protected static readonly InputControlSource Button5 = Button( 5 );
		protected static readonly InputControlSource Button6 = Button( 6 );
		protected static readonly InputControlSource Button7 = Button( 7 );
		protected static readonly InputControlSource Button8 = Button( 8 );
		protected static readonly InputControlSource Button9 = Button( 9 );
		protected static readonly InputControlSource Button10 = Button( 10 );
		protected static readonly InputControlSource Button11 = Button( 11 );
		protected static readonly InputControlSource Button12 = Button( 12 );
		protected static readonly InputControlSource Button13 = Button( 13 );
		protected static readonly InputControlSource Button14 = Button( 14 );
		protected static readonly InputControlSource Button15 = Button( 15 );
		protected static readonly InputControlSource Button16 = Button( 16 );
		protected static readonly InputControlSource Button17 = Button( 17 );
		protected static readonly InputControlSource Button18 = Button( 18 );
		protected static readonly InputControlSource Button19 = Button( 19 );

		protected static readonly InputControlSource Analog0 = Analog( 0 );
		protected static readonly InputControlSource Analog1 = Analog( 1 );
		protected static readonly InputControlSource Analog2 = Analog( 2 );
		protected static readonly InputControlSource Analog3 = Analog( 3 );
		protected static readonly InputControlSource Analog4 = Analog( 4 );
		protected static readonly InputControlSource Analog5 = Analog( 5 );
		protected static readonly InputControlSource Analog6 = Analog( 6 );
		protected static readonly InputControlSource Analog7 = Analog( 7 );
		protected static readonly InputControlSource Analog8 = Analog( 8 );
		protected static readonly InputControlSource Analog9 = Analog( 9 );
		protected static readonly InputControlSource Analog10 = Analog( 10 );
		protected static readonly InputControlSource Analog11 = Analog( 11 );
		protected static readonly InputControlSource Analog12 = Analog( 12 );
		protected static readonly InputControlSource Analog13 = Analog( 13 );
		protected static readonly InputControlSource Analog14 = Analog( 14 );
		protected static readonly InputControlSource Analog15 = Analog( 15 );
		protected static readonly InputControlSource Analog16 = Analog( 16 );
		protected static readonly InputControlSource Analog17 = Analog( 17 );
		protected static readonly InputControlSource Analog18 = Analog( 18 );
		protected static readonly InputControlSource Analog19 = Analog( 19 );

		#endregion
	}
}

