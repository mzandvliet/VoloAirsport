using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.InputModule {

    public class SurfaceDragHandler : UnityDraggable, ICompleteDragHandler, IPointerEnterHandler, IPointerExitHandler {

        public override event Action OnDragStart;
        public override event Dragging Dragging;
        public override event Action OnDragStop;

        public override event Action OnHighlight;
        public override event Action OnUnHighlight;

        private bool _isPointerOnSurface;
        private bool _isDragging;
        private Vector2 _prevDragPosition;

        public void OnDrag(PointerEventData eventData) {
            var dragDiff = _prevDragPosition - eventData.position;
            _prevDragPosition = eventData.position;
            dragDiff = dragDiff / 60f;
            if (Dragging != null) {
                Dragging(eventData.pressEventCamera.transform, dragDiff);    
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) {
            _prevDragPosition = eventData.position;
            if (!_isDragging && OnDragStart != null) {
                OnDragStart();
            }
            _isDragging = true;
        }

        private void OnEndDrag(PointerEventData eventData) {
            if (_isDragging) {
                if (OnDragStop != null) {
                    OnDragStop();
                }
                if (!_isPointerOnSurface && OnUnHighlight != null) {
                    OnUnHighlight();
                }
            }
            _isDragging = false;
        }

        public void OnPointerDown(PointerEventData eventData) {
            OnInitializePotentialDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            OnEndDrag(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _isPointerOnSurface = true;
            if (OnHighlight != null) {
                OnHighlight();
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isPointerOnSurface = false;
            if (!_isDragging && OnUnHighlight != null) {
                OnUnHighlight();
            }
        }
    }
}
