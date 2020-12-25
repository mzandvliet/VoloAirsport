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

public class NetworkStopwatchClock : MonoBehaviour, INetworkClock {

    [SerializeField] private double _targetInterpolationSpeed = 1.0;

    private Routines.DeltaTime _pollDeltaTime;
    private Stopwatch _stopwatch;

    private long _lastFrameTimeInTicks;
    private long _deltaTimeInTicks;
    private long _frameCount;
    private long _elapsedTimeInTicks;
    private long _timeOffsetInTicks;
    private long _timeOffsetTargetInTicks;

    private int _lastSampledFrame;

    void Awake() {
        _lastSampledFrame = -1;
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
            _deltaTimeInTicks = _elapsedTimeInTicks - _lastFrameTimeInTicks;
            _lastFrameTimeInTicks = _elapsedTimeInTicks;

            InterpolateToTargetTime();

            _frameCount++;
            _lastSampledFrame = Time.renderedFrameCount;
        }
    }

    private void InterpolateToTargetTime() {
        double deltaTimeInSecs = _deltaTimeInTicks / (double)TimeSpan.TicksPerSecond;
        // Todo: is this precise enough?
        _timeOffsetInTicks = (long) Mathd.Lerp(_timeOffsetInTicks, _timeOffsetTargetInTicks, deltaTimeInSecs*_targetInterpolationSpeed);
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
    public float DeltaTime {
        get { return (float) (_deltaTimeInTicks / (double)TimeSpan.TicksPerSecond); }
    }

    public double CurrentTime {
        get {
            var totalElapsedTicks = _elapsedTimeInTicks + _timeOffsetInTicks;
            return totalElapsedTicks / (double)TimeSpan.TicksPerSecond;
        }
    }

    public long FrameCount {
        get { return _frameCount; }
    }

    public Routines.DeltaTime PollDeltaTime {
        get { return _pollDeltaTime; }
    }

    public double TargetTimeInterpolationSpeed {
        get { return _targetInterpolationSpeed; }
        set { _targetInterpolationSpeed = value; }
    }

    public void SetCurrentTime(double time) {
        var ticks = (long) (time * TimeSpan.TicksPerSecond);
        _timeOffsetInTicks = ticks - _elapsedTimeInTicks;
        _timeOffsetTargetInTicks = _timeOffsetInTicks;
    }
    
    // Todo: setting target time but only interpolating to it in next update frame introduces error
    // It might be worth doing an interpolation step here, but it needs to make *sense*
    public void SetCurrentTimeInterpolated(double time) {
        var ticks = (long)(time * TimeSpan.TicksPerSecond);
        _timeOffsetTargetInTicks = ticks - _elapsedTimeInTicks;
    }

    public double TimeScale {
        get { return enabled ? 1f : 0f; }
        set { enabled = value > 0; }
    }
}
