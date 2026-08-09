using System;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    // Todo: inert placeholder - see InputBindingStubs.cs. ActiveController never fires, so no
    // controller is ever reported as connected. Real implementation should watch
    // InputSystem.onDeviceChange / Gamepad.current and push connect/disconnect events here.
    public class JoystickActivator : MonoBehaviour {
        public IObservable<ConnectedController?> ActiveController {
            get { return System.Reactive.Linq.Observable.Never<ConnectedController?>(); }
        }
    }
}
