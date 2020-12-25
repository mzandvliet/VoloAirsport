using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {
    public class NoOpNavigation : INavigationDevice {
        public static readonly INavigationDevice Default = new NoOpNavigation();

        private NoOpNavigation() {}

        public NavigationInput Poll() {
            return new NavigationInput(Vector2.zero, PointerEventData.FramePressState.NotChanged);
        }
    }
}
