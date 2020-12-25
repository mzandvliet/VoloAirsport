using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Networking;
using UnityEngine;
using UnityEngine.Networking;
using Guid = RamjetAnvil.Unity.Utility.Guid;

namespace RamjetAnvil.RamNet {

    public static class BasicMessage {

        public class RequestHostInfo : IMessage {
            public double Timestamp;

            public void Serialize(NetBuffer writer) {
                writer.Write(Timestamp);
            }

            public void Deserialize(NetBuffer reader) {
                Timestamp = reader.ReadFloat();
            }
            public NetDeliveryMethod QosType { get { return NetDeliveryMethod.ReliableUnordered; } }
        }

        public class HostInfoResponse : IMessage {
            public double SenderTimestamp;
            public ushort PlayerCount;

            public void Serialize(NetBuffer writer) {
                writer.Write(SenderTimestamp);
                writer.Write(PlayerCount);
            }

            public void Deserialize(NetBuffer reader) {
                SenderTimestamp = reader.ReadDouble();
                PlayerCount = reader.ReadUInt16();
            }

            public NetDeliveryMethod QosType { get { return NetDeliveryMethod.ReliableUnordered; } }
        }

        public class CreateObject : IMessage {
            public ObjectType ObjectType;
            public ObjectId ObjectId;
            public ObjectRole ObjectRole;
            public Vector3 Position;
            public Quaternion Rotation;
            public NetBuffer AdditionalData;

            public CreateObject() {
                AdditionalData = new NetBuffer();
            }

            public void Serialize(NetBuffer writer) {
                writer.Write(ObjectType);
                writer.Write(ObjectId);
                writer.Write((byte) ObjectRole);
                writer.Write(Position);
                writer.WriteRotation(Rotation);
                var additionalDataSize = (uint) AdditionalData.WriterPosition();
                writer.WriteVariableUInt32(additionalDataSize);
                AdditionalData.ResetReader();
                writer.Write(AdditionalData);
            }

            public void Deserialize(NetBuffer reader) {
                ObjectType = reader.ReadObjectType();
                ObjectId = reader.ReadObjectId();
                ObjectRole = (ObjectRole) reader.ReadByte();
                Position = reader.ReadVector3();
                Rotation = reader.ReadRotation();
                var additionalDataSize = reader.ReadVariableUInt32();
                AdditionalData.Reset();
                reader.ReadInto(AdditionalData, (int) additionalDataSize);
            }

            public NetDeliveryMethod QosType {
                get { return NetDeliveryMethod.ReliableSequenced; }
            }
        }

        public class ReplicatePreExistingObject : IMessage {
            public Guid GlobalObjectId;
            public ObjectId NetworkObjectId;
            public ObjectRole ObjectRole;

            public ReplicatePreExistingObject() {
                GlobalObjectId = new Guid();
            }

            public void Serialize(NetBuffer writer) {
                GlobalObjectId.WriteTo(writer);
                writer.Write(NetworkObjectId);
                writer.Write((byte) ObjectRole);
            }

            public void Deserialize(NetBuffer reader) {
                GlobalObjectId.ReadFrom(reader);
                NetworkObjectId = reader.ReadObjectId();
                ObjectRole = (ObjectRole) reader.ReadByte();
            }

            public NetDeliveryMethod QosType { get { return NetDeliveryMethod.ReliableOrdered; } }
        }

        public class DeleteObject : IMessage {
            public ObjectId ObjectId;

            public void Serialize(NetBuffer writer) {
                writer.Write(ObjectId);
            }

            public void Deserialize(NetBuffer reader) {
                ObjectId = reader.ReadObjectId();
            }

            public NetDeliveryMethod QosType {
                get { return NetDeliveryMethod.ReliableSequenced; }
            }
        }

        public class Ping : IMessage {
            public ushort FrameId;

            public void Serialize(NetBuffer writer) {
                writer.Write(FrameId);
            }

            public void Deserialize(NetBuffer reader) {
                FrameId = reader.ReadUInt16();
            }

            public NetDeliveryMethod QosType {
                get { return NetDeliveryMethod.Unreliable; }
            }
        }

        public class Pong : IMessage {
            public ushort FrameId;
            public double Timestamp;
            public double FixedTimestamp;

            public void Serialize(NetBuffer networkWriter) {
                networkWriter.Write(FrameId);
                networkWriter.Write(Timestamp);
                networkWriter.Write(FixedTimestamp);
            }

            public void Deserialize(NetBuffer networkReader) {
                FrameId = networkReader.ReadUInt16();
                Timestamp = networkReader.ReadDouble();
                FixedTimestamp = networkReader.ReadDouble();
            }

            public NetDeliveryMethod QosType {
                get { return NetDeliveryMethod.Unreliable; }
            }
        }

        public class ToObject : IMessage {
            public ObjectId ReceiverId;

            public void Serialize(NetBuffer networkWriter) {
                networkWriter.Write(ReceiverId);
            }

            public NetDeliveryMethod QosType { get; set; }

            public void Deserialize(NetBuffer networkReader) {
                ReceiverId = networkReader.ReadObjectId();
            }
        }

        public struct ObjectMessageTypeId {
            public readonly long Value;

            public ObjectMessageTypeId(long value) {
                Value = value;
            }

            public bool Equals(ObjectMessageTypeId other) {
                return Value == other.Value;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ObjectMessageTypeId && Equals((ObjectMessageTypeId) obj);
            }

            public override int GetHashCode() {
                return (int) Value;
            }

            public override string ToString() {
                return string.Format("Id: {0}", Value);
            }
        }
    }
}
