using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RamjetAnvil.GameObjectFactories;
using RamjetAnvil.RamNet;
using UnityEngine;

namespace RamjetAnvil.Volo.Networking {
    public static class ReplicationObjects {
        public static readonly ObjectType Pilot = new ObjectType(0);
        public static readonly ObjectType Whisp = new ObjectType(1);

        public static readonly IReadOnlyDictionary<ObjectType, Func<GameObject>> Factories =
            new Dictionary<ObjectType, Func<GameObject>> {
                {Pilot, GameObjectFactory.FromPrefab("Prefabs/Pilot", turnOff: true)},
                {Whisp, GameObjectFactory.FromPrefab("Prefabs/Whisp", turnOff: true)}
            }.ToImmutableDictionary();
    }
}
