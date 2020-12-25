using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    public class NetworkReplicator {

        private readonly IMessageSender _messageSender;
        private readonly INetworkMessagePool<BasicMessage.CreateObject> _createMessages;
        private readonly INetworkMessagePool<BasicMessage.DeleteObject> _deleteMessages;
        private readonly INetworkMessagePool<BasicMessage.ReplicatePreExistingObject> _replicatePreExistingMessages;
        
        public NetworkReplicator(IMessageSender messageSender, MessagePool messagePool) {
            _messageSender = messageSender;
            _createMessages = messagePool.GetPool<BasicMessage.CreateObject>();
            _deleteMessages = messagePool.GetPool<BasicMessage.DeleteObject>();
            _replicatePreExistingMessages = messagePool.GetPool<BasicMessage.ReplicatePreExistingObject>();
        }

        public void Delete(IList<ConnectionId> receivers, ObjectId objectId) {
            for (int i = 0; i < receivers.Count; i++) {
                var receiver = receivers[i];
                Delete(receiver, objectId);
            }
        }

        public void Delete(ConnectionId connectionId, ObjectId objectId) {
            var deleteObjectMsg = _deleteMessages.Create();
            deleteObjectMsg.Content.ObjectId = objectId;
            _messageSender.Send(connectionId, deleteObjectMsg);
        }

        public void Replicate(IList<ConnectionId> receivers, ReplicatedObject instance) {
            for (int i = 0; i < receivers.Count; i++) {
                var receiver = receivers[i];
                Replicate(receiver, instance);
            }
        }

        public void Replicate(ConnectionId receiver, ReplicatedObject instance) {
            var objectRole = ObjectRole.Nobody;
            var isOwner = receiver == instance.OwnerConnectionId;
            var isAuthority = receiver == instance.AuthorityConnectionId;
            objectRole = objectRole | (isOwner ? ObjectRole.Owner : ObjectRole.Nobody);
            objectRole = objectRole | (isAuthority ? ObjectRole.Authority : ObjectRole.Nobody);
            objectRole = objectRole | (!isOwner && !isAuthority ? ObjectRole.Others : ObjectRole.Nobody);

            Debug.Log("replicating to " + receiver + " role: " + objectRole);

            if (instance.IsPreExisting) {
                var replicatePreExistingMsg = _replicatePreExistingMessages.Create();
                replicatePreExistingMsg.Content.GlobalObjectId.CopyFrom(instance.GlobalObjectId);
                replicatePreExistingMsg.Content.NetworkObjectId = instance.Id;
                replicatePreExistingMsg.Content.ObjectRole = objectRole;
                _messageSender.Send(receiver, replicatePreExistingMsg);
            } else {
                var createObjectMsg = _createMessages.Create();
                createObjectMsg.Content.ObjectId = instance.Id;
                createObjectMsg.Content.ObjectRole = objectRole;
                createObjectMsg.Content.ObjectType = instance.Type.Value;
                createObjectMsg.Content.Position = instance.GameObject.transform.position;
                createObjectMsg.Content.Rotation = instance.GameObject.transform.rotation;

                // Add any additional messages that are required to atomically construct the object
                createObjectMsg.Content.AdditionalData.Reset();
                instance.ReplicationConstructor.SerializeInitialState(createObjectMsg.Content.AdditionalData);

                _messageSender.Send(receiver, createObjectMsg);
            }
        }
    }
}
