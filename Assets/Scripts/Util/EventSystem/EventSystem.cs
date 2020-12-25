using System;
using System.Collections.Generic;

namespace RamjetAnvil.EventSystem {
    public interface IEventBus {
        void Emit<T>(T @event);
    }

    public interface IEventListener {
        IDisposable Listen<T>(Action<T> eventHandler);
        IDisposable Listen<T>(Action eventHandler);
    }

    public interface IEventSystem : IEventBus, IEventListener { }

    public class EventSystem : IEventSystem {

        private readonly IDictionary<Type, IList<object>> _eventHandlers;

        public EventSystem() {
            _eventHandlers = new Dictionary<Type, IList<object>>();
        }

        public void Emit<T>(T @event) {
            IList<object> handlers;
            if (_eventHandlers.TryGetValue(typeof (T), out handlers)) {
                for (int i = 0; i < handlers.Count; i++) {
                    var handler = (Action<T>)handlers[i];
                    handler(@event);
                }       
            }
        }

        public IDisposable Listen<T>(Action<T> eventHandler) {
            IList<object> registeredHandlers;
            if(!_eventHandlers.TryGetValue(typeof(T), out registeredHandlers)) {
                registeredHandlers = new List<object>();
                _eventHandlers.Add(typeof(T), registeredHandlers);
            }
            registeredHandlers.Add(eventHandler);

            return new EventHandlerDisposable<T>(registeredHandlers, eventHandler);
        }

        public IDisposable Listen<T>(Action eventHandler) {
            return Listen<T>(_ => eventHandler());
        }
    }

    public struct EventHandlerDisposable<TEvent> : IDisposable {

        private readonly IList<object> _registeredHandlers;
        private readonly Action<TEvent> _handler;
        private bool _isDisposed;

        public EventHandlerDisposable(IList<object> registeredHandlers, Action<TEvent> handler) {
            _registeredHandlers = registeredHandlers;
            _handler = handler;
            _isDisposed = false;
        }

        public void Dispose() {
            if (!_isDisposed) {
                _registeredHandlers.Remove(_handler);
                _isDisposed = true;
            }
        }
    }

}
