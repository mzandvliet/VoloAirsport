using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.BubblingEventSystem;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.UIEvents;
using UnityEngine;

public class UIEventSource : MonoBehaviour, ICursorHoverListener, ICursorClickListener {

    public event Action OnCursorEnter;
    public event Action OnCursorLeave;
    public event Action OnCursorClick;

    public void Receive(ref Event<CursorEnterEvent> @event) {
        if (OnCursorEnter != null) {
            OnCursorEnter();
        }
    }

    public void Receive(ref Event<CursorLeaveEvent> @event) {
        if (OnCursorLeave != null) {
            OnCursorLeave();
        }
    }

    public void Receive(ref Event<CursorClickEvent> @event) {
        if (OnCursorClick != null) {
            OnCursorClick();
        }
    }

    void OnDestroy() {
        OnCursorEnter = null;
        OnCursorLeave = null;
        OnCursorClick = null;
    }
}
