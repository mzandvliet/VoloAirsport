using System;
using RamjetAnvil.BubblingEventSystem;
using RamjetAnvil.Volo.UIEvents;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnityUIEventConverter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler {

    public void OnPointerClick(PointerEventData eventData) {
        switch (eventData.button) {
            case PointerEventData.InputButton.Left:
                this.SendBubblingEventTo(gameObject, new CursorClickEvent());
                break;
//            case PointerEventData.InputButton.Right:
//                break;
//            case PointerEventData.InputButton.Middle:
//                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        this.SendEventTo(gameObject, new CursorEnterEvent());
    }

    public void OnPointerExit(PointerEventData eventData) {
        this.SendEventTo(gameObject, new CursorLeaveEvent());
    }

    public void OnSubmit(BaseEventData eventData) {
        this.SendBubblingEventTo(gameObject, new CursorClickEvent());
    }
}
