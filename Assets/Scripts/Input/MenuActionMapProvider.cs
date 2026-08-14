using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RamjetAnvil.Volo.Input {

    public class MenuActionMap : ActionMap<MenuAction> {
        public MenuActionMap() : base(new InputActionMap("Menu")) {
            SetupBindings();
            EnableActions();
        }

        private void SetupBindings() {
            var left = AddButton(MenuAction.Left, "Left");
            left.AddBinding("<Keyboard>/a");
            left.AddBinding("<Keyboard>/leftArrow");
            left.AddBinding("<Gamepad>/dpad/left");
            left.AddBinding("<Gamepad>/leftStick/left");

            var right = AddButton(MenuAction.Right, "Right");
            right.AddBinding("<Keyboard>/d");
            right.AddBinding("<Keyboard>/rightArrow");
            right.AddBinding("<Gamepad>/dpad/right");
            right.AddBinding("<Gamepad>/leftStick/right");

            var up = AddButton(MenuAction.Up, "Up");
            up.AddBinding("<Keyboard>/w");
            up.AddBinding("<Keyboard>/upArrow");
            up.AddBinding("<Gamepad>/dpad/up");
            up.AddBinding("<Gamepad>/leftStick/up");

            var down = AddButton(MenuAction.Down, "Down");
            down.AddBinding("<Keyboard>/s");
            down.AddBinding("<Keyboard>/downArrow");
            down.AddBinding("<Gamepad>/dpad/down");
            down.AddBinding("<Gamepad>/leftStick/down");

            var confirm = AddButton(MenuAction.Confirm, "Confirm");
            confirm.AddBinding("<Keyboard>/enter");
            confirm.AddBinding("<Keyboard>/space");
            confirm.AddBinding("<Gamepad>/buttonSouth");

            var back = AddButton(MenuAction.Back, "Back");
            back.AddBinding("<Keyboard>/escape");
            back.AddBinding("<Gamepad>/buttonEast");

            var pause = AddButton(MenuAction.Pause, "Pause");
            pause.AddBinding("<Keyboard>/escape");
            pause.AddBinding("<Gamepad>/start");

            // VR recenter is unbound for now - VR isn't supported in this build yet.
            AddButton(MenuAction.RecenterVrHeadset, "RecenterVrHeadset");

            var screenshot = AddButton(MenuAction.TakeScreenshot, "TakeScreenshot");
            screenshot.AddBinding("<Keyboard>/f12");
        }

        // Discrete (button-based, not analog) directional nudge for list/grid UI navigation -
        // e.g. SpawnScreen's spawnpoint list. Gated on the rising edge (WasPressedThisFrame),
        // not raw IsPressed(), so a held key/stick nudges the selection once per press instead
        // of shooting through the whole list every frame the direction stays held.
        public Vector2 PollDiscreteCursor() {
            float x = (PollButtonEvent(MenuAction.Right) == ButtonEvent.Down ? 1f : 0f)
                      - (PollButtonEvent(MenuAction.Left) == ButtonEvent.Down ? 1f : 0f);
            float y = (PollButtonEvent(MenuAction.Up) == ButtonEvent.Down ? 1f : 0f)
                      - (PollButtonEvent(MenuAction.Down) == ButtonEvent.Down ? 1f : 0f);
            return new Vector2(x, y);
        }
    }

    public class MenuActionMapProvider : MonoBehaviour {
        private MenuActionMap _actionMap;
        private IReadonlyRef<MenuActionMap> _actionMapRef;

        private void Awake() {
            _actionMap = new MenuActionMap();
            _actionMapRef = new Ref<MenuActionMap>(_actionMap);
        }

        public IReadonlyRef<MenuActionMap> ActionMap {
            get { return _actionMapRef; }
        }

        // Todo: not wired up yet - see PilotActionMapProvider.SetInputMappingSource.
        public void SetInputMappingSource(System.IObservable<ActionMapConfig<MenuAction>> mappingChanges) {
        }

        private void OnDestroy() {
            if (_actionMap != null) {
                _actionMap.Dispose();
            }
        }
    }
}
