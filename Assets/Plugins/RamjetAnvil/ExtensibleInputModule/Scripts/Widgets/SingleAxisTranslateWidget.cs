using System;
using UnityEngine;

namespace RamjetAnvil.InputModule {

    public class SingleAxisTranslateWidget : MonoBehaviour {

        [SerializeField] private float _dragSpeed = 1f;

        [SerializeField] private SurfaceDragHandler _dragHandler;
        [SerializeField] private Transform _widgetTransform;

        public event Action<Vector3> OnPositionChanged;

        void Awake() {
            _dragHandler.Dragging += ProcessDrag;
        }

        public void SetPosition(Vector3 localPosition) {
            _widgetTransform.localPosition = localPosition;
        }

        private void ProcessDrag(Transform cameraTransform, Vector3 diff) {
            diff *= _dragSpeed;

            var forward = cameraTransform.InverseTransformDirection(_widgetTransform.forward);
            var movement = Vector3.Project(-diff, forward);
            movement = cameraTransform.TransformDirection(movement);
            var newPosition = _widgetTransform.localPosition + movement;

            if (OnPositionChanged != null) {
                OnPositionChanged(newPosition);
            }
        }

        public float DragSpeed {
            get { return _dragSpeed; }
            set { _dragSpeed = value; }
        }
    }
}
