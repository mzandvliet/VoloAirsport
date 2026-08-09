using UnityEngine;
using UnityEngine.InputSystem;

public class InactiveCursorHider : MonoBehaviour {
    [SerializeField] private float _activityThreshold = 0.01f;
    [SerializeField] private float _inactivityDelayInS = 3f;

    private float _activationTime;

    private bool MouseActivity() {
        var mouse = Mouse.current;
        if (mouse == null) {
            return false;
        }
        return mouse.delta.ReadValue().sqrMagnitude > _activityThreshold * _activityThreshold
            || mouse.leftButton.isPressed
            || mouse.rightButton.isPressed
            || mouse.middleButton.isPressed;
    }

    void Update() {
        if (Cursor.lockState == CursorLockMode.None) {
            if (MouseActivity()) {
                Cursor.visible = true;
                _activationTime = Time.realtimeSinceStartup;
            } else if (_activationTime + _inactivityDelayInS < Time.realtimeSinceStartup) {
                Cursor.visible = false;
            }
        }
    }
}
