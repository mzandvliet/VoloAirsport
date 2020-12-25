//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using RamjetAnvil.Volo;
//using RamjetAnvil.Volo.Util;
//using UnityEngine;
//using File = System.IO.File;
//
//public class PilotRecorder : MonoBehaviour {
//
//    [SerializeField] private AbstractUnityClock _clock;
//    [SerializeField] private PilotBodyParts _pilot;
//    [SerializeField] private string _recordDirectory;
//
//    private bool _isRecording;
//    private double _startTime;
//    private BlockingCollection<PilotState?> _pilotStates; 
//
//    void Awake() {
//        _isRecording = false;
//        _pilotStates = new BlockingCollection<PilotState?>(new ConcurrentQueue<PilotState?>());
//    }
//
//    void Update() {
//        if (Input.GetKeyDown(KeyCode.F6)) {
//            // Stop recording
//            if (_isRecording) {
//                StopRecording();
//            }
//            // Start recording
//            else {
//                StartRecording();
//            }
//        }
//    }
//
//    void LateUpdate() {
//        if (_isRecording) {
//            var flightTime = _clock.CurrentTime - _startTime;
//            _pilotStates.Add(PilotRecording.CaptureState(flightTime, _pilot));
//        }
//    }
//
//    void OnDestroy() {
//        if (_isRecording) {
//            StopRecording();
//        }
//    }
//
//    private void StartRecording() {
//        _startTime = _clock.CurrentTime;
//        _isRecording = true;
//        StartRecording(_recordDirectory, _pilotStates);
//    }
//
//    private void StopRecording() {
//        // Signal the consumer to stop
//        _pilotStates.Add(null);
//        _isRecording = false;
//    }
//
//    private static void StartRecording(string recordDirectory, BlockingCollection<PilotState?> pilotStates) {
//        Task.Factory.StartNew(() => {
//            Debug.Log("Recording Pilot's flight");
//            var fileName = String.Format("{0:yyyy_MM_dd_-_HH_mm_ss}", DateTime.Now) + ReplayLogic.ReplayExtension;
//
//            var tempFileName = fileName + ".temp";
//            var tempFilePath = Path.Combine(recordDirectory, tempFileName);
//            var frameCount = 0;
//            var flightTime = 0.0;
//            using (var binaryWriter = new BinaryWriter(new FileStream(tempFilePath, FileMode.Create))) {
//                var isRecording = true;
//                while (isRecording) {
//                    var pilotState = pilotStates.Take();
//                    if (pilotState.HasValue) {
//                        PilotRecording.WritePilotState(binaryWriter, pilotState.Value);
//                        frameCount++;
//                        flightTime = pilotState.Value.RecordTime;
//                    } else {
//                        isRecording = false;
//                    }
//                }    
//            }
//
//            Debug.Log("frame count: " + frameCount + ", flight time: " + flightTime);
//
//            var filePath = Path.Combine(recordDirectory, fileName);
//            using (var tempFileReader = new FileStream(tempFilePath, FileMode.Open))
//            using (var fileWriter = new BinaryWriter(new FileStream(filePath, FileMode.Create))) {
//                PilotRecording.WriteHeader(fileWriter, new PilotReplayHeader { FlightTime = flightTime, FrameCount = frameCount});
//
//                tempFileReader.CopyTo(fileWriter.BaseStream);
//            }
//
//            File.Delete(tempFilePath);
//
//            Debug.Log("Pilot's flight was recorded to " + filePath + ".");
//        });
//    }
//
//}
