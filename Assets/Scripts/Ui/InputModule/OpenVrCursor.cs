using RamjetAnvil.Cameras;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace RamjetAnvil.Volo.Ui {
    public class OpenVrCursor : ICursor {
        [Dependency] private OpenVrCameraRig _rig;
        [Dependency] private ViveControllerList _controllerList;

        public CursorInput Poll() {
            Ray ray;
            if (!_controllerList || _controllerList.ControllerIndices.Count == 0 || _rig == null) {
                ray = new Ray(Vector3.zero, Vector3.down);
            } else {
                ray = new Ray();
            }

            PointerEventData.FramePressState submitEvent = PointerEventData.FramePressState.NotChanged;
            var controllerIndices = _controllerList.ControllerIndices;
            if (SteamVR_Controller.Input(controllerIndices[0]).GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger)) {
                submitEvent = PointerEventData.FramePressState.Pressed;
            } else if (SteamVR_Controller.Input(controllerIndices[0]).GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger)) {
                submitEvent = PointerEventData.FramePressState.Released;
            }

            Vector2 screenPosition = _rig.GetMainCamera().WorldToScreenPoint(ray.origin);

            return new CursorInput(
                ray,
                screenPosition,
                submitEvent: submitEvent,
                scrollDelta: Vector2.zero);
        }
    }
}
