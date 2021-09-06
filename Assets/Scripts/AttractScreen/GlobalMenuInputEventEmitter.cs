using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public class GlobalMenuInputEventEmitter : MonoBehaviour {

    [Dependency, SerializeField] private MenuActionMapProvider _menuActionMapProvider;
    [Dependency, SerializeField] private AbstractUnityEventSystem _eventSystem;

    private void Update() {
        Debug.LogWarning("I want to handle menu input");
        // var actionMap = _menuActionMapProvider.ActionMap.V;

        // if (actionMap.PollButtonEvent(MenuAction.Confirm) == ButtonEvent.Down) {
        //     _eventSystem.Emit(new Events.OnConfirmPressed());
        // }
        // if (actionMap.PollButtonEvent(MenuAction.Back) == ButtonEvent.Down) {
        //     _eventSystem.Emit(new Events.OnBackPressed());
        // }
        // if (actionMap.PollButtonEvent(MenuAction.Pause) == ButtonEvent.Down) {
        //     _eventSystem.Emit(new Events.OnPausePressed());
        // }
    }
}