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
            var pitch = AddAxis(WingsuitAction.Pitch, "Pitch");
            pitch.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/s")
                .With("Positive", "<Keyboard>/w");
            pitch.AddBinding("<Gamepad>/leftStick/y");

            var roll = AddAxis(WingsuitAction.Roll, "Roll");
            roll.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            roll.AddBinding("<Gamepad>/leftStick/x");

            var yaw = AddAxis(WingsuitAction.Yaw, "Yaw");
            yaw.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/q")
                .With("Positive", "<Keyboard>/e");
            yaw.AddBinding("<Gamepad>/rightStick/x");

            var cannonball = AddButton(WingsuitAction.Cannonball, "Cannonball");
            cannonball.AddBinding("<Keyboard>/space");
            cannonball.AddBinding("<Gamepad>/buttonSouth");

            var closeLeftArm = AddAxis(WingsuitAction.CloseLeftArm, "CloseLeftArm");
            closeLeftArm.AddBinding("<Keyboard>/leftShift");
            closeLeftArm.AddBinding("<Gamepad>/leftTrigger");

            var closeRightArm = AddAxis(WingsuitAction.CloseRightArm, "CloseRightArm");
            closeRightArm.AddBinding("<Keyboard>/rightShift");
            closeRightArm.AddBinding("<Gamepad>/rightTrigger");

            var lookHorizontal = AddAxis(WingsuitAction.LookHorizontal, "LookHorizontal");
            lookHorizontal.AddBinding("<Gamepad>/rightStick/x");

            var lookVertical = AddAxis(WingsuitAction.LookVertical, "LookVertical");
            lookVertical.AddBinding("<Gamepad>/rightStick/y");

            var mousePitch = AddMouseAxis(WingsuitAction.Pitch, "MousePitch");
            mousePitch.AddBinding("<Mouse>/delta/y");

            var mouseRoll = AddMouseAxis(WingsuitAction.Roll, "MouseRoll");
            mouseRoll.AddBinding("<Mouse>/delta/x");
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
