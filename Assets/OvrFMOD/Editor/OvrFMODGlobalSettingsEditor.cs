/************************************************************************************
Filename    :   OvrFMODGlobalSettingsEditor.cs
Content     :   OvrFMOD Global Settings interface. 
				This script adds editor functionality to OvrFMODGlobalSettingsEditor script.
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
#define CUSTOM_LAYOUT

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(OvrFMODGlobalSettings))]

public class OvrFMODGlobalSettingsEditor : Editor
{
	// target component
	private OvrFMODGlobalSettings m_Component;

	// highlight color
	Color HColor = Color.yellow;

	// OnEnable
	void OnEnable()
	{
		m_Component = (OvrFMODGlobalSettings)target;
	}

	// OnDestroy
	void OnDestroy()
	{
	}

	// OnInspectorGUI
	public override void OnInspectorGUI()
	{
		GUI.color = Color.white;
		
		Undo.RecordObject(m_Component, "OvrFMODGlobalSettings");

		{
#if CUSTOM_LAYOUT
			Separator();
			m_Component.Bypass    = EditorGUILayout.Toggle("Bypass",   m_Component.Bypass);

			Separator();
			GUI.color = HColor;
			Label ("Global Scale (Range: 0.00001 - 10000.0f)");
			GUI.color = Color.white;
			m_Component.GlobalScale = EditorGUILayout.FloatField(" ", m_Component.GlobalScale);

			Separator();
			GUI.color = HColor;
			Label ("Gain (Range: -/+24 Decibels)");
			GUI.color = Color.white;
			m_Component.Gain   = EditorGUILayout.FloatField(" ",      m_Component.Gain);

			Separator();
			GUI.color = HColor;
			Label ("EARLY REFLECTIONS");
			GUI.color = Color.white;
			Label ("");
			m_Component.EnableReflections = EditorGUILayout.Toggle("Enable", m_Component.EnableReflections);
			m_Component.EnableReverb = EditorGUILayout.Toggle("Reverb On", m_Component.EnableReverb);
			Label ("");
			GUI.color = HColor;
			Label("Room Dimensions (Range: 0 - 230 meters)");
			GUI.color = Color.white;
			m_Component.Dimensions = EditorGUILayout.Vector3Field("",  m_Component.Dimensions);
			Label ("");
			GUI.color = HColor;
			Label("Reflection Values (Range: 0 - 0.95)");
			GUI.color = Color.white;
			m_Component.ReflectionLeftRight = EditorGUILayout.Vector2Field("Left/Right",     m_Component.ReflectionLeftRight);
			m_Component.ReflectionUpDown    = EditorGUILayout.Vector2Field("Up/Down",        m_Component.ReflectionUpDown);
			m_Component.ReflectionFrontBack = EditorGUILayout.Vector2Field("Behind/Front",   m_Component.ReflectionFrontBack);

			Separator();
/*
			// Reference GUI Layout fields
			m_Component.VerticalFOV         = EditorGUILayout.FloatField("Vertical FOV", m_Component.VerticalFOV);
			m_Component.NeckPosition 		= EditorGUILayout.Vector3Field("Neck Position", m_Component.NeckPosition);
			m_Component.UsePlayerEyeHeight  = EditorGUILayout.Toggle ("Use Player Eye Height", m_Component.UsePlayerEeHeight);
			m_Component.FollowOrientation   = EditorGUILayout.ObjectField("Follow Orientation", 
																		m_Component.FollowOrientation,
																		typeof(Transform), true) as Transform;
			m_Component.BackgroundColor 	= EditorGUILayout.ColorField("Background Color", m_Component.BackgroundColor);
			OVREditorGUIUtility.Separator();
*/

#else			 
			DrawDefaultInspector ();
#endif
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(m_Component);
		}
	}	

	// Utilities, move out of here (or copy over to other editor script)

	// Separator
	void Separator()
	{
		GUI.color = new Color(1, 1, 1, 0.25f);
		GUILayout.Box("", "HorizontalSlider", GUILayout.Height(16));
		GUI.color = Color.white;
	}

	// Label
	void Label(string label)
	{
		EditorGUILayout.LabelField(label);
	}
}
