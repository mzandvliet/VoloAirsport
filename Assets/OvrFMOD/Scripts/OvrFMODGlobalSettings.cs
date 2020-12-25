/************************************************************************************
Filename    :   OvrFMODGlobalSettings.cs
Content     :   Interface into global settings of the Oculus Spatializer for FMOD.
Created     :   March 30, 2015
Authors     :   Peter Stirling
Copyright   :   Copyright 2015 Oculus VR, Inc. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.1 (the "License"); 
you may not use the Oculus VR Rift SDK except in compliance with the License, 
which is provided at the time of installation or download, or which 
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.1 

Unless required by applicable law or agreed to in writing, the Oculus VR SDK 
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
************************************************************************************/
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//-------------------------------------------------------------------------------------
// ***** OvrFMODGlobalSettings
//
/// <summary>
/// OSPManager interfaces into the Oculus Spatializer. This component should be added
/// into the scene once. 
///
/// </summary>
public class OvrFMODGlobalSettings : MonoBehaviour
{

	public const string strOSP = "ovrfmod";
	
	// * * * * * * * * * * * * *
	// Import functions	
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_Initialize(int SampleRate, int BufferLength);	
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_SetBypass(bool Enabled);
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_SetGain(float Gain_dB);
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_SetGlobalScale(float Scale);	
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_SetEarlyReflectionsEnabled(bool Enabled);	
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_SetLateReverberationEnabled(bool Enabled);		
	[DllImport(strOSP)]
	private static extern bool OSP_FMOD_SetSimpleBoxRoomParameters(float Width, float Height, float Depth, float RefLeft, float RefRight, float RefUp, float RefDown, float RefBehind, float RefFront);


	[SerializeField]
	private bool bypass = false;
	public  bool Bypass
	{
		get{ return bypass; }
		set{ bypass = value; 
			OSP_FMOD_SetBypass(bypass);}
	}
	
	[SerializeField]
	private float globalScale = 1.0f;
	public  float GlobalScale
	{
		get{return globalScale; }
		set{globalScale = Mathf.Clamp (value, 0.00001f, 10000.0f); 
			OSP_FMOD_SetGlobalScale(globalScale);}
	}
	
	[SerializeField]
	private float gain = 0.0f;
	public  float Gain
	{
		get{return gain; }
		set{gain = Mathf.Clamp(value, -24.0f, 24.0f); 
			OSP_FMOD_SetGain(gain);}
	}
	
	//----------------------
	// Reflection parameters
	private bool dirtyReflection;
	
	[SerializeField]
	private bool enableReflections = true;
	public bool  EnableReflections
	{
		get{return enableReflections; }
		set{enableReflections = value; dirtyReflection = true;}
	}
	
	[SerializeField]
	private bool enableReverb = false;
	public bool  EnableReverb
	{
		get{return enableReverb; }
		set{enableReverb = value; dirtyReflection = true;}
	}
	
	[SerializeField]
	private Vector3 dimensions = new Vector3 (8.0f, 2.5f, 5.0f);
	public Vector3 Dimensions
	{
		get{return dimensions; }
		set{dimensions = value; 
			dimensions.x = Mathf.Clamp (dimensions.x, 0.0f, 230.0f);
			dimensions.y = Mathf.Clamp (dimensions.y, 0.0f, 230.0f);
			dimensions.z = Mathf.Clamp (dimensions.z, 0.0f, 230.0f);
			dirtyReflection = true;}
	}
	
	[SerializeField]
	private Vector2 reflectionLeftRight = new Vector2(0.75f, 0.75f);
	public Vector2 ReflectionLeftRight
	{
		get{return reflectionLeftRight; }
		set{reflectionLeftRight = value; 
			reflectionLeftRight.x = Mathf.Clamp (reflectionLeftRight.x, 0.0f, 0.95f);
			reflectionLeftRight.y = Mathf.Clamp (reflectionLeftRight.y, 0.0f, 0.95f);
			dirtyReflection = true;}
	}
	
	[SerializeField]
	private Vector2 reflectionUpDown = new Vector2(0.85f, 0.25f);
	public Vector2 ReflectionUpDown
	{
		get{return reflectionUpDown; }
		set{reflectionUpDown = value; 
			reflectionUpDown.x = Mathf.Clamp (reflectionUpDown.x, 0.0f, 0.95f);
			reflectionUpDown.y = Mathf.Clamp (reflectionUpDown.y, 0.0f, 0.95f);
			dirtyReflection = true;}
	}
	
	[SerializeField]
	private Vector2 reflectionFrontBack = new Vector2(0.75f, 0.75f);
	public Vector2 ReflectionFrontBack
	{
		get{return reflectionFrontBack; }
		set{reflectionFrontBack = value; 
			reflectionFrontBack.x = Mathf.Clamp (reflectionFrontBack.x, 0.0f, 0.95f);
			reflectionFrontBack.y = Mathf.Clamp (reflectionFrontBack.y, 0.0f, 0.95f);
			dirtyReflection = true;}
	}

	void Start()
	{
//		FMOD.System sys = null;
//		FMOD_StudioSystem.instance.System.getLowLevelSystem(out sys);
        FMOD.System sys = FMODUnity.RuntimeManager.LowlevelSystem;
		
		FMOD.RESULT result = FMOD.RESULT.OK;
		int sampleRate = 0;
		FMOD.SPEAKERMODE speakerMode;
		int speakerCount = 0;
		result = sys.getSoftwareFormat(out sampleRate, out speakerMode, out speakerCount);
		if (result != FMOD.RESULT.OK)
		{
			Debug.LogError("OVRA FMOD: Error retreiving state from FMOD: " + result);
		}

		uint bufferLength = 0;
		int bufferCount = 0;
		result = sys.getDSPBufferSize(out bufferLength, out bufferCount);
		if (result != FMOD.RESULT.OK)
		{
			Debug.LogError("OVRA FMOD: Error retreiving state from FMOD: " + result);
		}

		if (!OSP_FMOD_Initialize(sampleRate, (int)bufferLength))
		{
			Debug.LogError("OVRA FMOD: Error initializing Oculus VR audio");
		}
		
		OSP_FMOD_SetBypass             (bypass);
		OSP_FMOD_SetGlobalScale        (globalScale);
		OSP_FMOD_SetGain               (gain);
		UpdateReflections();
	}

	void Update()
	{
		if (dirtyReflection)
		{
			UpdateReflections();
		}
	}
		
	void UpdateReflections()
	{
		OSP_FMOD_SetSimpleBoxRoomParameters(dimensions.x, dimensions.y, dimensions.z,
		                                    reflectionLeftRight.x, 	reflectionLeftRight.y, 
		                                    reflectionUpDown.x, 	reflectionUpDown.y, 
		                                    reflectionFrontBack.x, 	reflectionFrontBack.y);

		OSP_FMOD_SetLateReverberationEnabled(enableReverb);
		OSP_FMOD_SetEarlyReflectionsEnabled(enableReflections);
		dirtyReflection = false;
	}
}
