using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Guid = RamjetAnvil.Unity.Utility.Guid;
using Object = UnityEngine.Object;

namespace RamjetAnvil.RamNet {

    public static class PreExistingObjects {
        public static IDictionary<Guid, GameObject> FindAll() {
            return Object.FindObjectsOfType<GameObjectNetworkId>()
                .ToDictionary(go => go.Id, go => go.gameObject);
        } 
    }
}
