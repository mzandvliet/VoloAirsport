using System;
using System.Diagnostics;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

/* Pong message read timing is important. If you receive a set of object state messages and a pong, and
 * you don't process the pong first, you get undefined timing differences within that frame.
 * 
 * We have a frame-racing problem with networked local game instances that have near-0 network latency between them. 
 */

public class StopwatchClock : AbstractUnityClock {

    private Routines.DeltaTime _pollDeltaTime;
    private Stopwatch _stopwatch;

    private long _lastFrameTimeInTicks;
    private long _deltaTimeInTicksScaled;
    private long _currentTimeScaled;
    private long _frameCount;
    private long _elapsedTimeInTicks;
    private double _timeScale;

    private int _lastSampledFrame;

    void Awake() {
        _lastSampledFrame = -1;
        _timeScale = 1f;
        _stopwatch = _stopwatch ?? new Stopwatch();
        _pollDeltaTime = () => TimeSpan.FromSeconds(DeltaTime);

#if UNITY_EDITOR
        // Handles whatever
        UnityEditor.EditorApplication.playmodeStateChanged += () => {
//            UnityEngine.Debug.Log("Clock pausing because editor state changed: " + UnityEditor.EditorApplication.isPaused);
            if (UnityEditor.EditorApplication.isPaused) {
                _stopwatch.Stop();
            }
            else {
                _stopwatch.Start();
            }
        };
#endif
    }

    void FixedUpdate() {
        Tick();
    }

    void Update() {
        Tick();
    }

    void Tick() {
        if (_lastSampledFrame != Time.renderedFrameCount) {
            _elapsedTimeInTicks = _stopwatch.ElapsedTicks;
            _deltaTimeInTicksScaled = (long) ((_elapsedTimeInTicks - _lastFrameTimeInTicks) * _timeScale);
            _currentTimeScaled += _deltaTimeInTicksScaled;
            _lastFrameTimeInTicks = _elapsedTimeInTicks;

            _frameCount++;
            _lastSampledFrame = Time.renderedFrameCount;
        }
    }

    void OnEnable() {
        if (_stopwatch == null) {
            _stopwatch = new Stopwatch();
        }
        _stopwatch.Start();
    }

    void OnDisable() {
        if (_stopwatch == null) {
            _stopwatch = new Stopwatch();
        }
        _stopwatch.Stop();
        _deltaTimeInTicksScaled = 0;
    }

    // Handles application focus gain and loss
    void OnApplicationPause(bool isPaused) {
        UnityEngine.Debug.Log("Clock pausing because OnApplicationPause called: " + isPaused);
        if (_stopwatch == null) {
            _stopwatch = new Stopwatch();
        }

        if (isPaused) {
            _stopwatch.Stop();
        }
        else {
            _stopwatch.Start();
        }
    }

    void OnDestroy() {
        _stopwatch.Stop();
    }

    /// <summary>
    /// Note: DeltaTime is entirely based on frame latency, and not at all related to network synced time.
    /// </summary>
    public override float DeltaTime {
        get { return (float) (_deltaTimeInTicksScaled / (double)TimeSpan.TicksPerSecond); }
    }

    public override double CurrentTime {
        get {
            return _currentTimeScaled / (double)TimeSpan.TicksPerSecond;
        }
    }

    public override long FrameCount {
        get { return _frameCount; }
    }

    public override Routines.DeltaTime PollDeltaTime {
        get { return _pollDeltaTime; }
    }

    public override double TimeScale {
        get {
            return _timeScale;
        }
        set {
            enabled = value > 0;
            _timeScale = value;
        }
    }
}
