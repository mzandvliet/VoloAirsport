using System;
using System.Collections.Generic;
using UnityEngine;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;

public class UnityCoroutineScheduler : MonoBehaviour, ICoroutineScheduler {
    [Dependency, SerializeField] private AbstractUnityClock _clock;

    private long _lastRunFrame;
    private CoroutineScheduler _scheduler;

    public IAwaitable Run(IEnumerator<WaitCommand> fibre) {
        if (_scheduler == null) {
            _scheduler = new CoroutineScheduler();
        }

        // Prevent any newly scheduled routines from
        // being updated immediately
        Update();

        return _scheduler.Run(fibre);
    }

    void Awake() {
        if (_scheduler == null) {
            _scheduler = new CoroutineScheduler();
        }

        _lastRunFrame = -1;
    }

    void Update() {
        if (_lastRunFrame < _clock.FrameCount) {
            _scheduler.Update(_clock.FrameCount, _clock.CurrentTime);
            _lastRunFrame = _clock.FrameCount;
        }
    }

    public AbstractUnityClock Clock {
        set { _clock = value; }
    }
}
