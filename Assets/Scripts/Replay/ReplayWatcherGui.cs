//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Coherent.UI.Binding;
//using RamjetAnvil.DependencyInjection;
//using RamjetAnvil.Unity.Utility;
//using RamjetAnvil.Volo;
//using UnityEngine;
//
//public class ReplayWatcherGui : MonoBehaviour {
//
//    [SerializeField] private CoherentUIView _view;
//
//    [SerializeField] private ReplayWatcherRequests _requests;
//    [SerializeField] private IObservable<ReplayData> _replayState; 
//
//    private ReplayViewData _replayViewViewData;
//
//    void Start() {
//        _replayViewViewData = new ReplayViewData {
//            AvailableReplays = new List<string>(),
//            ActiveReplay = null,
//        };
//
//        _view.OnReadyForBindings += (id, path, frame) => {
//            var view = _view.View;
//            view.BindCall("play", new Action(() => _requests.Play.OnNext(Unit.Default)));
//            view.BindCall("pause", new Action(() => _requests.Pause.OnNext(Unit.Default)));
//            view.BindCall("speedUp", new Action(() => _requests.SpeedUp.OnNext(Unit.Default)));
//            view.BindCall("slowDown", new Action(() => _requests.SlowDown.OnNext(Unit.Default)));
//            view.BindCall("selectReplay", new Action<string>(replayId => _requests.SelectReplay.OnNext(replayId)));
//            view.BindCall("deleteReplay", new Action<string>(replayId => _requests.DeleteReplay.OnNext(replayId)));
//            view.BindCall("skipTo", new Action<float>(time => _requests.SkipTo.OnNext(time)));
//
//            _replayState.Subscribe(replayData => {
//                _replayViewViewData.AvailableReplays.Clear();
//                for (int i = 0; i < replayData.Replays.Count; i++) {
//                    var replay = replayData.Replays[i];
//                    _replayViewViewData.AvailableReplays.Add(replay.Name);
//                }
//
//                if (replayData.ActiveReplay.HasValue) {
//                    var activeReplay = replayData.ActiveReplay.Value;
//                    _replayViewViewData.ActiveReplay = new ActiveReplay {
//                        Id = activeReplay.ReplayId,
//                        CurrentTime = activeReplay.CurrentTime,
//                        Length = activeReplay.Length,
//                        Speed = activeReplay.Speed,
//                        IsPlaying = activeReplay.IsPlaying
//                    };
//                }
//                else {
//                    _replayViewViewData.ActiveReplay = null;
//                }
//
//                view.TriggerEvent("updateState", _replayViewViewData);
//            });
//        };
//    }
//
//    [Dependency]
//    public ReplayWatcherRequests Requests {
//        set { _requests = value; }
//    }
//
//    [Dependency]
//    public IObservable<ReplayData> ReplayState {
//        set { _replayState = value; }
//    }
//
//    [CoherentType(PropertyBindingFlags.Explicit)]
//    public class ReplayViewData {
//        [CoherentProperty("availableReplays")]
//        public List<string> AvailableReplays;
//
//        [CoherentProperty("activeReplay")]
//        public ActiveReplay? ActiveReplay;
//    }
//
//    [CoherentType(PropertyBindingFlags.Explicit)]
//    public struct ActiveReplay {
//        [CoherentProperty("id")]
//        public string Id;
//
//        [CoherentProperty("speed")]
//        public float Speed;
//
//        [CoherentProperty("isPlaying")]
//        public bool IsPlaying;
//
//        [CoherentProperty("length")]
//        public float Length;
//
//        [CoherentProperty("currentTime")]
//        public float CurrentTime;
//    }
//
//}
