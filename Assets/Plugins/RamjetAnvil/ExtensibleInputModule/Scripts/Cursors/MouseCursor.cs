using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule
{
    public class MouseCursor : ICursor {
        private const int LeftMouseButtonId = 0;

        private readonly Camera _camera;

        public MouseCursor(Camera camera) {
            _camera = camera;
        }

        public CursorInput Poll() {
            var isCursorUsable = Cursor.visible && Cursor.lockState != CursorLockMode.Locked;
            Ray ray;
            if (_camera != null && isCursorUsable) {
                ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            } else {
                ray = new Ray(Vector3.zero, Vector3.zero);
            }

            PointerEventData.FramePressState submitButtonState;
            if (Input.GetMouseButtonDown(LeftMouseButtonId)) {
                submitButtonState = PointerEventData.FramePressState.Pressed;
            } else if (Input.GetMouseButtonUp(LeftMouseButtonId)) {
                submitButtonState = PointerEventData.FramePressState.Released;
            } else {
                submitButtonState = PointerEventData.FramePressState.NotChanged;
            }

            Vector2 screenPosition = _camera.WorldToScreenPoint(ray.origin);

            return new CursorInput(ray, screenPosition, submitButtonState, Input.mouseScrollDelta);
        }
    }
}
