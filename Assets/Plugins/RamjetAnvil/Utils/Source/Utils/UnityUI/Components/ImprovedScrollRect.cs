using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ImprovedScrollRect : ScrollRect {

    [SerializeField] private bool _clickToScroll = false;

    public override void OnBeginDrag(PointerEventData eventData) {
        if (_clickToScroll) {
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData) {
        if (_clickToScroll) {
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData) {
        if (_clickToScroll) {
            base.OnEndDrag(eventData);
        }
    }
}
