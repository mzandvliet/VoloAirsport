using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using Valve.VR;

public class ViveController : MonoBehaviour {
    [SerializeField] private ViveControllerList _controllerList;

    private Quaternion? _neutralRotation;

    private float _pitch;
    private float _roll;

    void Update() {
//        if (Input.GetKeyDown(KeyCode.R)) {
//            Debug.Log("Recentering...");
//            Valve.VR.OpenVR.System.ResetSeatedZeroPose();
//        }
        if (_controllerList.ControllerIndices.Count > 0) {
            var controllerIndex = _controllerList.ControllerIndices[0];

            var controller = SteamVR_Controller.Input(controllerIndex);
            //var rotation = controller.transform.rot;

            _pitch = 0f;
            _roll = 0f;

            // TODO This provides spatial input for pilot pitch/roll
            // We don't use it at the moment because it sucks
            // What we want is a direct mapping from absolute rotation on the vive controller
            // to absolute position of the pilot.
            //            var touchClick = controller.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad);
            //            var touchClickDown = controller.GetTouchDown(EVRButtonId.k_EButton_SteamVR_Touchpad);
            //            if (touchClickDown) {
            //                _neutralRotation = controller.transform.rot;
            //            }
            //            if (touchClick && _neutralRotation.HasValue && controller.hasTracking) {
            //                //var localRotation = Quaternion.Inverse(_neutralRotation.Value) * controller.transform.rot;
            ////                Debug.Log("controller rotation: " + rotation.eulerAngles
            ////                    + " local rotation: " + localRotation.eulerAngles
            ////                    + " has tracking: " + controller.hasTracking
            ////                    + " touch click: " + touchClickDown);
            //
            //                Vector3 baseForward = _neutralRotation.Value * Vector3.forward;
            //                Vector3 baseRight = _neutralRotation.Value * Vector3.right;
            //                Vector3 viveForward = rotation * Vector3.forward;
            //                Vector3 viveRight = rotation * Vector3.right;
            //
            //                _pitch = -GetAxis(viveForward, baseForward, baseRight);
            //                _roll = GetAxis(viveRight, baseRight, baseForward);
            //
            //                //                pitch *= 1f - Mathf.Abs(Vector3.Dot(viveForward, baseRight));
            //            }

            var touchAxis = controller.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
            touchAxis = Adapters.ApplyDeadzone(0.16f, touchAxis);
            touchAxis = InputUtilities.CircularizeInput(touchAxis);
            touchAxis = MathUtils.ScaleQuadratically(touchAxis, 2f);

            _pitch = touchAxis.y;
            _roll = touchAxis.x;

            Debug.Log("pitch " + _pitch + ", roll " + _roll);
        }
    }

    private float GetAxis(Vector3 controllerVector, Vector3 baseVector, Vector3 rotationAxis) {
        const float maxAngle = 45f;
        float angle = MathUtils.AngleAroundAxis(controllerVector, baseVector, rotationAxis);
        float input = Mathf.Clamp(angle / maxAngle, -1f, 1f);
        input = MathUtils.ScaleQuadratically(input, 2f);
        return input;
    }

    public float Pitch {
        get { return _pitch; }
    }

    public float Roll {
        get { return _roll; }
    }

    public ViveControllerList ControllerList {
        set { _controllerList = value; }
    }
}