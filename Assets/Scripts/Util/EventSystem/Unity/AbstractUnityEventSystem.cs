using System;
using RamjetAnvil.EventSystem;
using UnityEngine;

public abstract class AbstractUnityEventSystem : MonoBehaviour, IEventSystem {
    public abstract IDisposable Listen<T>(Behaviour component, Action<T> handler);

    public IDisposable Listen<T>(Behaviour component, Action handler) {
        return Listen<T>(component, @event => handler());
    }

    public abstract void Emit<T>(T @event);
    public abstract IDisposable Listen<T>(Action<T> eventHandler);
    public IDisposable Listen<T>(Action eventHandler) {
        return Listen<T>(_ => eventHandler());
    }
}