//using System.IO;
//using RamjetAnvil.Unity.Utility;
//using UnityEngine;
//
//// Todo: Try offsetting fixedclock at app start to sync with swclock
//// Todo: Then try these new clocks in RigidbodyParty
//
//public class ClockTest : MonoBehaviour {
//
//    [SerializeField] private AbstractUnityControllableClock _renderClock;
//    [SerializeField] private FixedClock _fixedClock;
//
//    private double _timeDiff;
//    private StreamWriter csvStream;
//
//    void Awake() {
//        csvStream = new StreamWriter("F:\\Users\\Martijn\\Desktop\\timediff.csv");
//        csvStream.Write("sw_time,fix_time,diff\n");
//    }
//
//    void OnDestroy() {
//        csvStream.Close();
//    }
//
//    void Update() {
//        csvStream.Write(_renderClock.CurrentTime + "," + _fixedClock.CurrentTime + "," + (_renderClock.CurrentTime - _fixedClock.CurrentTime) + "\n");
//    }
//
//    void FixedUpdate() {
//        _timeDiff = (double)Time.time - (double)_renderClock.CurrentTime;
//
//        if (Input.GetKeyDown(KeyCode.H)) {
//            _renderClock.Pause();
//        }
//        if (Input.GetKeyDown(KeyCode.J)) {
//            _renderClock.Resume();
//        }
//    }
//
//    void OnGUI() {
//        GUILayout.Label("Time diff: " + (_renderClock.CurrentTime - _fixedClock.CurrentTime));
//    }
//
//}
