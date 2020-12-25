//using System;
//using System.Collections.Generic;
//using System.Reactive.Linq;
//using RamjetAnvil.DependencyInjection;
//using RamjetAnvil.GameObjectFactories;
//using RamjetAnvil.Unity.Utility;
//using RamjetAnvil.Volo;
//using RxUnity.Schedulers;
//using UnityEngine;
//
//public class ReplayWatcher : MonoBehaviour {
//
//    [Dependency, SerializeField] private AbstractUnityClock _clock;
//    [Dependency, SerializeField] private PilotSpawner _pilotSpawner;
//
//    [SerializeField] private List<float> _availableSpeeds = new List<float>{0.5f, 1f, 2f, 4f, 8f, 16f};
//    [SerializeField] private ReplayWatcherGui _replayWatcherGui;
//
//    private PilotBodyParts _pilot;
//    private ReplayWatcherStore _store;
//    private ReplayWatcherRequests _requests;
//
//    void Awake() {
//        _pilot = _pilotSpawner.PilotFactory
//            .Adapt(go => {
//                go.name = "ReplayPilot";
//
//                var rs = go.GetComponent<Wingsuit>().Rigidbodies;
//                for (int i = 0; i < rs.Count; i++) {
//                    var r = rs[i];
//                    r.isKinematic = true;
//                }
//
//                var cameraMountSwitcher = go.GetComponentInHierarchy<PilotCameraMountSwitcher>();
//                cameraMountSwitcher.enabled = true;
//                cameraMountSwitcher.SwitchToActiveGameMount();
//
//                go.FindInChildren("TrajectoryVisualizer").Destroy();
//                go.FindInChildren("_Visualizers").Destroy();
//                go.GetComponentInHierarchy<AerodynamicsVisualizationManager>().Destroy();
//            })
//            .Instantiate()
//            .GetComponent<PilotBodyParts>();
//
//        _requests = new ReplayWatcherRequests();
//        _store = new ReplayWatcherStore(_requests, _availableSpeeds);
//
//        _replayWatcherGui.ReplayState = _store.StateChanges;
//        _replayWatcherGui.Requests = _requests;
//
//        _store.StateChanges
//            .ObserveOn(UnityThreadScheduler.MainThread)
//            .Subscribe(replayData => {
//            if (replayData.ActiveReplay.HasValue) {
//                PilotRecording.ApplyState(_pilot, replayData.ActiveReplay.Value.CurrentPilotState);
//            }
//        });
//    }
//    
//    void Update() {
//        _requests.AdvanceTime.OnNext(_clock.DeltaTime);
//    }
//
//    void OnDestroy() {
//        _store.Dispose();
//    }
//}
