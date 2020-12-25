using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Guid = RamjetAnvil.Unity.Utility.Guid;

namespace RamjetAnvil.RamNet {

    public class ReplicatedObject {
        public ObjectType? Type;
        public ObjectRole Role;
        public ConnectionId OwnerConnectionId;
        public ConnectionId AuthorityConnectionId;
        public ObjectId Id;
        public readonly Guid GlobalObjectId;
        public bool IsPreExisting;
        public readonly GameObject GameObject;
        public readonly GameObjectNetworkInfo GameObjectNetworkInfo;
        public readonly ObjectMessageRouter MessageHandler;
        public readonly IReplicationConstructor ReplicationConstructor;
        public readonly IReadOnlyList<ISpawnable> Spawnables; 

        public ReplicatedObject(
            GameObject gameObject, 
            ObjectMessageRouter messageHandler, 
            IReplicationConstructor replicationConstructor, 
            IReadOnlyList<ISpawnable> spawnables) {

            GlobalObjectId = new Guid();
            GameObject = gameObject;
            MessageHandler = messageHandler;
            ReplicationConstructor = replicationConstructor;
            Spawnables = spawnables;
            GameObjectNetworkInfo = gameObject.GetComponent<GameObjectNetworkInfo>() ?? gameObject.AddComponent<GameObjectNetworkInfo>();
        }

        public void OnSpawn() {
            for (int i = 0; i < Spawnables.Count; i++) {
                Spawnables[i].OnSpawn();
            }
        }

        public void OnDespawn() {
            for (int i = 0; i < Spawnables.Count; i++) {
                Spawnables[i].OnDespawn();
            }
        }
    }
}
