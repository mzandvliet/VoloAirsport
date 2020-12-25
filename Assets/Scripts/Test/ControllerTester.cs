using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    public class ControllerTester : MonoBehaviour {

        void Awake() {
//            var buttonActivity = ControllerDetection.ControllerActivity(ControllerDetection.AllControllerButtons())
//                .DistinctUntilChanged();
//            buttonActivity.Subscribe(inputSource => Debug.Log(inputSource));
//
//            var axisInput = ControllerDetection.AllControllerAxes()
//                .Adapt(axis => Mathf.Abs(axis) > 0.99f ? ButtonState.Pressed : ButtonState.Released)
//                .Adapt(() => Adapters.ButtonEvents(() => Time.frameCount));
//            var axisActivity = ControllerDetection.ControllerActivity(axisInput)
//                .Where(inputSource => {
//                    var axis = inputSource.ControllerInputSource.InputSource.Interactable as Interactable.PolarizedAxis;
//                    if (axis != null) {
//                        var joystickAxis = axis.Axis as Interactable.JoystickAxis;
//                        if (joystickAxis != null) {
//                            return joystickAxis.Id != 4 && joystickAxis.Id != 3;
//                        }
//                    }
//                    return true;
//                })
//                .DistinctUntilChanged();
//            axisActivity.Subscribe(inputSource => Debug.Log(inputSource));
//
//            var controllerActivity = gameObject.AddComponent<JoystickActivator>().ActiveController;
//            controllerActivity.Subscribe(activity => Debug.Log(activity));
        }
    }
}
