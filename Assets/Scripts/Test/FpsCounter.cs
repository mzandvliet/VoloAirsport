using UnityEngine;

public class FpsCounter : MonoBehaviour {
    [SerializeField] private float _kernelSize = 60f;

    private float _lastRealTime;
    private float _lastAverageFrameTime;

    private bool _show;
	
    private void Awake() {
        _lastRealTime = Time.realtimeSinceStartup;
    }

	private void Update () {
	    if (Input.GetKeyDown(KeyCode.F2)) {
	        _show = !_show;
	    }

	    float deltaTime = Time.realtimeSinceStartup - _lastRealTime;
	    _lastRealTime = Time.realtimeSinceStartup;
        _lastAverageFrameTime = FilterExpMovingAverage(_lastAverageFrameTime, deltaTime, _kernelSize);
	}

//    private void OnGUI() {
//        if (_show) {
//            GUILayout.BeginArea(
//                new Rect(
//                    Screen.width - 210f,
//                    Screen.height - 110f,
//                    200f,
//                    100f),
//                GUI.skin.box);
//            {
//                GUILayout.BeginVertical();
//                {
//                    GUILayout.Label(string.Format("FPS:         {0:0.00}", 1f / _lastAverageFrameTime));
//                    GUILayout.Label(string.Format("Frame Time:  {0:0.00}", _lastAverageFrameTime * 1000f));
//                }
//                GUILayout.EndVertical();
//            }
//            GUILayout.EndArea();
//        }
//    }

    public static float FilterExpMovingAverage(float lastSample, float newest, float kernelSize) {		
		if (Mathf.Abs(lastSample - 0f) < float.Epsilon)
			return newest;

        float alpha = 2.0f / (kernelSize + 1.0f);
        float previous = lastSample;
        float current = newest * alpha + previous * (1.0f - alpha);

		return current;
	}
}
