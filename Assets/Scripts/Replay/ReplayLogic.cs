//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reactive.Disposables;
//using System.Reactive.Linq;
//using System.Reactive.Subjects;
//using System.Text;
//using RamjetAnvil.Coroutine;
//using RamjetAnvil.Reactive;
//using RamjetAnvil.Unity.Utility;
//using RamjetAnvil.Util;
//using RamjetAnvil.Volo;
//using UnityEngine;
//
//namespace RamjetAnvil.Volo {
//
//    public class ReplayWatcherRequests {
//
//        private readonly ISubject<Unit> _speedUp; 
//        private readonly ISubject<Unit> _slowDown;
//        private readonly ISubject<Unit> _play;
//        private readonly ISubject<Unit> _pause;
//        private readonly ISubject<string> _selectReplay;
//        private readonly ISubject<string> _deleteReplay;
//        private readonly ISubject<float> _advanceTime; 
//        private readonly ISubject<float> _skipTo;
//
//        public ReplayWatcherRequests() {
//            _speedUp = new Subject<Unit>();
//            _slowDown = new Subject<Unit>();
//            _play = new Subject<Unit>();
//            _pause = new Subject<Unit>();
//            _advanceTime = new Subject<float>();
//            _selectReplay = new Subject<string>();
//            _deleteReplay = new Subject<string>();
//            _skipTo = new Subject<float>();
//        }
//
//        public ISubject<float> AdvanceTime {
//            get { return _advanceTime; }
//        }
//
//        public ISubject<Unit> SpeedUp {
//            get { return _speedUp; }
//        }
//
//        public ISubject<Unit> SlowDown {
//            get { return _slowDown; }
//        }
//
//        public ISubject<Unit> Play {
//            get { return _play; }
//        }
//
//        public ISubject<Unit> Pause {
//            get { return _pause; }
//        }
//
//        public ISubject<string> SelectReplay {
//            get { return _selectReplay; }
//        }
//
//        public ISubject<string> DeleteReplay {
//            get { return _deleteReplay; }
//        }
//
//        public ISubject<float> SkipTo {
//            get { return _skipTo; }
//        }
//    }
//
//    public class ReplayWatcherStore : IDisposable {
//
//        private readonly IObservable<ReplayData> _stateChanges;
//        private readonly CompositeDisposable _disposables;
//
//        public ReplayWatcherStore(ReplayWatcherRequests requests, IList<float> availableSpeeds) {
//            _disposables = new CompositeDisposable();
//            var initialData = new ReplayData {
//                ActiveReplay = null,
//                Replays = new List<FileReference>()
//            };
//
//            var replayDirectoryTracker = ReplayLogic.TrackReplayDirectory();
//
//            var allRequests = Observable.Merge(
//                requests.Pause.Select(_ => new {RequestType = RequestType.Pause, Data = default(object)}),
//                requests.Play.Select(_ => new {RequestType = RequestType.Play, Data = default(object)}),
//                requests.SpeedUp.Select(_ => new {RequestType = RequestType.SpeedUp, Data = default(object)}),
//                requests.SlowDown.Select(_ => new {RequestType = RequestType.SlowDown, Data = default(object)}),
//                requests.SelectReplay.Select(replayId => new {RequestType = RequestType.SelectReplay, Data = replayId as object}),
//                requests.DeleteReplay.Select(replayId => new {RequestType = RequestType.DeleteReplay, Data = replayId as object}),
//                requests.SkipTo.Select(time => new {RequestType = RequestType.SkipTo, Data = time as object}),
//                requests.AdvanceTime.Select(deltaTime => new {RequestType = RequestType.AdvanceTime, Data = deltaTime as object}),
//                replayDirectoryTracker.Select(files => new {RequestType = RequestType.UpdateFiles, Data = files as object}));
//
//            var stateChanges = allRequests.Scan(initialData, (replayData, request) => {
//                switch (request.RequestType) {
//                    case RequestType.UpdateFiles:
//                        return replayData.UpdateFiles((IList<FileReference>) request.Data);
//                    case RequestType.AdvanceTime:
//                        return replayData.AdvanceTime((float) request.Data);
//                    case RequestType.Play:
//                        return replayData.Play();
//                    case RequestType.Pause:
//                        return replayData.Pause();
//                    case RequestType.SpeedUp:
//                        return replayData.SpeedUp(availableSpeeds);
//                    case RequestType.SlowDown:
//                        return replayData.SlowDown(availableSpeeds);
//                    case RequestType.SelectReplay:
//                        return replayData.SelectReplay((string) request.Data);
//                    case RequestType.DeleteReplay:
//                        return replayData.DeleteReplay((string) request.Data);
//                    case RequestType.SkipTo:
//                        return replayData.SkipTo((float) request.Data);
//                }
//                throw new ArgumentOutOfRangeException("Unsupported type: " + request.RequestType);
//            }).Replay(1);
//            _disposables.Add(stateChanges.Connect());
//            _stateChanges = stateChanges;
//        }
//
//        public IObservable<ReplayData> StateChanges {
//            get { return _stateChanges; }
//        }
//
//        private enum RequestType {
//            Play, Pause, SpeedUp, SlowDown, SelectReplay, SkipTo,
//            UpdateFiles, AdvanceTime, DeleteReplay
//        }
//
//        public void Dispose() {
//            _disposables.Dispose();
//        }
//    }
//
//    static class ReplayLogic {
//
//        public static Lazy<string> ReplayDirectory = new Lazy<string>(() => Path.Combine(UnityFileBrowserUtil.VoloAirsportDir.Value, "replays/"));
//        public static string ReplayExtension = ".voloreplay";
//
//        public static IObservable<IList<FileReference>> TrackReplayDirectory() {
//            Directory.CreateDirectory(ReplayDirectory.Value);
//            return FileWatching.TrackDirectory(new FileWatching.WatcherSettings {
//                Directory = ReplayDirectory.Value, 
//                SearchPattern = "*" + ReplayExtension,
//                SearchOption = SearchOption.AllDirectories
//            });
//        }
//
//        public static ReplayData UpdateFiles(this ReplayData replayData, IList<FileReference> files) {
//            // Check if active replay still exists, otherwise select new replay
//            var activeReplayDoesNotExist = replayData.ActiveReplay.HasValue &&
//                                           !files.HasValue((file, replayId) => file.Name.Equals(replayId), replayData.ActiveReplay.Value.ReplayId);
//            replayData.Replays = files;
//            if (activeReplayDoesNotExist || !replayData.ActiveReplay.HasValue) {
//                if (files.Count > 0) {
//                    replayData.ActiveReplay = CreateActiveReplay(files[0]);
//                } else {
//                    replayData.ActiveReplay = null;
//                }
//            }
//            return replayData;
//        }
//
//        public static ReplayData DeleteReplay(this ReplayData replayData, string replayId) {
//            var replayFile = replayData.Replays.First(file => file.Name.Equals(replayId));
//            if (replayData.ActiveReplay.HasValue && replayData.ActiveReplay.Value.ReplayId.Equals(replayId)) {
//                replayData.ActiveReplay.Value.FileReader.BaseStream.Close();
//
//                // Select the next replay
//                var nextReplay = replayData.Replays.GetNext(replayFile);
//                var hasNextReplay = !nextReplay.Equals(replayFile);
//                if (hasNextReplay) {
//                    replayData = replayData.SelectReplay(nextReplay.Name);
//                } else {
//                    replayData.ActiveReplay = null;
//                }
//            }
//            File.Delete(replayFile.FullPath);
//            return replayData;
//        }
//
//        public static ReplayData SwapPlayState(this ReplayData replayData) {
//            if (replayData.ActiveReplay.HasValue) {
//                if (replayData.ActiveReplay.Value.IsPlaying) {
//                    return replayData.Pause();
//                }
//                return replayData.Play();
//            }
//            return replayData;
//        }
//
//        public static ReplayData Play(this ReplayData replayData) {
//            return replayData.UpdateActiveReplay(activeReplay => {
//                activeReplay.IsPlaying = true;
//                return activeReplay;
//            });
//        }
//
//        public static ReplayData Pause(this ReplayData replayData) {
//            return replayData.UpdateActiveReplay(activeReplay => {
//                activeReplay.IsPlaying = false;
//                return activeReplay;
//            });
//        }
//
//        public static ReplayData AdvanceTime(this ReplayData replayData, float deltaTime) {
//            if (replayData.ActiveReplay.HasValue && replayData.ActiveReplay.Value.IsPlaying) {
//                var activeReplay = replayData.ActiveReplay.Value;
//
//                if (activeReplay.CurrentTime < activeReplay.Length) {
//                    activeReplay.CurrentTime += deltaTime * activeReplay.Speed;
//                
//                    var replayReader = activeReplay.FileReader;
//                    var newState = activeReplay.CurrentPilotState;
//                    while (replayReader.CurrentFrame.RecordTime < activeReplay.CurrentTime && !replayReader.IsFinished) {
//                        replayReader.AdvanceToNextFrame();
//                        newState = PilotRecording.Interpolate(newState, replayReader.CurrentFrame, activeReplay.CurrentTime);
//                    }
//                    activeReplay.CurrentPilotState = newState;
//                } else {
//                    activeReplay = activeReplay.Reset();
//                }
//
//                replayData.ActiveReplay = activeReplay;
//                return replayData;
//            }
//            return replayData;
//        }
//
//        public static ReplayData SkipTo(this ReplayData replayData, float time) {
//            if (replayData.ActiveReplay.HasValue) {
//                var activeReplay = replayData.ActiveReplay.Value;
//                activeReplay.CurrentTime = time;
//
//                var replayReader = activeReplay.FileReader;
//                if (replayReader.CurrentFrame.RecordTime > time) {
//                    replayReader.Reset();    
//                    replayReader.AdvanceToNextFrame();
//                }
//                while (replayReader.CurrentFrame.RecordTime < activeReplay.CurrentTime && !replayReader.IsFinished) {
//                    var previousFrame = replayReader.CurrentFrame;
//                    replayReader.AdvanceToNextFrame();
//                    activeReplay.CurrentPilotState = PilotRecording.Interpolate(previousFrame, replayReader.CurrentFrame,
//                        time);
//                }
//
//                replayData.ActiveReplay = activeReplay;
//                return replayData;
//            }
//            return replayData;
//        }
//
//        public static ReplayData SpeedUp(this ReplayData replayData, IList<float> availableSpeeds) {
//            return replayData.UpdateActiveReplay(activeReplay => {
//                activeReplay.Speed = availableSpeeds.GetNextClamp(activeReplay.Speed);
//                return activeReplay;
//            });
//        }
//
//        public static ReplayData SlowDown(this ReplayData replayData, IList<float> availableSpeeds) {
//            return replayData.UpdateActiveReplay(activeReplay => {
//                activeReplay.Speed = availableSpeeds.GetPreviousClamp(activeReplay.Speed);
//                return activeReplay;
//            });
//        }
//
//        public static ReplayData SelectReplay(this ReplayData replayData, string id) {
//            if (replayData.ActiveReplay.HasValue) {
//                replayData.ActiveReplay.Value.FileReader.BaseStream.Close();
//            }
//
//            var newActiveReplay = replayData.Replays.Find(replay => replay.Name.Equals(id));
//            if (newActiveReplay.IsJust) {
//                replayData.ActiveReplay = CreateActiveReplay(newActiveReplay.Value);
//            } else {
//                replayData.ActiveReplay = null;
//            }
//            
//            return replayData;
//        }
//
//        public static ActiveReplay Reset(this ActiveReplay activeReplay) {
//            return CreateActiveReplay(activeReplay.ReplayId, activeReplay.FileReader);
//        }
//
//        public static ActiveReplay CreateActiveReplay(FileReference fileReference) {
//            var replayReader = new PilotReplayReader(new FileStream(fileReference.FullPath, FileMode.Open));
//            return CreateActiveReplay(fileReference.Name, replayReader);
//        }
//
//        public static ActiveReplay CreateActiveReplay(string replayId, PilotReplayReader reader) {
//            reader.Reset();
//            reader.AdvanceToNextFrame();
//            return new ActiveReplay {
//                CurrentPilotState = reader.CurrentFrame,
//                CurrentTime = 0f,
//                Speed = 1f,
//                FileReader = reader,
//                IsPlaying = false,
//                Length = reader.Header.FlightTime,
//                ReplayId = replayId,
//            };
//        }
//
//        private static ReplayData UpdateActiveReplay(this ReplayData replayData, Func<ActiveReplay, ActiveReplay> updateFn) {
//            if (replayData.ActiveReplay.HasValue) {
//                return new ReplayData {
//                    Replays = replayData.Replays,
//                    ActiveReplay = updateFn(replayData.ActiveReplay.Value)
//                };
//            }
//            return replayData;
//        }
//    }
//
//    public struct ReplayData {
//        public IList<FileReference> Replays; 
//        public ActiveReplay? ActiveReplay;
//
//    }
//
//    public struct ActiveReplay {
//        public PilotState CurrentPilotState;
//        public PilotReplayReader FileReader;
//        public double Length;
//        public double CurrentTime;
//        public float Speed;
//        public bool IsPlaying;
//        public string ReplayId;
//    }
//}
