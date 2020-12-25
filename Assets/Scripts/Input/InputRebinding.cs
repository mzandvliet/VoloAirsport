using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public static class InputRebinding {

        public static InputMap<InputSource, ButtonState> CreateCandidateInputMap(
            ControllerId controllerId = null, 
            float joystickAxisThreshold = 0.7f, 
            float mouseAxisThreshold = 30f) {

            var inputMap = ImperoCore.MergeAll(
                Adapters.MergeButtons,
                Peripherals.Keyboard,
                Peripherals.Mouse.Buttons,
                Peripherals.Mouse.PolarizedAxes
                    .Adapt(Adapters.Axis2Button(mouseAxisThreshold)));

            if (controllerId != null) {
                var peripheral = Peripherals.Controller.GetPeripheral(controllerId, InputSettings.Default);
                inputMap = ImperoCore.MergeAll(
                    Adapters.MergeButtons,
                    inputMap,
                    peripheral.Buttons,
                    peripheral.Axes.Adapt(Adapters.Axis2Button(joystickAxisThreshold)));
            }

            return inputMap;
        }
    }
}
