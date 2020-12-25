using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using UnityEngine;

public class UnityCoRoutineSchedulerTest : MonoBehaviour {
    [SerializeField] private UnityCoroutineScheduler _scheduler;
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            _scheduler.Run(TestRoutine());
        }
    }

    IEnumerator<WaitCommand> TestRoutine() {
        Debug.Log("time " + Time.frameCount);
        yield return WaitCommand.WaitForNextFrame;
        Debug.Log("time " + Time.frameCount);
    }
}
