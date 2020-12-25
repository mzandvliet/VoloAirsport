using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public abstract class RamjetInputModule : PointerInputModule {
        public abstract void ReContextualize(GameObject firstObject);
    }
}
