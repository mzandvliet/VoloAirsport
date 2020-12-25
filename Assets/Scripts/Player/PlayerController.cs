using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Volo {
    /* 
     * Todo: 
     * 
     * - Our input pipeline is quite disorganized with all this new parachute stuff going on,
     * and then consider even more bespoke control sets such as for running and jumping on ground.
     * Figure out a better way to author this.
     * - Sort out execution order for input polling and usage. Right now it's random.
     * 
     */

    public class PlayerController : MonoBehaviour {
        [SerializeField]
        private PilotAnimator _animator;

        [SerializeField]
        private float _mouseGravity = 10f;
        [SerializeField]
        private float _mouseCancelPower = 10f;
        [SerializeField]
        private float _mouseBufferStrength = 0.02f;

        /* dependencies */
        [Dependency, SerializeField]
        private PilotActionMapProvider _wingsuitActionMap;
        [Dependency, SerializeField]
        private ParachuteActionMapProvider _parachuteActionMap;
        [Dependency("gameClock"), SerializeField]
        private AbstractUnityClock _gameClock;
        [Dependency("fixedClock"), SerializeField]
        private AbstractUnityClock _fixedClock;

        private CharacterInput _wingsuitInput;
        private CharacterInput _mouseWingsuitInput;
        private CameraInput _cameraInput;
        private Vector2 _mouseInput;

        private ParachuteInput _parachuteInput;

        public void Clear() {
            _wingsuitInput = CharacterInput.Zero;
            _parachuteInput = ParachuteInput.Zero;
        }

        private void Update() {
            if (_wingsuitActionMap == null) {
                Debug.LogWarning("No actionmap found");
                return;
            }

            var wingsuitInput = PollWingsuitInput(_wingsuitActionMap.ActionMap);
            var prevMouseInput = _mouseWingsuitInput;
            _mouseWingsuitInput = PollWingsuitMouseInput(_wingsuitActionMap.ActionMap);
            var mouseWingsuitInput = ApplyWingsuitMouseState(_gameClock.DeltaTime, _mouseGravity, _mouseCancelPower, _mouseBufferStrength, prevMouseInput, _mouseWingsuitInput);
            _wingsuitInput = wingsuitInput.Merge(mouseWingsuitInput);
            _cameraInput = PollCameraInput(_wingsuitActionMap.ActionMap);

            if (_parachuteActionMap == null) {
                Debug.LogWarning("No parachute actionmap assigned");
                return;
            }

            _parachuteInput = _parachuteActionMap.V.Input;
        }

        private static CharacterInput PollWingsuitMouseInput(PilotActionMap actionMap) {
            return new CharacterInput {
                Pitch = actionMap.PollMouseAxis(WingsuitAction.Pitch),
                Roll = actionMap.PollMouseAxis(WingsuitAction.Roll),
                Yaw = actionMap.PollMouseAxis(WingsuitAction.Yaw),
                Cannonball = actionMap.PollMouseAxis(WingsuitAction.Cannonball),
                CloseLeftArm = actionMap.PollMouseAxis(WingsuitAction.CloseLeftArm),
                CloseRightArm = actionMap.PollMouseAxis(WingsuitAction.CloseRightArm)
            };
        }

        private static CharacterInput PollWingsuitInput(PilotActionMap actionMap) {
            return new CharacterInput {
                Pitch = actionMap.PollAxis(WingsuitAction.Pitch),
                Roll = actionMap.PollAxis(WingsuitAction.Roll),
                Yaw = actionMap.PollAxis(WingsuitAction.Yaw),
                Cannonball = actionMap.PollAxis(WingsuitAction.Cannonball),
                CloseLeftArm = actionMap.PollAxis(WingsuitAction.CloseLeftArm),
                CloseRightArm = actionMap.PollAxis(WingsuitAction.CloseRightArm)
            };
        }

        private static CameraInput PollCameraInput(PilotActionMap actionMap) {
            return new CameraInput {
                Horizontal = actionMap.PollAxis(WingsuitAction.LookHorizontal),
                Vertical = actionMap.PollAxis(WingsuitAction.LookVertical),
            };
        }

        private static CharacterInput ApplyWingsuitMouseState(float deltaTime, float gravity, float cancelPower, float inputBufferStrength, CharacterInput prevInput, CharacterInput input) {
            /* Todo: 
             * 
             * Mouse control should be mode-less. This click 'n hold business is too complex, but simple input = output mapping is not good enough either
             * What we certainly want to avoid: multiple mouse drags for longer maneuvers.
             * 
             * We want to make mouse input 'sticky' in some situations, but not in others. The stickiness should be fluid, and automatic.
             * By sticky, I mean we want some input to stay active over time.
             * We might be able to use mouse input magnitude to determine how muc
             */
            var adjustedInput = input;

            //Debug.Log(input.Pitch);

            adjustedInput.Pitch = prevInput.Pitch + input.Pitch * inputBufferStrength;
            adjustedInput.Roll = prevInput.Roll + input.Roll * inputBufferStrength;

            adjustedInput.Pitch = Mathf.Lerp(adjustedInput.Pitch, 0, deltaTime * gravity);
            adjustedInput.Roll = Mathf.Lerp(adjustedInput.Roll, 0, deltaTime * gravity);

            if (Mathf.Abs(input.Pitch) > 0f) {
                adjustedInput.Pitch = Mathf.Sign(input.Pitch) != Mathf.Sign(prevInput.Pitch) ? Mathf.Lerp(adjustedInput.Pitch, input.Pitch, cancelPower * deltaTime) : adjustedInput.Pitch;
            }
            if (Mathf.Abs(input.Roll) > 0f) {
                adjustedInput.Roll = Mathf.Sign(input.Roll) != Mathf.Sign(prevInput.Roll) ? Mathf.Lerp(adjustedInput.Roll, input.Roll, cancelPower * deltaTime) : adjustedInput.Roll;
            }

            adjustedInput.Pitch = Mathf.Clamp(adjustedInput.Pitch, -1f, 1f);
            adjustedInput.Roll = Mathf.Clamp(adjustedInput.Roll, -1f, 1f);

            return adjustedInput;
        }

        private void FixedUpdate() {
            _animator.SetInput(_wingsuitInput, _parachuteInput, _cameraInput);
        }
    }

    public struct CameraInput {
        public float Horizontal;
        public float Vertical;

        public static readonly CameraInput Zero = new CameraInput() { Horizontal = 0f, Vertical = 0f };
    }
}