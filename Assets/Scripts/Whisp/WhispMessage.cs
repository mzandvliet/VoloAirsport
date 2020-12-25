using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.RamNet;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public static class WhispMessage {

        public class UpdateDirection : IOrderedObjectMessage {

            public Vector3 Position;
            public Vector3 Direction;

            public void Serialize(NetBuffer writer) {
                writer.Write(Position);
                writer.Write(Direction);
            }

            public void Deserialize(NetBuffer reader) {
                Position = reader.ReadVector3();
                Direction = reader.ReadVector3();
            }

            public NetDeliveryMethod QosType { get { return NetDeliveryMethod.ReliableUnordered; } }
        }
    }
}
