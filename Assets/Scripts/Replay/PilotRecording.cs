using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo
{
//    public class PilotReplayReader {
//
//        private readonly BinaryReader _reader;
//        private PilotReplayHeader _header;
//
//        private PilotState _previousFrame;
//
//        private int _currentFrameIndex;
//        private PilotState _currentFrame;
//
//        public PilotReplayReader(Stream stream) {
//            _reader = new BinaryReader(new BufferedStream(stream));
//            Reset();
//        }
//
//        public PilotReplayHeader Header {
//            get { return _header; }
//        }
//
//        public Stream BaseStream {
//            get { return _reader.BaseStream; }
//        }
//
//        public void Reset() {
//            _reader.BaseStream.Position = 0;
//            _header = PilotRecording.ReadHeader(_reader);
//            _currentFrameIndex = -1;
//        }
//
//        public bool IsFinished {
//            get { return _currentFrameIndex >= _header.FrameCount - 1; }
//        }
//
//        public void SeekFrame(float time) {
//            Reset();
//            var currentTime = -1.0;
//            while (currentTime < time && !IsFinished) {
//                AdvanceToNextFrame();
//                currentTime = CurrentFrame.RecordTime;
//            }
//        }
//
//        public void AdvanceToNextFrame() {
//            var previousFrame = _currentFrame;
//            _currentFrame = PilotRecording.ReadPilotState(_reader);
//            _currentFrameIndex++;
//            _previousFrame = _currentFrameIndex <= 0 ? _currentFrame : previousFrame;
//        }
//
//        public PilotState PreviousFrame {
//            get { return _previousFrame; }
//        }
//
//        public PilotState CurrentFrame {
//            get { return _currentFrame; }
//        }
//    }
//
//    public struct PilotReplayHeader {
//        public int FrameCount;
//        public double FlightTime;
//    }
//
//    public static class PilotRecording {
//
//        public static PilotReplayHeader ReadHeader(BinaryReader stream) {
//            return new PilotReplayHeader {
//                FrameCount = stream.ReadInt32(),
//                FlightTime = stream.ReadSingle()
//            };
//        }
//
//        public static void WriteHeader(BinaryWriter writer, PilotReplayHeader header) {
//            writer.Write(header.FrameCount);
//            writer.Write(header.FlightTime);
//        }
//
//        public static PilotState ReadPilotState(BinaryReader stream) {
//            return new PilotState {
//                RecordTime = stream.ReadSingle(),
//
//                ArmLLower = ReadTransform(stream),
//                ArmLUpper = ReadTransform(stream),
//                ArmRLower = ReadTransform(stream),
//                ArmRUpper = ReadTransform(stream),
//                LegLLower = ReadTransform(stream),
//                LegLUpper = ReadTransform(stream),
//                LegRLower = ReadTransform(stream),
//                LegRUpper = ReadTransform(stream),
//
//                Torso = ReadTransform(stream),
//                Hips = ReadTransform(stream),
//
//                WingArmL = ReadTransform(stream),
//                WingArmR = ReadTransform(stream),
//                WingLegsLower = ReadTransform(stream),
//                WingLegsUpper = ReadTransform(stream),
//            };
//        }
//
//        public static ImmutableTransform ReadTransform(BinaryReader stream) {
//            return new ImmutableTransform(
//                position: ReadVector3(stream),
//                rotation: Quaternion.Euler(ReadVector3(stream)),
//                scale: Vector3.one);
//        }
//
//        public static Vector3 ReadVector3(BinaryReader stream) {
//            return new Vector3(
//                x: stream.ReadSingle(), 
//                y: stream.ReadSingle(),
//                z: stream.ReadSingle());
//        }
//
//        public static void WritePilotState(BinaryWriter writer, PilotState pilotState) {
//            writer.Write(pilotState.RecordTime);
//
//            WriteTransform(writer, pilotState.ArmLLower);
//            WriteTransform(writer, pilotState.ArmLUpper);
//            WriteTransform(writer, pilotState.ArmRLower);
//            WriteTransform(writer, pilotState.ArmRUpper);
//            WriteTransform(writer, pilotState.LegLLower);
//            WriteTransform(writer, pilotState.LegLUpper);
//            WriteTransform(writer, pilotState.LegRLower);
//            WriteTransform(writer, pilotState.LegRUpper);
//
//            WriteTransform(writer, pilotState.Torso);
//            WriteTransform(writer, pilotState.Hips);
//
//            WriteTransform(writer, pilotState.WingArmL);
//            WriteTransform(writer, pilotState.WingArmR);
//            WriteTransform(writer, pilotState.WingLegsLower);
//            WriteTransform(writer, pilotState.WingLegsUpper);
//        }
//
//        public static void WriteTransform(BinaryWriter writer, ImmutableTransform transform) {
//            WriteVector3(writer, transform.Position);
//            WriteVector3(writer, transform.Rotation.eulerAngles);
//        }
//
//        public static void WriteVector3(BinaryWriter writer, Vector3 v) {
//            writer.Write(v.x);
//            writer.Write(v.y);
//            writer.Write(v.z);
//        }
//
//        public static PilotState CaptureState(double flightTime, PilotBodyParts pilot) {
//            return new PilotState {
//                RecordTime = flightTime,
//
//                ArmLLower = pilot.ArmLLower.MakeImmutable(),
//                ArmLUpper = pilot.ArmLUpper.MakeImmutable(),
//                ArmRLower = pilot.ArmRLower.MakeImmutable(),
//                ArmRUpper = pilot.ArmRUpper.MakeImmutable(),
//                LegLLower = pilot.LegLLower.MakeImmutable(),
//                LegLUpper = pilot.LegLUpper.MakeImmutable(),
//                LegRLower = pilot.LegRLower.MakeImmutable(),
//                LegRUpper = pilot.LegRUpper.MakeImmutable(),
//
//                Torso = pilot.Torso.MakeImmutable(),
//                Hips = pilot.Hips.MakeImmutable(),
//
//                WingArmL = pilot.WingArmL.MakeImmutable(),
//                WingArmR = pilot.WingArmR.MakeImmutable(),
//                WingLegsLower = pilot.WingLegsLower.MakeImmutable(),
//                WingLegsUpper = pilot.WingLegsUpper.MakeImmutable()
//            };
//        }
//
//        public static void ApplyState(PilotBodyParts pilot, PilotState pilotState) {
//            pilot.ArmLLower.ApplyStateTransform(pilotState.ArmLLower);
//            pilot.ArmLUpper.ApplyStateTransform(pilotState.ArmLUpper);
//            pilot.ArmRLower.ApplyStateTransform(pilotState.ArmRLower);
//            pilot.ArmRUpper.ApplyStateTransform(pilotState.ArmRUpper);
//            pilot.LegLLower.ApplyStateTransform(pilotState.LegLLower);
//            pilot.LegLUpper.ApplyStateTransform(pilotState.LegLUpper);
//            pilot.LegRLower.ApplyStateTransform(pilotState.LegRLower);
//            pilot.LegRUpper.ApplyStateTransform(pilotState.LegRUpper);
//
//            pilot.Torso.ApplyStateTransform(pilotState.Torso);
//            pilot.Hips.ApplyStateTransform(pilotState.Hips);
//
//            pilot.WingArmL.ApplyStateTransform(pilotState.WingArmL);
//            pilot.WingArmR.ApplyStateTransform(pilotState.WingArmR);
//            pilot.WingLegsLower.ApplyStateTransform(pilotState.WingLegsLower);
//            pilot.WingLegsUpper.ApplyStateTransform(pilotState.WingLegsUpper);
//        }
//
//        public static PilotState Interpolate(PilotState previousState, PilotState nextState, double currentTime) {
//            currentTime = Math.Min(nextState.RecordTime, currentTime);
//            float relativeTime = (float)((currentTime - previousState.RecordTime) /
//                                         (nextState.RecordTime - previousState.RecordTime));
//            return new PilotState {
//                RecordTime = currentTime,
//
//                ArmLLower = Interpolate(previousState.ArmLLower, nextState.ArmLLower, relativeTime),
//                ArmLUpper = Interpolate(previousState.ArmLUpper, nextState.ArmLUpper, relativeTime),
//                ArmRLower = Interpolate(previousState.ArmRLower, nextState.ArmRLower, relativeTime),
//                ArmRUpper = Interpolate(previousState.ArmRUpper, nextState.ArmRUpper, relativeTime),
//                LegLLower = Interpolate(previousState.LegLLower, nextState.LegLLower, relativeTime),
//                LegLUpper = Interpolate(previousState.LegLUpper, nextState.LegLUpper, relativeTime),
//                LegRLower = Interpolate(previousState.LegRLower, nextState.LegRLower, relativeTime),
//                LegRUpper = Interpolate(previousState.LegRUpper, nextState.LegRUpper, relativeTime),
//
//                Torso = Interpolate(previousState.Torso, nextState.Torso, relativeTime),
//                Hips = Interpolate(previousState.Hips, nextState.Hips, relativeTime),
//
//                WingArmL = Interpolate(previousState.WingArmL, nextState.WingArmL, relativeTime),
//                WingArmR = Interpolate(previousState.WingArmR, nextState.WingArmR, relativeTime),
//                WingLegsLower = Interpolate(previousState.WingLegsLower, nextState.WingLegsLower, relativeTime),
//                WingLegsUpper = Interpolate(previousState.WingLegsUpper, nextState.WingLegsUpper, relativeTime),
//            };
//        }
//
//        public static ImmutableTransform Interpolate(ImmutableTransform previous, ImmutableTransform next, float lerp) {
//            return new ImmutableTransform(
//                position: Vector3.Lerp(previous.Position, next.Position, lerp),
//                rotation: Quaternion.Lerp(previous.Rotation, next.Rotation, lerp),
//                scale: Vector3.Lerp(previous.Scale, next.Scale, lerp));
//        }
//
//        private static void ApplyStateTransform(this Transform t, ImmutableTransform state) {
//            t.position = state.Position;
//            t.rotation = state.Rotation;
//        }
//    }
//
//    public struct PilotState {
//        public double RecordTime;
//
//        public ImmutableTransform ArmLUpper;
//        public ImmutableTransform ArmLLower;
//        public ImmutableTransform ArmRUpper;
//        public ImmutableTransform ArmRLower;
//        public ImmutableTransform LegLUpper;
//        public ImmutableTransform LegLLower;
//        public ImmutableTransform LegRUpper;
//        public ImmutableTransform LegRLower;
//        public ImmutableTransform Torso;
//        public ImmutableTransform Hips;
//
//        // Wings
//        public ImmutableTransform WingArmL;
//        public ImmutableTransform WingArmR;
//        public ImmutableTransform WingLegsUpper;
//        public ImmutableTransform WingLegsLower;
//    }
}
