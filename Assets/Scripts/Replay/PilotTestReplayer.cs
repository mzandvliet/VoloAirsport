//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RamjetAnvil.Reactive;
//using RamjetAnvil.Unity.Utility;
//using RamjetAnvil.Volo;
//using UnityEngine;
//
//public class PilotTestReplayer : MonoBehaviour {
//
//    [SerializeField] private AbstractUnityClock _clock;
//    [SerializeField] private PilotBodyParts _pilot;
//    [SerializeField] private string _replayFilePath;
//    [SerializeField] private List<float> _availableSpeeds = new List<float> {0.5f, 1f, 2f, 4f, 8f, 16f};
//
//    private ReplayData _replayData;
//
//    void Awake() {
//        var rs = _pilot.GetComponent<Wingsuit>().Rigidbodies;
//        for (int i = 0; i < rs.Count; i++) {
//            var r = rs[i];
//            r.isKinematic = true;
//        }
//
//        _replayData = new ReplayData {
//            ActiveReplay = null,
//            Replays = new List<FileReference> { new FileReference { FullPath = _replayFilePath } }
//        };
//    }
//
//    void Update() {
//        if (Input.GetKeyDown(KeyCode.F7)) {
//            _replayData = _replayData.SwapPlayState();
//        }
//
//        if (Input.GetKeyDown(KeyCode.RightArrow)) {
//            _replayData = _replayData.SpeedUp(_availableSpeeds);
//        }
//        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
//            _replayData = _replayData.SlowDown(_availableSpeeds);
//        }
//
//        if (Input.GetKeyDown(KeyCode.F9)) {
//            _replayData = _replayData.SkipTo(5f);
//        }
//
//        if (_replayData.ActiveReplay.HasValue) {
//            _replayData = _replayData.AdvanceTime(_clock.DeltaTime);    
//            PilotRecording.ApplyState(_pilot, _replayData.ActiveReplay.Value.CurrentPilotState);
//        }
//    }
//}
