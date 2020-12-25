using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public interface ICompleteDragHandler : 
        IInitializePotentialDragHandler, 
        IDragHandler, 
        IPointerUpHandler, 
        IPointerDownHandler, 
        IDraggable {}
}
