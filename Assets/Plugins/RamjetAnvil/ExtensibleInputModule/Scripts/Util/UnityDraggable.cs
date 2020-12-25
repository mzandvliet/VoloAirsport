using System;
using UnityEngine;

namespace RamjetAnvil.InputModule
{
    public abstract class UnityDraggable : UnityHighlightable, IDraggable {
        public abstract event Action OnDragStart;
        public abstract event Dragging Dragging;
        public abstract event Action OnDragStop;
    }
}
