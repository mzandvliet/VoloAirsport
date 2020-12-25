using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.Volo.Ui {

    public class DefaultHoverEventSource : MonoBehaviour, IHoverEventSource {
        public event Action OnCursorEnter;
        public event Action OnCursorExit;

        void Awake() {
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateEntry(EventTriggerType.PointerEnter, OnPointerEnter));
            eventTrigger.triggers.Add(CreateEntry(EventTriggerType.PointerExit, OnPointerExit));
        }

        void OnPointerEnter() {
            if (OnCursorEnter != null) {
                OnCursorEnter();
            }
        }

        void OnPointerExit() {
            if (OnCursorExit != null) {
                OnCursorExit();
            }
        }

        EventTrigger.Entry CreateEntry(EventTriggerType type, Action callback) {
            var entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(eventData => callback());
            return entry;
        }
    }
}