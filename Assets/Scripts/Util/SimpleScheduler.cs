using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

/// <summary>
/// A very simple implementation of a scheduler.
/// It's quite useless but was easy to write and covers some use cases.
/// </summary>
public class SimpleScheduler : MonoBehaviour {

    [Dependency, SerializeField] private AbstractUnityClock _clock;

    private IList<ScheduledTask> _scheduledTasks; 

    protected void Awake() {
        _scheduledTasks = new List<ScheduledTask>();    
    }

    void Update() {
        for (int i = _scheduledTasks.Count - 1; i >= 0; i--) {
            var scheduledTask = _scheduledTasks[i];
            if (_clock.CurrentTime > scheduledTask.ScheduleTime) {
                scheduledTask.Task();
                _scheduledTasks.RemoveAt(i);
            }
        }
    }

    public void AddTask(Action t, TimeSpan after) {
        _scheduledTasks.Add(new ScheduledTask(_clock.CurrentTime + after.TotalSeconds, t));
    }

    private struct ScheduledTask {
        private readonly double _scheduleTime;
        private readonly Action _task;

        public ScheduledTask(double scheduleTime, Action task) {
            _scheduleTime = scheduleTime;
            _task = task;
        }

        public double ScheduleTime {
            get { return _scheduleTime; }
        }

        public Action Task {
            get { return _task; }
        }
    }
}
