using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public interface INavigationDevice {
        NavigationInput Poll();
    }

    public struct NavigationInput {
        public readonly Vector2 MovementDelta;
        public readonly PointerEventData.FramePressState SubmitEvent;

        public NavigationInput(Vector2 movementDelta, PointerEventData.FramePressState submitEvent) {
            MovementDelta = movementDelta;
            SubmitEvent = submitEvent;
        }
    }
}
