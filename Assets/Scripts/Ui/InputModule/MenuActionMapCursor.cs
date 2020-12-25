using System;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {

    public class MenuActionMapCursor : INavigationDevice {

        private readonly MenuActionMapProvider _actionMapProvider;

        public MenuActionMapCursor(MenuActionMapProvider actionMapProvider) {
            _actionMapProvider = actionMapProvider;
        }

        public NavigationInput Poll() {
            var actionMap = _actionMapProvider.ActionMap.V;
            Vector2 movementDelta = Vector2.zero;
            if (actionMap.PollButton(MenuAction.Down) == ButtonState.Pressed) {
                movementDelta.y -= 1;
            }
            if (actionMap.PollButton(MenuAction.Up) == ButtonState.Pressed) {
                movementDelta.y += 1;
            }
            if (actionMap.PollButton(MenuAction.Left) == ButtonState.Pressed) {
                movementDelta.x -= 1;
            }
            if (actionMap.PollButton(MenuAction.Right) == ButtonState.Pressed) {
                movementDelta.x += 1;
            }

            var submitEvent = ToFramePressState(actionMap.PollButtonEvent(MenuAction.Confirm));
            return new NavigationInput(movementDelta, submitEvent);
        }

        private static PointerEventData.FramePressState ToFramePressState(ButtonEvent @event) {
            switch (@event) {
                case ButtonEvent.Nothing:
                    return PointerEventData.FramePressState.NotChanged;
                case ButtonEvent.Down:
                    return PointerEventData.FramePressState.Pressed;
                case ButtonEvent.Up:
                    return PointerEventData.FramePressState.Released;
                default:
                    throw new ArgumentOutOfRangeException("event", @event, null);
            }
        }
    }
}
