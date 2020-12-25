using System;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class FixedClock : AbstractUnityClock {
    [SerializeField] private double _targetInterpolationSpeed = 1.0;

    private double _deltaTime;
    private long _frameCount;
    private Routines.DeltaTime _pollDeltaTime;

    private double _currentTime;
    private double _targetTimeOffset;
    private double _timeOffset;

    void Awake() {
        _deltaTime = Time.fixedDeltaTime;
        _timeOffset = 0;
        _frameCount = 0;
        _pollDeltaTime = () => TimeSpan.FromSeconds(_deltaTime);
    }

    void FixedUpdate() {
        _deltaTime = Time.fixedDeltaTime;
        _timeOffset = Mathd.Lerp(_timeOffset, _targetTimeOffset, Time.fixedDeltaTime * _targetInterpolationSpeed); // Todo: Is linear instead of exponential lerp better?
        _currentTime += _deltaTime;
        _frameCount++;
    }

    public override float DeltaTime {
        get { return (float) _deltaTime; }
    }

    public override double CurrentTime {
        get { return _currentTime + _timeOffset; }
    }

    public override long FrameCount {
        get { return _frameCount; }
    }

    public override Routines.DeltaTime PollDeltaTime {
        get { return _pollDeltaTime; }
    }

    public override double TimeScale {
        get { return Time.timeScale; }
        set { Time.timeScale = (float) value; }
    }

    // Todo: make pretty plz
    public static void PausePhysics() {
        Time.timeScale = 0f;
    }

    public static void ResumePhysics() {
        Time.timeScale = 1f;
    }
}
