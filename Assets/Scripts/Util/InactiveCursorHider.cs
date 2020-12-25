using System;
using System.Linq;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public class InactiveCursorHider : MonoBehaviour {
    [SerializeField] private float _activityThreshold = 0.01f;
    [SerializeField] private float _inactivityDelayInS = 3f;

    private float _activationTime;

    private Func<bool> _mouseActivity;

    void Awake() {
        var mouseInput = ImperoCore.MergeAll(
            Adapters.MergeButtons,
            Peripherals.Mouse.Buttons,
            Peripherals.Mouse.Axes.Adapt(Adapters.Axis2Button(_activityThreshold)));

        _mouseActivity = ImperoCore.MergePollFns(Adapters.MergeButtons, mouseInput.Source.Values)
            .Adapt(buttonState => buttonState == ButtonState.Pressed);
    }

    void Update() {
        if (Cursor.lockState == CursorLockMode.None) {
            if (_mouseActivity()) {
                Cursor.visible = true;
                _activationTime = Time.realtimeSinceStartup;
            } else if (_activationTime + _inactivityDelayInS < Time.realtimeSinceStartup) {
                Cursor.visible = false;
            }
        }
    }
}
