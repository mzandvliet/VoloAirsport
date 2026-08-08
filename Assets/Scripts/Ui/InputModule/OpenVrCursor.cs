using RamjetAnvil.Cameras;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.Volo.Ui {
    // VR support (SteamVR) is not ported yet; this is a neutral no-op stand-in for ICursor.
    public class OpenVrCursor : ICursor {
        [Dependency] private OpenVrCameraRig _rig;

        public CursorInput Poll() {
            var ray = new Ray(Vector3.zero, Vector3.down);
            Vector2 screenPosition = _rig != null ? _rig.GetMainCamera().WorldToScreenPoint(ray.origin) : Vector2.zero;

            return new CursorInput(
                ray,
                screenPosition,
                submitEvent: PointerEventData.FramePressState.NotChanged,
                scrollDelta: Vector2.zero);
        }
    }
}
