using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.BubblingEventSystem;
using RamjetAnvil.Volo.UIEvents;
using UnityEngine;

public class LoggingCursorListener : MonoBehaviour, ICursorHoverListener, ICursorClickListener
{
    public void Receive(ref Event<CursorEnterEvent> @event) {
        Debug.Log("Cursor enter from " + ((GameObject)(@event.Target)).name + " on " + this.name);
    }

    public void Receive(ref Event<CursorLeaveEvent> @event) {
        Debug.Log("Cursor leave from " + ((GameObject)(@event.Target)).name + " on " + this.name);
    }

    public void Receive(ref Event<CursorClickEvent> @event) {
        Debug.Log("Cursor click from " + ((GameObject)(@event.Target)).name + " on " + this.name);
    }
}
