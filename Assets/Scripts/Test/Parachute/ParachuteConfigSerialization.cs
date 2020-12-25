using System;
using Lidgren.Network;

namespace RamjetAnvil.Volo {
    public static class ParachuteConfigSerialization {

        public static void ToByteStream(ParachuteConfig config, NetBuffer writer) {
            writer.Write(config.Id);
            writer.Write(config.Name);
            writer.Write(config.RadiusHorizontal);
            writer.Write(config.RadiusVertical);
            writer.Write(config.HeightOffset);
            writer.Write(config.Span);
            writer.Write(config.Chord);
            writer.Write(config.NumCells);
            writer.Write(config.Mass);
            writer.Write(config.RiggingAngle);
            writer.Write(config.PressureMultiplier);
            writer.Write(config.RigAttachPos);
            writer.Write(config.NumToggleControlledCells);
            writer.Write(config.RearRiserPullMagnitude);
            writer.Write(config.FrontRiserPullMagnitude);
            writer.Write(config.WeightshiftMagnitude);
            writer.Write(config.PilotWeight);
            writer.Write(config.InputGamma);
            writer.Write(config.InputSmoothing);
        }

        public static void FromByteStream(ParachuteConfig config, NetBuffer reader) {
            config.Id = reader.ReadString();
            config.Name = reader.ReadString();
            config.RadiusHorizontal = reader.ReadFloat();
            config.RadiusVertical = reader.ReadFloat();
            config.HeightOffset = reader.ReadFloat();
            config.Span = reader.ReadFloat();
            config.Chord = reader.ReadFloat();
            config.NumCells = reader.ReadInt32();
            config.Mass = reader.ReadFloat();
            config.RiggingAngle = reader.ReadFloat();
            config.PressureMultiplier = reader.ReadFloat();
            config.RigAttachPos = reader.ReadVector3();
            config.NumToggleControlledCells = reader.ReadInt32();
            config.RearRiserPullMagnitude = reader.ReadFloat();
            config.FrontRiserPullMagnitude = reader.ReadFloat();
            config.WeightshiftMagnitude = reader.ReadFloat();
            config.PilotWeight = reader.ReadFloat();
            config.InputGamma = reader.ReadFloat();
            config.InputSmoothing = reader.ReadFloat();
        }

    }
}