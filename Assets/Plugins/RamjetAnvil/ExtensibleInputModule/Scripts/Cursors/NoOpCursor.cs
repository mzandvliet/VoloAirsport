using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public class NoOpCursor : ICursor {
        public static ICursor Default = new NoOpCursor();

        private NoOpCursor() {}

        public CursorInput Poll() {
            return new CursorInput(
                new Ray(Vector3.zero, Vector3.zero), 
                Vector2.zero, 
                PointerEventData.FramePressState.NotChanged, 
                Vector2.zero);
        }
    }
}
