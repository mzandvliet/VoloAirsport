using UnityEngine;

public class FlightStatisticsDebugGui : MonoBehaviour {
    [SerializeField] private FlightStatistics _flightStatistics;

//	private void OnGUI() {
//	    GUILayout.BeginVertical(GUI.skin.box);
//	    {
//	        GUILayout.Label("Speed: " + _flightStatistics.WorldVelocity.magnitude * 3.6f + " km/h");
//            GUILayout.Label("Glide Ratio: " + _flightStatistics.GlideRatio);
//            GUILayout.Label("AoA: " + _flightStatistics.AngleOfAttack);
//            GUILayout.Label("Altitude: " + _flightStatistics.AltitudeGround);
//	    }
//        GUILayout.EndVertical();
//	}
}
