using System;
using UnityEngine;
using FMODUnity;

public class ProfilerUtil : MonoBehaviour {
    private bool _capturing;
    private int _frameCount;

    void Update() {
        if (Application.isEditor) {
            return;
        }

        if (_capturing) {
            _frameCount++;

            if (_frameCount >= 300) {
                End();
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.P)) {
                Begin();
            }
        }
    }

    void Begin() {
        RuntimeManager.PlayOneShot("event:/ui/forward");
        Debug.Log("Unity Profiler Started");

        UnityEngine.Profiling.Profiler.logFile = Application.dataPath + "/profilerLog_" + DateTime.Now.ToFileTimeUtc() + ".txt";
        UnityEngine.Profiling.Profiler.enableBinaryLog = true;
        UnityEngine.Profiling.Profiler.enabled = true;

        _capturing = true;
        _frameCount = 0;
    }

    void End() {
        Debug.Log("Unity Profiler Stopped");
        RuntimeManager.PlayOneShot("event:/ui/forward");

        UnityEngine.Profiling.Profiler.enabled = false;
        UnityEngine.Profiling.Profiler.enableBinaryLog = false;

        _capturing = false;
    }
}
