using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ClockSyncDebugGui : MonoBehaviour {

    [SerializeField] private AbstractUnityClock _gameClock;
    [SerializeField] private AbstractUnityClock _fixedClock;

    void OnGUI() {
        GUILayout.Label("Game clock: " + _gameClock.CurrentTime);
        GUILayout.Label("Fixed clock: " + _fixedClock.CurrentTime);
    }
}
