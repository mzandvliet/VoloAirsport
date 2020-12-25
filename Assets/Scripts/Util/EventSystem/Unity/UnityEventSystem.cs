using System;
using System.Collections.Generic;
using RamjetAnvil.EventSystem;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class UnityEventSystem : AbstractUnityEventSystem {

    private IEventSystem _eventSystem;

    void Awake() {
        _eventSystem = _eventSystem ?? new EventSystem();
    }

    private IEventSystem EventSystem {
        get {
            if (_eventSystem == null) {
                _eventSystem = new EventSystem();
            }
            return _eventSystem;
        }
    }

    public override IDisposable Listen<T>(Behaviour component, Action<T> handler) {
        var handlerDisposable = EventSystem.Listen<T>(@event => {
            if (component.IsDestroyed()) {
                throw new Exception(typeof(T) + " was emitted to destroyed component " + component.name + ". Did you forget to dispose the listener?");
            } else if (component.enabled && component.gameObject.activeInHierarchy) {
                handler(@event);
            }
        });

        var lifecycleEvents = component.gameObject.AddComponent<ComponentLifecycleEvents>();
        lifecycleEvents.OnDestroyEvent += handlerDisposable.Dispose;

        return handlerDisposable;
    }

    public override void Emit<T>(T @event) {
        EventSystem.Emit(@event);
    }

    public override IDisposable Listen<T>(Action<T> eventHandler) {
        return EventSystem.Listen(eventHandler);
    }

}