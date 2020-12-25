using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.BubblingEventSystem {

    public struct Event<TPayload> {
        private readonly TPayload _payload;
        private bool _shouldPropagate;
        private readonly object _source;
        private readonly object _target;

        public Event(object source, object target, TPayload payload) : this() {
            _payload = payload;
            _source = source;
            _target = target;
            _shouldPropagate = true;
        }

        public void StopPropagation() {
            _shouldPropagate = false;
        }

        public TPayload Payload {
            get { return _payload; }
        }

        public object Source {
            get { return _source; }
        }

        public object Target {
            get { return _target; }
        }

        public bool ShouldPropagate {
            get { return _shouldPropagate; }
        }
    }

    // TODO Rename to something other than bubbling
    public interface IBubblingEventListener<TEventPayload> {
        void Receive(ref Event<TEventPayload> @event);
    }

    public static class BubblingEventSystem {

        private static readonly IList<object> EventListeners = new List<object>();

        public static void SendEventTo<TEventPayload>(this Component source, GameObject target, TEventPayload eventPayload) {
            var @event = new Event<TEventPayload>(source.gameObject, target, eventPayload);
            @event.StopPropagation();
            SendEventTo(@event);
        }

        public static void SendBubblingEventTo<TEventPayload>(this Component source, GameObject target, TEventPayload eventPayload) {
            SendEventTo(new Event<TEventPayload>(source.gameObject, target, eventPayload));
        }

        private static void SendEventTo<TEventPayload>(Event<TEventPayload> @event) {
            var target = (GameObject)@event.Target;
            while (target != null) {
                var eventSourceMapping = target.GetComponent<EventSourceMapping>();
                if (eventSourceMapping != null) {
                    target = eventSourceMapping.Target;
                }

                EventListeners.Clear();
                target.GetComponentsOfInterface<IBubblingEventListener<TEventPayload>>(EventListeners);
                for (int i = 0; i < EventListeners.Count; i++) {
                    var component = EventListeners[i] as IBubblingEventListener<TEventPayload>;
                    component.Receive(ref @event);
                }

                // While targets are non-ui components, keep on bubbling

                target = @event.ShouldPropagate ? target.GetParent() : null;
            }
        }
    }
}