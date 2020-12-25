using UnityEngine;
using System.Collections;

public class DebugGui : MonoBehaviour
{
    [SerializeField] private Rigidbody _aircraftBody;

//    private void OnGUI()
//    {
//        GUILayout.BeginArea(new Rect(16f, 12f, 240f, 460f), GUI.skin.box);
//        GUILayout.BeginVertical();
//        {
//            GUILayout.Label(string.Format("Speed: {0:0.0} Km/h", _aircraftBody.velocity.magnitude * 3.6f));
//        }
//        GUILayout.EndVertical();
//        GUILayout.EndArea();
//    }
}
