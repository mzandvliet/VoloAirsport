using System;
using System.Collections.Generic;
using UnityEngine;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;

public class FixedUnityCoroutineScheduler : MonoBehaviour, ICoroutineScheduler {
    [Dependency("fixedClock"), SerializeField] private AbstractUnityClock _fixedClock;

    private long _lastRunFrame;
    private CoroutineScheduler _scheduler;

    public IAwaitable Run(IEnumerator<WaitCommand> fibre) {
        if (_scheduler == null) {
            _scheduler = new CoroutineScheduler();
        }

        // Prevent any newly scheduled routines from
        // being updated immediately
        FixedUpdate();

        return _scheduler.Run(fibre);
    }

    void Awake() {
        if (_scheduler == null) {
            _scheduler = new CoroutineScheduler();
        }

        _lastRunFrame = -1;
    }

    void FixedUpdate() {
        if (_lastRunFrame < _fixedClock.FrameCount) {
            _scheduler.Update(_fixedClock.FrameCount, _fixedClock.CurrentTime);
            _lastRunFrame = _fixedClock.FrameCount;
        }
    }

    public AbstractUnityClock Clock {
        get { return _fixedClock; }
        set { _fixedClock = value; }
    }
}
