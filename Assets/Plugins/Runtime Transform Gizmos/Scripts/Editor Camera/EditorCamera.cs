using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Implements the functionality for a runtime editor camera.
    /// </summary>
    public class EditorCamera : MonoSingletonBase<EditorCamera>
    {
        #region Private Variables
        /// <summary>
        /// Holds all zoom related settings.
        /// </summary>
        [SerializeField]
        private EditorCameraZoomSettings _zoomSettings = new EditorCameraZoomSettings();

        /// <summary>
        /// Holds all pan related settings.
        /// </summary>
        [SerializeField]
        private EditorCameraPanSettings _panSettings = new EditorCameraPanSettings();

        /// <summary>
        /// Holds all focus related settings.
        /// </summary>
        [SerializeField]
        private EditorCameraFocusSettings _focusSettings = new EditorCameraFocusSettings();

        /// <summary>
        /// Holds all camera move related settings.
        /// </summary>
        [SerializeField]
        private EditorCameraMoveSettings _moveSettings = new EditorCameraMoveSettings();

        [SerializeField]
        private bool _takeZoomFactorIntoAccount = true;

        /// <summary>
        /// This is the camera rotation speed expressed in degrees/second.
        /// </summary>
        [SerializeField]
        private float _rotationSpeedInDegrees = 8.8f;

        /// <summary>
        /// Cached camera component for easy access.
        /// </summary>
        private Camera _camera;
        private Transform _transform;

        /// <summary>
        /// We will need to have access to mouse information.
        /// </summary>
        private Mouse _mouse = new Mouse();

        /// <summary>
        /// This will be false until the first focus operation is performed. It is used to
        /// decide if a camera orbit operation is possible. Orbiting the camera is only
        /// possible if the camera was previously focused.
        /// </summary>
        private bool _wasFocused = false;

        /// <summary>
        /// This represents an offset along the camera look vector from the camera position
        /// and it allows us to calculate the camera orbit point.
        /// </summary>
        private float _orbitOffsetAlongLook = 0.0f;

        /// <summary>
        /// Identifies the current rotation mode.
        /// </summary>
        private EditorCameraRotationMode _rotationMode = EditorCameraRotationMode.LookAround;

        /// <summary>
        /// If the application is running in windowed mode, the user may press the left mouse button outside
        /// the application window causing the application to loose focus. The problem is that the focus will
        /// only be gained when the user presses a mouse button inside the application window client area. The
        /// 'Update' method of the monobehaviour will not be called while the app doesn't have focus and the 
        /// mouse cursor position in previous frame is not updated accordingly. The moment the user presses
        /// a mouse button inside the window client area, the mouse cursor move offset will be calculated but
        /// will contain wild values which will cause the camera position/orientation to snap. This effect is 
        /// totally undesirable and it can also happen while running the application inside the Unity Editor.
        /// So, every time the application gains focus, we will set this variable to true, so that we can adjust
        /// the mouse cursor position in previous frame properly inside the 'Update' method.
        /// </summary>
        private bool _applicationJustGainedFocus;

        private bool _isObjectVisibilityDirty = true;
        private HashSet<GameObject> _visibleGameObjects = new HashSet<GameObject>();

        [SerializeField]
        private EditorCameraBk _background = new EditorCameraBk();
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum value that the camera rotation speed can have.
        /// </summary>
        public static float MinRotationSpeedInDegrees { get { return 0.01f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the zoom settings.
        /// </summary>
        public EditorCameraZoomSettings ZoomSettings { get { return _zoomSettings; } }

        /// <summary>
        /// Returns the pan settings.
        /// </summary>
        public EditorCameraPanSettings PanSettings { get { return _panSettings; } }

        /// <summary>
        /// Returns the focs settings.
        /// </summary>
        public EditorCameraFocusSettings FocusSettings { get { return _focusSettings; } }

        /// <summary>
        /// Returns the move settings.
        /// </summary>
        public EditorCameraMoveSettings MoveSettings { get { return _moveSettings; } }

        /// <summary>
        /// Gets/sets the camera rotation mode.
        /// </summary>
        public EditorCameraRotationMode RotationMode { get { return _rotationMode; } set { _rotationMode = value; } }

        /// <summary>
        /// Gets/sets the camera rotation speed in degrees. The minimum value that this property can have is given by the 
        /// 'MinRotationSpeedInDegrees' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float RotationSpeedInDegrees { get { return _rotationSpeedInDegrees; } set { _rotationSpeedInDegrees = Mathf.Max(value, MinRotationSpeedInDegrees); } }

        public EditorCameraBk Background { get { return _background; } }
        public bool TakeZoomFactorIntoAccount { get { return _takeZoomFactorIntoAccount; } set { _takeZoomFactorIntoAccount = value; } }

        /// <summary>
        /// Returns the 'Camera' component.
        /// </summary>
        public Camera Camera { get { return _camera; } }
        #endregion

        #region Public Methods
        public void SetObjectVisibilityDirty()
        {
            _isObjectVisibilityDirty = true;
        }

        public void AdjustObjectVisibility(GameObject gameObject)
        {
            if (Camera.IsGameObjectVisible(gameObject)) _visibleGameObjects.Add(gameObject);
            else _visibleGameObjects.Remove(gameObject);
        }

        public List<GameObject> GetVisibleGameObjects()
        {
            if (_isObjectVisibilityDirty)
            {
                _visibleGameObjects = new HashSet<GameObject>(_camera.GetVisibleGameObjects());
                _isObjectVisibilityDirty = false;
            }

            _visibleGameObjects.RemoveWhere(item => item == null);
            return new List<GameObject>(_visibleGameObjects);
        }

        /// <summary>
        /// Performs a camera focus operation on the currently selected game objects. The
        /// method uses the current focs settings to perform the focus operation.
        /// </summary>
        /// <remarks>
        /// The method has no effect if there are no objects currently selected.
        /// </remarks>
        public void FocusOnSelection()
        {
            // No objects selected?
            if (EditorObjectSelection.Instance.NumberOfSelectedObjects == 0) return;

            // Focus the camera based on the chosen focus method
            if (_focusSettings.FocusMode == EditorCameraFocusMode.Instant)
            {
                // Get the focus info
                EditorCameraFocusOperationInfo focusOpInfo = EditorCameraFocus.GetFocusOperationInfo(_camera, _focusSettings);

                // Note: We will also adjust the ortho size to make sure things are all well when
                //       switching from a perspective to orthographic camera.
                _camera.orthographicSize = focusOpInfo.OrthoCameraHalfVerticalSize;
                _camera.transform.position = focusOpInfo.CameraDestinationPosition;
                _camera.nearClipPlane = focusOpInfo.NearClipPlane;

                // The camera was focused
                _wasFocused = true;
                CalculateOrbitOffsetAlongLook(focusOpInfo);
            }
            else
            if(_focusSettings.FocusMode == EditorCameraFocusMode.ConstantSpeed)
            {
                StopCoroutine("StartConstantFocusOnSelection");
                StopCoroutine("StartSmoothZoom");
                StopCoroutine("StartSmoothPan");

                StartCoroutine("StartConstantFocusOnSelection");
            }
            else
            if(_focusSettings.FocusMode == EditorCameraFocusMode.Smooth)
            {
                StopCoroutine("StartSmoothFocusOnSelection");
                StopCoroutine("StartSmoothZoom");
                StopCoroutine("StartSmoothPan");

                StartCoroutine("StartSmoothFocusOnSelection");
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called when the game object is created and we will use this to 
        /// perform any necessary initializations.
        /// </summary>
        private void Awake()
        {
            // Cache needed data.
            // Note: If a camera component wasn't attached, we will create one.
            _camera = this.gameObject.GetComponent<Camera>();
            if (_camera == null) _camera = this.gameObject.AddComponent<Camera>();

            _transform = transform;
        }

        /// <summary>
        /// Called every frame to perform any necessary updates.
        /// </summary>
        private void Update()
        {
            // Reset the mouse cursor position in the previous frame in order to make sure we don't 
            // get wild offset values which will cause unwanted effects.
            if (_applicationJustGainedFocus)
            {
                _applicationJustGainedFocus = false;
                _mouse.ResetCursorPositionInPreviousFrame();
            }

            // Make sure the mouse has its information updated for the current frame update
            _mouse.UpdateInfoForCurrentFrame();

            if (_zoomSettings.IsZoomEnabled) ApplyCameraZoomBasedOnUserInput();
            PanCameraBasedOnUserInput();
            RotateCameraBasedOnUserInput();
            MoveCameraBasedOnUserInput();

            _background.OnCameraUpdate(Camera);

            if(_transform.hasChanged)
            {
                SetObjectVisibilityDirty();
                _transform.hasChanged = false;
            }
        }

        /// <summary>
        /// Applies any necessary zoom to the camera based on user input. 
        /// </summary>
        private void ApplyCameraZoomBasedOnUserInput()
        {
            // Zoom if necessary
            float scrollSpeed = Input.GetAxis("Mouse ScrollWheel");
            if(scrollSpeed != 0.0f)
            {
                // Make sure all coroutines are stopped to avoid any conflicts
                StopAllCoroutines();

                // Zoom based on the active zoom mode
                if (_zoomSettings.ZoomMode == EditorCameraZoomMode.Standard)
                {
                    // Note: We will use the mouse scroll wheel for zooming and we will establish
                    //       the zoom speed based on the camera type.
                    float zoomSpeed = _camera.orthographic ? _zoomSettings.OrthographicStandardZoomSpeed : _zoomSettings.PerspectiveStandardZoomSpeed * Time.deltaTime;
                    if (TakeZoomFactorIntoAccount) zoomSpeed *= CalculateZoomFactor();
                    EditorCameraZoom.ZoomCamera(_camera, scrollSpeed * zoomSpeed);
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine("StartSmoothZoom");
                }
            }
        }

        /// <summary>
        /// Pans the camera based on user input.
        /// </summary>
        private void PanCameraBasedOnUserInput()
        {
            // Only pan if the middle mouse button is down
            if (_mouse.IsMiddleMouseButtonDown && _mouse.WasMouseMovedSinceLastFrame)
            {
                // Make sure all coroutines are stopped to avoid any conflicts
                StopAllCoroutines();

                // Pan based on the chosen pan mode
                if(_panSettings.PanMode == EditorCameraPanMode.Standard)
                {
                    float panSpeedTimesDeltaTime = Time.deltaTime * _panSettings.StandardPanSpeed;
                    EditorCameraPan.PanCamera(_camera,
                                              -_mouse.CursorOffsetSinceLastFrame.x * panSpeedTimesDeltaTime * (_panSettings.InvertXAxis ? -1.0f : 1.0f),
                                              -_mouse.CursorOffsetSinceLastFrame.y * panSpeedTimesDeltaTime * (_panSettings.InvertYAxis ? -1.0f : 1.0f));
                }
                else StartCoroutine(StartSmoothPan());
            }
        }

        /// <summary>
        /// Rotates the camera based on user input.
        /// </summary>
        private void RotateCameraBasedOnUserInput()
        {
            // Only rotate if the right mouse button is pressed
            if (_mouse.IsRightMouseButtonDown && _mouse.WasMouseMovedSinceLastFrame)
            {
                // Make sure all coroutines are stopped to avoid any conflicts
                StopAllCoroutines();

                // Calculate the amount of rotation which must be applied
                float rotationSpeedTimesDeltaTime = _rotationSpeedInDegrees * Time.deltaTime;

                // Rotate based on the type of rotation we are dealing with.
                // Note: Even if the rotation mode is set to orbit, we will still perform a 'LookAround' rotation
                //       if the camera hasn't been focused.
                if(_rotationMode == EditorCameraRotationMode.LookAround || !_wasFocused)
                {
                    EditorCameraRotation.RotateCamera(_camera,
                                                      -_mouse.CursorOffsetSinceLastFrame.y * rotationSpeedTimesDeltaTime,
                                                      _mouse.CursorOffsetSinceLastFrame.x * rotationSpeedTimesDeltaTime);
                }
                else
                if(_wasFocused && _rotationMode == EditorCameraRotationMode.Orbit)
                {
                    // Calculate the orbit point. This is done by moving from the camera position along the camera
                    // look vector by a distance equal to '_orbitOffsetAlongLook'.
                    Transform cameraTransform = _camera.transform;
                    Vector3 orbitPoint = cameraTransform.position + cameraTransform.forward * _orbitOffsetAlongLook;

                    EditorCameraOrbit.OrbitCamera(_camera,
                                                  -_mouse.CursorOffsetSinceLastFrame.y * rotationSpeedTimesDeltaTime,
                                                  _mouse.CursorOffsetSinceLastFrame.x * rotationSpeedTimesDeltaTime, orbitPoint);
                }
            }
        }

        /// <summary>
        /// Moves the camera based on user supplied input.
        /// </summary>
        private void MoveCameraBasedOnUserInput()
        {
            if(_mouse.IsRightMouseButtonDown)
            {
                float moveAmount = _moveSettings.MoveSpeed * Time.deltaTime;
                if (TakeZoomFactorIntoAccount) moveAmount *= CalculateZoomFactor();

                Transform cameraTransform = _camera.transform;
                if (Input.GetKey(KeyCode.W))
                {
                    if (_camera.orthographic) EditorCameraZoom.ZoomCamera(_camera, moveAmount);
                    else cameraTransform.position += cameraTransform.forward * moveAmount;
                }
                else
                if (Input.GetKey(KeyCode.S))
                {
                    if (_camera.orthographic) EditorCameraZoom.ZoomCamera(_camera, -moveAmount);
                    else cameraTransform.position -= cameraTransform.forward * moveAmount;
                }

                if (Input.GetKey(KeyCode.A)) cameraTransform.position -= cameraTransform.right * moveAmount;
                else if (Input.GetKey(KeyCode.D)) cameraTransform.position += cameraTransform.right * moveAmount;

                if (Input.GetKey(KeyCode.Q)) cameraTransform.position -= cameraTransform.up * moveAmount;
                else if (Input.GetKey(KeyCode.E)) cameraTransform.position += cameraTransform.up * moveAmount;
            }
        }

        private float CalculateZoomFactor()
        {
            float distFromXZGrid = RuntimeEditorApplication.Instance.XZGrid.Plane.GetDistanceToPoint(transform.position);
            return Mathf.Max(1.0f, (distFromXZGrid / 10.0f));
        }

        /// <summary>
        /// Called when the focus of the application window changes.
        /// </summary>
        private void OnApplicationFocus(bool focusStatus)
        {
            // If the application gained focus, store this state in the '_applicationJustGainedFocus' variable.
            // We will need this information inside the 'Update' method to make sure that the mouse cursor
            // move offset doesn't contain wild values.
            // Note: Settings the mouse cursor position here doesn't seem to produce the correct results.
            if (focusStatus) _applicationJustGainedFocus = true;
        }

        /// <summary>
        /// Given a focus operation info instance, the method calculates the orbit offset
        /// along the camera look vector which can be used to perform orbit operations.
        /// </summary>
        private void CalculateOrbitOffsetAlongLook(EditorCameraFocusOperationInfo focusOpInfo)
        {
            _orbitOffsetAlongLook = (_camera.transform.position - focusOpInfo.FocusPoint).magnitude;
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Starts a smooth zoom operation.
        /// </summary>
        private IEnumerator StartSmoothZoom()
        {
            // Calculate the camera initial speed and the smooth value based on the camera type
            float currentSpeed = (_camera.orthographic ? _zoomSettings.OrthographicSmoothZoomSpeed : _zoomSettings.PerspectiveSmoothZoomSpeed) * Input.GetAxis("Mouse ScrollWheel");
            float smoothValue = _camera.orthographic ? _zoomSettings.OrthographicSmoothValue : _zoomSettings.PerspectiveSmoothValue;
            if (TakeZoomFactorIntoAccount) currentSpeed *= CalculateZoomFactor();

            while (true)
            {
                // Zoom the camera using the current speed
                EditorCameraZoom.ZoomCamera(_camera, currentSpeed * Time.deltaTime);

                // Move from the current speed towards 0 using the smooth value
                currentSpeed = Mathf.Lerp(currentSpeed, 0.0f, smoothValue);

                // Exit if the speed is small enough
                if (Mathf.Abs(currentSpeed) < 1e-5f) break;

                // Wait for the next frame
                yield return null;
            }
        }

        /// <summary>
        /// Starts a smooth pan operation.
        /// </summary>
        private IEnumerator StartSmoothPan()
        {
            // Calculate the camera initial speed and store the smooth value
            float panSpeedRightAxis = -_mouse.CursorOffsetSinceLastFrame.x * _panSettings.SmoothPanSpeed * (_panSettings.InvertXAxis ? -1.0f : 1.0f);
            float panSpeedUpAxis = -_mouse.CursorOffsetSinceLastFrame.y * _panSettings.SmoothPanSpeed * (_panSettings.InvertYAxis ? -1.0f : 1.0f);
            float smoothValue = _panSettings.SmoothValue;

            while (true)
            {
                // Pan the camera using the current speed along the camera right and up axes
                EditorCameraPan.PanCamera(_camera, panSpeedRightAxis * Time.deltaTime, panSpeedUpAxis * Time.deltaTime);

                // Move from the current speed towards 0 using the smooth value
                panSpeedRightAxis = Mathf.Lerp(panSpeedRightAxis, 0.0f, smoothValue);
                panSpeedUpAxis = Mathf.Lerp(panSpeedUpAxis, 0.0f, smoothValue);

                // Exit if both speed values are small enough
                if (Mathf.Abs(panSpeedRightAxis) < 1e-5f && Mathf.Abs(panSpeedUpAxis) < 1e-5f) break;

                // Wait for the next frame
                yield return null;
            }
        }

        /// <summary>
        /// Starts a constant focus operation on the current object selection.
        /// </summary>
        private IEnumerator StartConstantFocusOnSelection()
        {
            // Store needed data
            EditorCameraFocusOperationInfo focusOpInfo = EditorCameraFocus.GetFocusOperationInfo(_camera, _focusSettings);
            Vector3 cameraDestinationPoint = focusOpInfo.CameraDestinationPosition;
            float cameraSpeed = _focusSettings.ConstantFocusSpeed;
            Transform cameraTransform = _camera.transform;

            // Calculate the vector which is used to move the camera from its current position to the destination position.
            // We will normalize this vector so that we can move along it with the required speed.
            Vector3 fromCamPosToDestination = cameraDestinationPoint - cameraTransform.position;
            float distanceToTravel = fromCamPosToDestination.magnitude;     // Needed later inside the 'while' loop
            if(distanceToTravel < 1e-4f) yield break;                       // No focus necessary?
            fromCamPosToDestination.Normalize();

            // We will need this to know how much we travelled
            Vector3 initialCameraPosition = cameraTransform.position;

            // We will need this to adjust the ortho camera size
            float initialCameraOrthoSize = _camera.orthographicSize;

            // The first iteration of the 'while' loop will perform the first focus step. We will
            // set this to true here just in case the focus operation is cancelled by another camera
            // operation. This will allow the user to orbit the camera even if it wasn't focused 100%.
            _wasFocused = true;

            while(true)
            {
                // Move the camera along the direction vector with the desired speed
                cameraTransform.position += (fromCamPosToDestination * cameraSpeed * Time.deltaTime);

                // Calculate the new camera ortho size. This is done by lerping between the initial
                // ortho size and the target size. The interpolation factor is the ratio between how
                // much we travelled so far and the total distance that we have to travel.
                float distanceTraveledSoFar = (cameraTransform.position - initialCameraPosition).magnitude;
                _camera.orthographicSize = Mathf.Lerp(initialCameraOrthoSize, focusOpInfo.OrthoCameraHalfVerticalSize, distanceTraveledSoFar / distanceToTravel);

                // Recalculate the orbit focus to ensure proper orbit if the focus operation is not completed 100%.
                CalculateOrbitOffsetAlongLook(focusOpInfo);

                // Check if the camera has reached the destination position or if it went past it. It is very 
                // probable that most of the times, the camera will move further away than the target position.
                // When that happens, we will clamp the camera position and exit the coroutine. In order to detect
                // this situation, we will perform a dot product between the camera move direction vector and
                // the vector which unites the current camera position with the target position. When this dot
                // product is < 0, it means that the camera has moved too far away and we will clamp its position
                // and exit. This could also probably be done using a variable which holds the accumulated travel
                // distance. When this distance becomes >= than the original length of 'fromCamPosToDestination',
                // we can exit.
                if(Vector3.Dot(fromCamPosToDestination, cameraDestinationPoint - cameraTransform.position) <= 0.0f &&
                   Mathf.Abs(_camera.orthographicSize - focusOpInfo.OrthoCameraHalfVerticalSize) < 1e-3f)
                {
                    cameraTransform.position = cameraDestinationPoint;
                    break;
                }

                yield return null;
            }

            _camera.nearClipPlane = focusOpInfo.NearClipPlane;
        }

        /// <summary>
        /// Starts a smooth focus operation on the current object selection.
        /// </summary>
        private IEnumerator StartSmoothFocusOnSelection()
        {
            // Store needed data
            EditorCameraFocusOperationInfo focusOpInfo = EditorCameraFocus.GetFocusOperationInfo(_camera, _focusSettings);
            Vector3 cameraDestinationPoint = focusOpInfo.CameraDestinationPosition;
            Transform cameraTransform = _camera.transform;

            // If the distance to travel is small enough, we can exit
            if ((cameraDestinationPoint - cameraTransform.position).magnitude < 1e-4f) yield break;

            // We will need this to modify the position using 'Vector3.SmoothDamp'
            Vector3 velocity = Vector3.zero;

            // We will need this to modify the camera ortho size using 'Mathf.SmoothDamp'.
            float orthoSizeVelocity = 0.0f;

            // The first iteration of the 'while' loop will perform the first focus step. We will
            // set this to true here just in case the focus operation is cancelled by another camera
            // operation. This will allow the user to orbit the camera even if it wasn't focused 100%.
            _wasFocused = true;

            while (true)
            {
                // Calculate the new position
                cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, cameraDestinationPoint, ref velocity, _focusSettings.SmoothFocusTime);

                // Calculate the new camera ortho size
                _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, focusOpInfo.OrthoCameraHalfVerticalSize, ref orthoSizeVelocity, _focusSettings.SmoothFocusTime);

                // Recalculate the orbit focus to ensure proper orbit if the focus operation is not completed 100%.
                CalculateOrbitOffsetAlongLook(focusOpInfo);

                // If the position is close enough to the target position and the camera ortho size
                // is close enough to the target size, we can exit the loop.
                if ((cameraTransform.position - cameraDestinationPoint).magnitude < 1e-3f &&
                    Mathf.Abs(_camera.orthographicSize - focusOpInfo.OrthoCameraHalfVerticalSize) < 1e-3f)
                {
                    // Clamp to make sure we got the correct values and then exit the loop
                    cameraTransform.position = cameraDestinationPoint;
                    _camera.orthographicSize = focusOpInfo.OrthoCameraHalfVerticalSize;

                    break;
                }

                yield return null;
            }

            _camera.nearClipPlane = focusOpInfo.NearClipPlane;
        }
        #endregion
    }
}
