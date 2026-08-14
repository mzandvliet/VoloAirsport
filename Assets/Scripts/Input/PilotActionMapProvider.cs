using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RamjetAnvil.Volo.Input {

    // Todo: placeholder bindings, needs real tuning once the flight model is retuned for Unity 6 physics.
    public class PilotActionMap : ActionMap<WingsuitAction> {
        public PilotActionMap() : base(new InputActionMap("Pilot")) {
            SetupBindings();
            EnableActions();
        }

        private void SetupBindings() {
            // Mouse-primary: nobody plays keyboard-only, and the game already expects a
            // gamepad, so the mouse (not WASD) is the default keyboard+mouse source for
            // pitch/roll - see the MousePitch/MouseRoll bindings below. WASD is freed up to
            // mirror the gamepad's face-button layout instead (see CloseArms/CloseLeftArm/
            // CloseRightArm/Cannonball below), matching the original mouse-primary scheme.
            var pitch = AddAxis(WingsuitAction.Pitch, "Pitch");
            pitch.AddBinding("<Gamepad>/leftStick/y");

            var roll = AddAxis(WingsuitAction.Roll, "Roll");
            roll.AddBinding("<Gamepad>/leftStick/x");

            var yaw = AddAxis(WingsuitAction.Yaw, "Yaw");
            yaw.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/q")
                .With("Positive", "<Keyboard>/e");
            // Bipolar trigger axis, not the right stick - the right stick already drives
            // camera look (LookHorizontal/LookVertical below); it was previously double-bound
            // to both, which made the right stick fight the camera for yaw.
            yaw.AddCompositeBinding("1DAxis")
                .With("Negative", "<Gamepad>/leftTrigger")
                .With("Positive", "<Gamepad>/rightTrigger");

            // Face buttons mirror WASD spatially: South/down=Cannonball, North/up=CloseArms,
            // West/left=CloseLeftArm, East/right=CloseRightArm - same mental model on both
            // devices.
            // AddAxis, not AddButton: PlayerController.PollWingsuitInput/PollWingsuitMouseInput
            // read every wingsuit control (including these two) via PollAxis/PollMouseAxis,
            // which only look in the axis/mouse-axis dictionaries. An AddButton-registered
            // action lives in a separate dictionary entirely and silently polls as 0 forever -
            // this was already the case for Cannonball before this change (a pre-existing bug,
            // not introduced here), and would have been for CloseArms too.
            var cannonball = AddAxis(WingsuitAction.Cannonball, "Cannonball");
            cannonball.AddBinding("<Keyboard>/s");
            cannonball.AddBinding("<Gamepad>/buttonSouth");

            // Also see PlayerController.cs - CloseArms isn't a field on CharacterInput at all,
            // so on its own this binding would still do nothing. It's composed additively into
            // CloseLeftArm/CloseRightArm at the polling layer instead, since nothing downstream
            // (PilotAnimator) needs to know "close both" is a distinct action from "both
            // individual closes happened to be held at once".
            var closeArms = AddAxis(WingsuitAction.CloseArms, "CloseArms");
            closeArms.AddBinding("<Keyboard>/w");
            closeArms.AddBinding("<Gamepad>/buttonNorth");

            var closeLeftArm = AddAxis(WingsuitAction.CloseLeftArm, "CloseLeftArm");
            closeLeftArm.AddBinding("<Keyboard>/a");
            closeLeftArm.AddBinding("<Gamepad>/buttonWest");

            var closeRightArm = AddAxis(WingsuitAction.CloseRightArm, "CloseRightArm");
            closeRightArm.AddBinding("<Keyboard>/d");
            closeRightArm.AddBinding("<Gamepad>/buttonEast");

            var lookHorizontal = AddAxis(WingsuitAction.LookHorizontal, "LookHorizontal");
            lookHorizontal.AddBinding("<Gamepad>/rightStick/x");

            var lookVertical = AddAxis(WingsuitAction.LookVertical, "LookVertical");
            lookVertical.AddBinding("<Gamepad>/rightStick/y");

            var mousePitch = AddMouseAxis(WingsuitAction.Pitch, "MousePitch");
            mousePitch.AddBinding("<Mouse>/delta/y");

            var mouseRoll = AddMouseAxis(WingsuitAction.Roll, "MouseRoll");
            mouseRoll.AddBinding("<Mouse>/delta/x");

            var respawn = AddButton(WingsuitAction.Respawn, "Respawn");
            respawn.AddBinding("<Keyboard>/r");
            respawn.AddBinding("<Gamepad>/select");

            // Moved off North/West (now CloseArms/CloseLeftArm) to the old Impero defaults'
            // actual shoulder/stick-click slots for these two.
            var unfoldParachute = AddButton(WingsuitAction.UnfoldParachute, "UnfoldParachute");
            unfoldParachute.AddBinding("<Keyboard>/t");
            unfoldParachute.AddBinding("<Gamepad>/rightShoulder");

            var changeCamera = AddButton(WingsuitAction.ChangeCamera, "ChangeCamera");
            changeCamera.AddBinding("<Mouse>/middleButton");
            changeCamera.AddBinding("<Gamepad>/rightStickPress");

            var toggleSpectatorView = AddButton(WingsuitAction.ToggleSpectatorView, "ToggleSpectatorView");
            toggleSpectatorView.AddBinding("<Keyboard>/f3");
        }
    }

    public class PilotActionMapProvider : MonoBehaviour {
        private PilotActionMap _actionMap;
        private IReadonlyRef<PilotActionMap> _actionMapRef;

        private void Awake() {
            _actionMap = new PilotActionMap();
            _actionMapRef = new Ref<PilotActionMap>(_actionMap);
        }

        public PilotActionMap ActionMap {
            get { return _actionMap; }
        }

        public IReadonlyRef<PilotActionMap> ActionMapRef {
            get { return _actionMapRef; }
        }

        // Todo: not wired up yet - live-rebinding support (see InputBindings<T> in
        // InputBindingStubs.cs) needs to actually push new bindings into the InputActionMap.
        public void SetInputMappingSource(System.IObservable<ActionMapConfig<WingsuitAction>> mappingChanges) {
        }

        private void OnDestroy() {
            if (_actionMap != null) {
                _actionMap.Dispose();
            }
        }
    }
}
