using UnityEngine;
using System.Collections;

public class AmbientLightTest : MonoBehaviour {
	void Update () {
	    Debug.Log("Ambient Light Intensity: " + RenderSettings.ambientIntensity);
	}
}
