using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule
{
    public class RayPointerEventData : PointerEventData {
        public RayPointerEventData(EventSystem eventSystem) : base(eventSystem) {}
        public Ray Ray { get; set; }
    }
}
