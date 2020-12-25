using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.RamNet {
    public static class ReplicationObjectDatabaseExtensions {

        private static readonly IList<ReplicatedObject> ObjectCache = new List<ReplicatedObject>();
        public static IList<ReplicatedObject> FindObjects(this IReplicatedObjectDatabase db, 
            ObjectType type, ObjectRole role = ObjectRoles.Everyone) {

            ObjectCache.Clear();
            return db.FindObjects(type, ObjectCache, role);
        }

        public static Maybe<ReplicatedObject> FindObject(this IReplicatedObjectDatabase db, 
            ObjectType type, ObjectRole role = ObjectRoles.Everyone) {

            ObjectCache.Clear();
            db.FindObjects(type, ObjectCache, role);
            if (ObjectCache.Count > 0) {
                return Maybe.Just(ObjectCache[0]);
            }
            return Maybe<ReplicatedObject>.Nothing;
        }

        public static IList<ReplicatedObject> FilterOwner(
            this IList<ReplicatedObject> objects,
            ConnectionId connectionId) {

            for (int i = objects.Count - 1; i >= 0; i--) {
                var @object = objects[i];
                if (@object.OwnerConnectionId != connectionId) {
                    objects.RemoveAt(i);
                }
            }
            return objects;
        }

        public static IList<ReplicatedObject> FilterAuthority(
            this IList<ReplicatedObject> objects,
            ConnectionId connectionId) {

            for (int i = objects.Count - 1; i >= 0; i--) {
                var @object = objects[i];
                if (@object.AuthorityConnectionId != connectionId) {
                    objects.RemoveAt(i);
                }
            }
            return objects;
        }
    }
}
