using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.RamNet {
    public interface IReplicatedObjectDatabase {
        event Action<ReplicatedObject> ObjectAdded;
        event Action<ReplicatedObject> ObjectRemoved;

        IList<ReplicatedObject> FindObjects(ObjectType type, IList<ReplicatedObject> results, ObjectRole role = ObjectRoles.Everyone);
        Maybe<ReplicatedObject> Find(ObjectId id);
    }
}
