using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Reactive
{
    // Functions
    public static class UnityRxKeyboard
    {
        public static IObservable<KeyboardEvent> CreateKeyboard()
        {
            return CreateKeyboard(Enum.GetValues(typeof(KeyCode)) as KeyCode[]);
        }

        public static IObservable<KeyboardEvent> CreateKeyboard(IList<KeyCode> pollableKeys)
        {
            return UnityObservable.CreateUpdate<KeyboardEvent>(observer =>
            {
                for (int i = 0; i < pollableKeys.Count; i++) {
                    var key = pollableKeys[i];
                    if (Input.GetKeyDown(key))
                    {
                        observer.OnNext(new KeyboardEvent(KeyboardEvent.EventType.Down, key));
                    }
                    else if (Input.GetKeyUp(key))
                    {
                        observer.OnNext(new KeyboardEvent(KeyboardEvent.EventType.Up, key));
                    }
                }
            });
        }

        public static IObservable<KeyCode> KeyDown(this IObservable<KeyboardEvent> keyEvents)
        {
            return from keyEvent in keyEvents
                   where keyEvent.Type == KeyboardEvent.EventType.Down
                   select keyEvent.KeyCode;
        }

        public static IObservable<KeyCode> KeyUp(this IObservable<KeyboardEvent> keyEvents)
        {
            return from keyEvent in keyEvents
                where keyEvent.Type == KeyboardEvent.EventType.Up
                select keyEvent.KeyCode;
        }

        public static IObservable<HashSet<KeyCode>> KeysHeld(this IObservable<KeyboardEvent> keyEvents)
        {
            return keyEvents.Scan(new HashSet<KeyCode>(), (set, @event) =>
            {
                if (@event.Type == KeyboardEvent.EventType.Down)
                {
                    set.Add(@event.KeyCode);
                }
                else
                {
                    set.Remove(@event.KeyCode);
                }
                return set;
            })
            .Publish()
            .RefCount();
        }
    }

    // Data
    public struct KeyboardEvent : IEquatable<KeyboardEvent>
    {
        public enum EventType
        {
            Up, Down
        }

        private readonly EventType _type;
        private readonly KeyCode _keyCode;

        public KeyboardEvent(EventType type, KeyCode keyCode)
        {
            _type = type;
            _keyCode = keyCode;
        }

        public EventType Type
        {
            get { return _type; }
        }

        public KeyCode KeyCode
        {
            get { return _keyCode; }
        }

        public bool Equals(KeyboardEvent other)
        {
            return _type == other._type && _keyCode == other._keyCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is KeyboardEvent && Equals((KeyboardEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)_type * 397) ^ (int)_keyCode;
            }
        }
    }

}
