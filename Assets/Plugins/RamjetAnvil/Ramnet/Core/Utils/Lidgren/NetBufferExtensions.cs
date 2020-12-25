using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Networking;
using RamjetAnvil.Util;
using UnityEngine;
using Guid = RamjetAnvil.Unity.Utility.Guid;

namespace RamjetAnvil.RamNet {

    public static class NetBufferExtensions {

        public static void Reset(this NetBuffer buffer) {
            buffer.Position = 0; 
            buffer.LengthBits = 0;
        }

        public static void ResetReader(this NetBuffer buffer) {
            buffer.Position = 0;
        }

        public static void ResetWriter(this NetBuffer buffer) {
            buffer.LengthBits = 0;
        }

        public static int ReaderPosition(this NetBuffer buffer) {
            return buffer.PositionInBytes;
        }

        public static int WriterPosition(this NetBuffer buffer) {
            return buffer.LengthBytes;
        }

        public static void ReadFrom(this Guid guid, NetBuffer reader) {
            reader.ReadBytes(guid.Value, 0, guid.Length);
        }

        public static void WriteTo(this Guid guid, NetBuffer writer) {
            writer.Write(guid.Value);
        }

        public static void WriteRotation(this NetBuffer writer, Quaternion rotation) {
            Compression.WriteRotation(writer, Compression.CompressRotation(rotation));
        }

        public static Quaternion ReadRotation(this NetBuffer reader) {
            return Compression.ReadRotation(reader).ToQuaternion();
        }
        
        public static void Write(this NetBuffer writer, MessageType messageType) {
            writer.WriteVariableUInt32(messageType.Value);
        }

        public static MessageType ReadMessageType(this NetBuffer reader) {
            return new MessageType(reader.ReadVariableUInt32());
        }

        public static void Write(this NetBuffer writer, ObjectType objectId) {
            writer.WriteVariableUInt32(objectId.Value);
        }

        public static ObjectType ReadObjectType(this NetBuffer reader) {
            return new ObjectType(reader.ReadVariableUInt32());
        }

        public static void Write(this NetBuffer writer, ObjectId objectId) {
            writer.WriteVariableUInt32(objectId.Value);
        }

        public static ObjectId ReadObjectId(this NetBuffer reader) {
            return new ObjectId(reader.ReadVariableUInt32());
        }

        public static void Write(this NetBuffer writer, SequenceNumber sequenceNumber) {
            writer.Write(sequenceNumber.Value);
        }

        public static SequenceNumber ReadSequenceNumber(this NetBuffer reader) {
            return new SequenceNumber(reader.ReadUInt16());
        }

        public static void Write(this NetBuffer writer, PeerEndpoint endpoint) {
            var hasInternalAddress = endpoint.Internal.HasValue;
            writer.Write(hasInternalAddress);
            if (hasInternalAddress) {
                writer.Write(endpoint.Internal.Value);
            }
            writer.Write(endpoint.External);
        }

        public static PeerEndpoint ReadPeerEndpoint(this NetBuffer reader) {
            var hasInternalAddress = reader.ReadBoolean();
            PeerEndpoint endpoint;
            if (hasInternalAddress) {
                endpoint = new PeerEndpoint(reader.ReadIpv4Endpoint(), reader.ReadIpv4Endpoint());
            } else {
                endpoint = new PeerEndpoint(reader.ReadIpv4Endpoint());
            }
            return endpoint;
        }

        public static void Write(this NetBuffer writer, Ipv4Endpoint endpoint) {
            writer.Write(endpoint.Address);
            writer.Write(endpoint.Port);
        }

        public static Ipv4Endpoint ReadIpv4Endpoint(this NetBuffer reader) {
            return new Ipv4Endpoint(
                address: reader.ReadString(), 
                port: reader.ReadUInt16());
        }

        public static void Write<T>(this NetBuffer writer, IList<T> list, Action<NetBuffer, T> serializeElement) {
            writer.WriteVariableInt32(list.Count);
            for (int i = 0; i < list.Count; i++) {
                var element = list[i];
                serializeElement(writer, element);
            }
        }

        public static void ReadList<T>(this NetBuffer reader, IList<T> list, Func<NetBuffer, T> deserializeElement) {
            list.Clear();
            var listSize = reader.ReadVariableInt32();
            for (int i = 0; i < listSize; i++) {
                list.Add(deserializeElement(reader));
            }
        }

        public static void ReadInto(this NetBuffer from, NetBuffer to, int byteCount) {
            to.EnsureBufferSize(byteCount * 8);
            from.ReadBytes(to.Data, offset: 0, numberOfBytes: byteCount);
            to.LengthBytes = byteCount;
            to.ResetReader();
        }

        public static void SkipBytes(this NetBuffer reader, long bytes) {
            for (int i = 0; i < bytes; i++) {
                reader.ReadByte();
            }
        }

        public static string ToHexString(this NetBuffer buffer) {
            return "[" + BitConverter.ToString(buffer.Data, 0, buffer.LengthBytes) + "]" + "size(" + buffer.LengthBytes + ")";
        }

        public static string ToHexString(this byte[] array, int startIndex, int length) {
            return "[" + BitConverter.ToString(array, startIndex, length) + "]" + "size(" + length + ")";
        }
    }
}
