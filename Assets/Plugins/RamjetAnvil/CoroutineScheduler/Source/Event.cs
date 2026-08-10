using System;
using System.Reflection;

namespace RamjetAnvil.Coroutine {

    public class Event {
        private readonly EventInfo _eventInfo;
        private readonly object _subject;

        public Event(object subject, string eventName) {
            _eventInfo = subject.GetType().GetEvent(eventName);
            _subject = subject;
        }

        public void AddHandler(Delegate handler) {
            _eventInfo.AddEventHandler(_subject, handler);
        }

        public void RemoveHandler(Delegate handler) {
            _eventInfo.RemoveEventHandler(_subject, handler);
        }
    }
}
