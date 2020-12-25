using UnityEngine;
using System.Threading;
using RamjetAnvil.Unity.Utility;
using StringLeakTest;

/* Todo: It'd be nice to have a detailed measure of headroom, but by just measuring deltatime I don't know it. */
public class PerformanceGui : MonoBehaviour {
    private float _addedDeltaTimeFramerateFactor;

    private MutableString _deltaTimeErrorString;
    private float _lastTime;
    private float _deltaTime;
    private float _smoothDeltaTime;
    private float _additionalDeltaTime;
    private int _fixedFrameCount;
    private int _fixedFrameCountGui;

    private CircularBuffer<float> _deltaTimes;

    private void Start() {
        Application.targetFrameRate = 60;

        _deltaTimeErrorString = new MutableString(Application.targetFrameRate);
        _deltaTimes = new CircularBuffer<float>(Application.targetFrameRate);
        _deltaTime = 1f / Application.targetFrameRate;
    }

    private void Update() {
        _fixedFrameCountGui = _fixedFrameCount;
        _fixedFrameCount = 0;

        MeasureDeltaTime();
        ApplyForcedFrameTime();
    }

    private void FixedUpdate() {
        _fixedFrameCount++;
    }

    private void ApplyForcedFrameTime() {
        _additionalDeltaTime = 1f / Application.targetFrameRate * _addedDeltaTimeFramerateFactor;
        if (_additionalDeltaTime > 0f) {
            Thread.Sleep(Mathf.RoundToInt(_additionalDeltaTime * 1000f));
        }
    }

    private void MeasureDeltaTime() {
        float time = Time.realtimeSinceStartup;
        _deltaTime = time - _lastTime;
        _lastTime = time;
        _deltaTimes.Add(_deltaTime);
        _smoothDeltaTime = Mathf.Lerp(_smoothDeltaTime, _deltaTime, Time.unscaledDeltaTime * 5f);
    }

    private void OnGUI() {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250f));
        {
            DrawConfigGui();
            GUILayout.Space(16f);
            DrawStatisticsGui();
        }
        GUILayout.EndVertical();
    }

    private void DrawConfigGui() {
        GUILayout.Label("Target Framerate: " + Application.targetFrameRate);
        int newTargetFramerate = Mathf.RoundToInt(GUILayout.HorizontalSlider(Application.targetFrameRate, -1f, 120f));
        if (newTargetFramerate != Application.targetFrameRate) {
            Application.targetFrameRate = newTargetFramerate;
        }

        GUILayout.Label("Added deltaTime factor: " + _addedDeltaTimeFramerateFactor + "x");
        _addedDeltaTimeFramerateFactor = GUILayout.HorizontalSlider(_addedDeltaTimeFramerateFactor, 0f, 8f);
        GUILayout.Label("Added deltaTime: " + _additionalDeltaTime);

    }

    private void DrawStatisticsGui() {
        GUILayout.Label("Framerate: " + (1f / _smoothDeltaTime));

        const float epsilon = 0.001f;
        float maxDeltaTime = 1f / Application.targetFrameRate + epsilon;
        GUILayout.Label("Performance Ratio: " + maxDeltaTime / _deltaTime);

        GUILayout.Label("Fixed Frames: " + _fixedFrameCountGui);

        GUI.color = Color.red;
        _deltaTimeErrorString.Clear();
        for (int i = 0; i < _deltaTimes.Count; i++) {
            _deltaTimeErrorString.Append(_deltaTimes[i] > maxDeltaTime ? "|" : " ");
        }
        GUILayout.Label(_deltaTimeErrorString.ToString());
        GUI.color = Color.white;
    }
}
