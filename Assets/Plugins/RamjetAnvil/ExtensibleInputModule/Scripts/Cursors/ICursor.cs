using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public interface ICursor {
        CursorInput Poll();
    }

    public struct CursorInput {
        public readonly Ray Ray;
        /// <summary>
        /// This field is used for compatibility reasons with Unity's UI system
        /// </summary>
        public readonly Vector2 ScreenPosition;
        public readonly PointerEventData.FramePressState SubmitEvent;
        public readonly Vector2 ScrollDelta;

        public CursorInput(Ray ray, Vector2 screenPosition, PointerEventData.FramePressState submitEvent, Vector2 scrollDelta) {
            Ray = ray;
            SubmitEvent = submitEvent;
            ScrollDelta = scrollDelta;
            ScreenPosition = screenPosition;
        }
    }
}
