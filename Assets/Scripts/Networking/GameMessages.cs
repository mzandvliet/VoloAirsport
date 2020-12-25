using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Padrone.Client;
using UnityEngine;

namespace RamjetAnvil.Volo.Networking {

    public static class GameMessages {
        public class RespawnPlayerRequest : IMessage {
            public ImmutableTransform Spawnpoint;
            public float InputPitch;
            public float InputRoll;

            public void Serialize(NetBuffer writer) {
                writer.Write(Spawnpoint.Position);
                writer.WriteRotation(Spawnpoint.Rotation);
                writer.Write(InputPitch);
                writer.Write(InputRoll);
            }

            public void Deserialize(NetBuffer reader) {
                Spawnpoint = new ImmutableTransform(
                    position: reader.ReadVector3(),
                    rotation: reader.ReadRotation());
                InputPitch = reader.ReadSingle();
                InputRoll = reader.ReadSingle();
            }

            public NetDeliveryMethod QosType {
                get { return NetDeliveryMethod.ReliableUnordered; }
            }
        }

        public class DespawnPlayerRequest : IMessage {
            public void Serialize(NetBuffer writer) {}
            public void Deserialize(NetBuffer reader) {}

            public NetDeliveryMethod QosType {
                get {
                    return NetDeliveryMethod.ReliableUnordered;
                }
            }
        }
    }
}
