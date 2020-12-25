using System;
using UnityEngine;

namespace RamjetAnvil.InputModule {
    public interface IDraggable {
        event Action OnDragStart;
        event Dragging Dragging;
        event Action OnDragStop;
    }
}
