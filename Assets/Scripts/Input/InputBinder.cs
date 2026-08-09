using System;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    // Todo: inert placeholder - see InputBindingStubs.cs. StartRebind should listen for the
    // next button/axis press across all connected devices (Input System's
    // InputActionRebindingExtensions.PerformInteractiveRebinding is the natural fit) and report
    // it back; currently it just immediately reports "nothing captured".
    public class InputBinder : MonoBehaviour {
        public void StartRebind(Action<Maybe<InputBindingSource>> onComplete) {
            onComplete(Maybe.Nothing<InputBindingSource>());
        }
    }
}
