using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Util;
using UnityEngine.Networking;

namespace RamjetAnvil.RamNet {

    [Flags]
    public enum ObjectRole : byte {
        /// <summary>
        /// When you want no one to receive what it is you're sending.
        /// </summary>
        Nobody = 0,
        /// <summary>
        /// The authority of the object. 
        /// </summary>
        Authority = 1,
        /// <summary>
        /// The owner of the object. 
        /// </summary>
        Owner = 2,
        /// <summary>
        /// Everyone that isn't the authority nor the owner A.K.A the mindless slaves.
        /// </summary>
        Others = 4
    }

    public static class ObjectRoles {
        public const ObjectRole Everyone = ObjectRole.Authority | ObjectRole.Owner | ObjectRole.Others;
        public const ObjectRole NonAuthoritive = ObjectRole.Owner | ObjectRole.Others;
        public const ObjectRole LocalInstance = ObjectRole.Owner | ObjectRole.Authority;

        public static bool IsOwner(this ObjectRole objectRole) {
            return (objectRole & ObjectRole.Owner) == ObjectRole.Owner;
        }
        public static bool IsAuthority(this ObjectRole objectRole) {
            return (objectRole & ObjectRole.Authority) == ObjectRole.Authority;
        }
        public static bool IsOther(this ObjectRole objectRole) {
            return (objectRole & ObjectRole.Others) == ObjectRole.Others;
        }

        public static void IntoList(this ObjectRole role, IList<ObjectRole> roles) {
            roles.Clear();
            if (role.IsOwner()) {
                roles.Add(ObjectRole.Owner);
            }
            if (role.IsAuthority()) {
                roles.Add(ObjectRole.Authority);
            }
            if (role.IsOther()) {
                roles.Add(ObjectRole.Others);
            }
        }
    }

    public interface IObjectMessageSender {
        void Send<TMessage>(INetworkMessage<TMessage> message, ObjectRole recipient) where TMessage : IObjectMessage;
    }
}
