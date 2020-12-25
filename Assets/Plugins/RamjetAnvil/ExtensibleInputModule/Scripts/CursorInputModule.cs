using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RamjetAnvil.InputModule {

    public class CursorInputModule : RamjetInputModule
    {
        [Header("Dragging")]
        [Tooltip("Minimum pointer movement in degrees to start dragging")]
        public float angleDragThreshold = 1;

        private float m_PrevActionTime;
        Vector2 m_LastMoveVector;
        int m_ConsecutiveMoveCount = 0;

        //private Vector2 m_LastMousePosition;
        //private Vector2 m_MousePosition;

//        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
//        public enum InputMode
//        {
//            Mouse,
//            Buttons
//        }
//
//        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
//        public InputMode inputMode
//        {
//            get { return InputMode.Mouse; }
//        }

//        [SerializeField]
//        private string m_HorizontalAxis = "Horizontal";
//
//        /// <summary>
//        /// Name of the vertical axis for movement (if axis events are used).
//        /// </summary>
//        [SerializeField]
//        private string m_VerticalAxis = "Vertical";
//
//        /// <summary>
//        /// Name of the submit button.
//        /// </summary>
//        [SerializeField]
//        private string m_SubmitButton = "Submit";
//
//        /// <summary>
//        /// Name of the submit button.
//        /// </summary>
//        [SerializeField]
//        private string m_CancelButton = "Cancel";

        [SerializeField]
        private float m_InputActionsPerSecond = 10;

        [SerializeField]
        private float m_RepeatDelay = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
        private bool m_ForceModuleActive;

        [Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
        public bool allowActivationOnMobileDevice
        {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        public bool forceModuleActive
        {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        public float inputActionsPerSecond
        {
            get { return m_InputActionsPerSecond; }
            set { m_InputActionsPerSecond = value; }
        }

        public float repeatDelay
        {
            get { return m_RepeatDelay; }
            set { m_RepeatDelay = value; }
        }

        public ICursor Cursor { get; set; }

        public INavigationDevice NavigationDevice { get; set; }

//        /// <summary>
//        /// Name of the horizontal axis for movement (if axis events are used).
//        /// </summary>
//        public string horizontalAxis
//        {
//            get { return m_HorizontalAxis; }
//            set { m_HorizontalAxis = value; }
//        }
//
//        /// <summary>
//        /// Name of the vertical axis for movement (if axis events are used).
//        /// </summary>
//        public string verticalAxis
//        {
//            get { return m_VerticalAxis; }
//            set { m_VerticalAxis = value; }
//        }
//
//        public string submitButton
//        {
//            get { return m_SubmitButton; }
//            set { m_SubmitButton = value; }
//        }
//
//        public string cancelButton
//        {
//            get { return m_CancelButton; }
//            set { m_CancelButton = value; }
//        }

        public override void UpdateModule()
        {
            //m_LastMousePosition = m_MousePosition;
            //m_MousePosition = Input.mousePosition;
        }

        public override bool IsModuleSupported()
        {
            // Check for mouse presence instead of whether touch is supported,
            // as you can connect mouse to a tablet and in that case we'd want
            // to use StandaloneInputModule for non-touch input events.
            return m_ForceModuleActive || Input.mousePresent;
        }

//        public override bool ShouldActivateModule()
//        {
//            if (!base.ShouldActivateModule())
//                return false;
//
//            var shouldActivate = m_ForceModuleActive;
//            Input.GetButtonDown(m_SubmitButton);
//            shouldActivate |= Input.GetButtonDown(m_CancelButton);
//            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0.0f);
//            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0.0f);
//            shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
//            shouldActivate |= Input.GetMouseButtonDown(0);
//            return shouldActivate;
//        }

        public override void ActivateModule()
        {
            base.ActivateModule();
            //m_MousePosition = Input.mousePosition;
            //m_LastMousePosition = Input.mousePosition;

            var toSelect = eventSystem.currentSelectedGameObject;
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        public override void ReContextualize(GameObject firstObject) {
            var selectable = firstObject.GetComponentInChildren<Selectable>();
            eventSystem.firstSelectedGameObject = selectable.gameObject;
            eventSystem.SetSelectedGameObject(selectable.gameObject, GetBaseEventData());
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        public override void Process()
        {
            if (Cursor != null && NavigationDevice != null)
            {
                Process(Cursor.Poll(), NavigationDevice.Poll());    
            }
        }

        public void Process(CursorInput cursorInput, NavigationInput navInput) {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject(navInput);

                if (!usedEvent)
                    SendSubmitEventToSelectedObject(navInput);
            }

            ProcessMouseEvent(cursorInput);
        }

        /// <summary>
        /// Process submit keys.
        /// </summary>
        protected bool SendSubmitEventToSelectedObject(NavigationInput input)
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            if (input.SubmitEvent == PointerEventData.FramePressState.Pressed)
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

//            if (input.CancelEvent == PointerEventData.FramePressState.Pressed)
//                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            return data.used;
        }

        /// <summary>
        /// Process keyboard events.
        /// </summary>
        protected bool SendMoveEventToSelectedObject(NavigationInput input)
        {
            float time = Time.unscaledTime;

            Vector2 movement = input.MovementDelta;
            if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
            {
                m_ConsecutiveMoveCount = 0;
                return false;
            }

            // If user pressed key again, always allow event
            //bool allow = Input.GetButtonDown(m_HorizontalAxis) || Input.GetButtonDown(m_VerticalAxis);
            bool allow = false;
            bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);
            if (!allow)
            {
                // Otherwise, user held down key or axis.
                // If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
                if (similarDir && m_ConsecutiveMoveCount == 1)
                    allow = (time > m_PrevActionTime + m_RepeatDelay);
                // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
                else
                    allow = (time > m_PrevActionTime + 1f / m_InputActionsPerSecond);
            }
            if (!allow)
                return false;

            // Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            if (!similarDir)
                m_ConsecutiveMoveCount = 0;
            m_ConsecutiveMoveCount++;
            m_PrevActionTime = time;
            m_LastMoveVector = movement;
            return axisEventData.used;
        }

        private readonly MouseState _mouseState = new MouseState();
        private RayPointerEventData _leftData;
        protected MouseState InputToPointerEvent(CursorInput input) {
            if (_leftData == null) {
                _leftData = new RayPointerEventData(eventSystem);
            }

            // Populate the left button...
            _leftData.Reset();

            //_leftData.delta = input.MovementDelta;
            _leftData.delta = Vector2.zero;
            _leftData.scrollDelta = input.ScrollDelta;
            _leftData.button = PointerEventData.InputButton.Left;
            _leftData.Ray = input.Ray;
            eventSystem.RaycastAll(_leftData, m_RaycastResultCache);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            if (raycast.gameObject != null) {
                //Debug.Log("first raycast " + raycast.gameObject.name, raycast.gameObject);
                //Debug.Log("hit objects: " + string.Join(", ", m_RaycastResultCache.Select(r => r.gameObject.name).ToArray()));
            }
            _leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

//            var raycaster = raycast.module as RayBasedRaycaster;
//            if (raycaster != null) {
//                _leftData.position = raycaster.GetScreenPos(raycast.worldPosition);
//            } else {
//                _leftData.position = input.ScreenPosition;
//            }
            _leftData.position = input.ScreenPosition;
            
            // copy the apropriate data into right and middle slots
//            PointerEventData rightData;
//            GetPointerData(kMouseRightId, out rightData, true);
//            CopyFromTo(_leftData, rightData);
//            rightData.button = PointerEventData.InputButton.Right;
//
//            PointerEventData middleData;
//            GetPointerData(kMouseMiddleId, out middleData, true);
//            CopyFromTo(_leftData, middleData);
//            middleData.button = PointerEventData.InputButton.Middle;
            
            _mouseState.SetButtonState(PointerEventData.InputButton.Left, input.SubmitEvent, _leftData);
//            _mouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, rightData);
//            _mouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, middleData);
            return _mouseState;
        }

        /// <summary>
        /// Exactly the same as the code from PointerInputModule, except that we call our own
        /// IsPointerMoving.
        /// 
        /// This would also not be necessary if PointerEventData.IsPointerMoving was virtual
        /// </summary>
        /// <param name="pointerEvent"></param>
        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (pointerEvent.pointerDrag != null
                && !pointerEvent.dragging
                && ShouldStartDrag(pointerEvent))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && pointerEvent.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        /// <summary>
        /// New version of ShouldStartDrag implemented first in PointerInputModule. This version differs in that
        /// for ray based pointers it makes a decision about whether a drag should start based on the angular change
        /// the pointer has made so far, as seen from the camera. This also works when the world space ray is 
        /// translated rather than rotated, since the beginning and end of the movement are considered as angle from
        /// the same point.
        /// </summary>
        private bool ShouldStartDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.useDragThreshold) {
                return true;
            }

            if (pointerEvent is RayPointerEventData) {
                 // Same as original behaviour for canvas based pointers
                return (pointerEvent.pressPosition - pointerEvent.position).sqrMagnitude >= eventSystem.pixelDragThreshold * eventSystem.pixelDragThreshold;
            } else {
                // When it's not a screen space pointer we have to look at the angle it moved rather than the pixels distance
                // For gaze based pointing screen-space distance moved will always be near 0
                Vector3 cameraPos = pointerEvent.pressEventCamera.transform.position;
                Vector3 pressDir = (pointerEvent.pointerPressRaycast.worldPosition - cameraPos).normalized;
                Vector3 currentDir = (pointerEvent.pointerCurrentRaycast.worldPosition - cameraPos).normalized;
                return Vector3.Dot(pressDir, currentDir) < Mathf.Cos(Mathf.Deg2Rad * (angleDragThreshold));
            }
        }

        /// <summary>
        /// Process all mouse events.
        /// </summary>
        protected void ProcessMouseEvent(CursorInput input)
        {
            var mouseData = InputToPointerEvent(input);
            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
//            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
//            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
//            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
//            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        protected bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        protected void ProcessMousePress(MouseButtonEventData data)
        {
            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (data.PressedThisFrame())
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

//                if (currentOverGo != null) {
//                    DeselectIfSelectionChanged(currentOverGo, pointerEvent);    
//                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (data.ReleasedThisFrame())
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over somethign that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }
            }
        }
    }
}
