using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Can be used to store useful data about the mouse like button states and cursor offset.
    /// </summary>
    public class Mouse
    {
        #region Private Variables
        /// <summary>
        /// These variables hold the current mouse button states.
        /// </summary>
        private bool _isLeftMouseButtonDown;
        private bool _isRightMouseButtonDown;
        private bool _isMiddleMouseButtonDown;

        /// <summary>
        /// These variables are used to supply information about which mouse buttons were pressed
        /// during the current frame update.
        /// </summary>
        private bool _wasLeftMouseButtonPressedInCurrentFrame;
        private bool _wasRightMouseButtonPressedInCurrentFrame;
        private bool _wasMiddleMouseButtonPressedInCurrentFrame;

        /// <summary>
        /// These variables are used to supply information about which mouse buttons were released
        /// during the current frame update.
        /// </summary>
        private bool _wasLeftMouseButtonReleasedInCurrentFrame;
        private bool _wasRightMouseButtonReleasedInCurrentFrame;
        private bool _wasMiddleMouseButtonReleasedInCurrentFrame;

        /// <summary>
        /// The cursor position in the previous frame update.
        /// </summary>
        private Vector2 _cursorPositionInPreviousFrame;

        /// <summary>
        /// The cursor offset since the last frame update.
        /// </summary>
        private Vector2 _cursorOffsetSinceLastFrame;

        /// <summary>
        /// These variables hold the cursor offset since the 3 mouse buttons (left, right
        /// and middle) were pressed. These will always equal the zero vector as long as the
        /// mouse buttons are not pressed.
        /// </summary>
        private Vector2 _cursorOffsetSinceLeftMouseButtonDown;
        private Vector2 _cursorOffsetSinceRightMouseButtonDown;
        private Vector2 _cursorOffsetSinceMiddleMouseButtonDown;
        #endregion

        #region Public Properties
        /// <summary>
        /// Checks if the left mouse button is currently pressed.
        /// </summary>
        public bool IsLeftMouseButtonDown { get { return _isLeftMouseButtonDown; } }

        /// <summary>
        /// Checks if the right mouse button is currently pressed.
        /// </summary>
        public bool IsRightMouseButtonDown { get { return _isRightMouseButtonDown; } }

        /// <summary>
        /// Check if the middle mouse button is currently pressed.
        /// </summary>
        public bool IsMiddleMouseButtonDown { get { return _isMiddleMouseButtonDown; } }

        /// <summary>
        /// Checks if the left mouse button was pressed in the current frame update.
        /// </summary>
        public bool WasLeftMouseButtonPressedInCurrentFrame { get { return _wasLeftMouseButtonPressedInCurrentFrame; } }

        /// <summary>
        /// Checks if the right mouse button was pressed in the current frame update.
        /// </summary>
        public bool WasRightMouseButtonPressedInCurrentFrame { get { return _wasRightMouseButtonPressedInCurrentFrame; } }

        /// <summary>
        /// Checks if the middle mouse button was pressed in the current frame update.
        /// </summary>
        public bool WasMiddleMouseButtonPressedInCurrentFrame { get { return _wasMiddleMouseButtonPressedInCurrentFrame; } }

        /// <summary>
        /// Checks if the left mouse button was released in the current frame update.
        /// </summary>
        public bool WasLeftMouseButtonReleasedInCurrentFrame { get { return _wasLeftMouseButtonReleasedInCurrentFrame; } }

        /// <summary>
        /// Checks if the right mouse button was released in the current frame update.
        /// </summary>
        public bool WasRightMouseButtonReleasedInCurrentFrame { get { return _wasRightMouseButtonReleasedInCurrentFrame; } }

        /// <summary>
        /// Checks if the middle mouse button was released in the current frame update.
        /// </summary>
        public bool WasMiddleMouseButtonReleasedInCurrentFrame { get { return _wasMiddleMouseButtonReleasedInCurrentFrame; } }

        /// <summary>
        /// Returns the cursor position in the previous frame update.
        /// </summary>
        public Vector2 CursorPositionInPreviousFrame { get { return _cursorPositionInPreviousFrame; } }

        /// <summary>
        /// Returns the cursor offset since the last frame update.
        /// </summary>
        public Vector2 CursorOffsetSinceLastFrame { get { return _cursorOffsetSinceLastFrame; } }

        /// <summary>
        /// Checks if the mouse was moved since the last frame update.
        /// </summary>
        public bool WasMouseMovedSinceLastFrame { get { return _cursorOffsetSinceLastFrame.magnitude != 0.0f; } }

        /// <summary>
        /// Returns the cursor offset since the left mouse button was pressed.
        /// </summary>
        public Vector2 CursorOffsetSinceLeftMouseButtonDown { get { return _cursorOffsetSinceLeftMouseButtonDown; } }

        /// <summary>
        /// Returns the cursor offset since the right mouse button was pressed.
        /// </summary>
        public Vector2 CursorOffsetSinceRightMouseButtonDown { get { return _cursorOffsetSinceRightMouseButtonDown; } }

        /// <summary>
        /// Returns the cursor offset since the middle mouse button was pressed.
        /// </summary>
        public Vector2 CursorOffsetSinceMiddleMouseButtonDown { get { return _cursorOffsetSinceMiddleMouseButtonDown; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mouse()
        {
            // We have to initialize this to a sensible value.
            // Note: Constructors are called from the loading thread. Unity will complain if we
            //       call 'Input.mousePosition' here, so we will have to initialize this to the
            //       zero vector. This is not as bad as it sounds. This will quickly be fixed
            //       during the first call to 'UpdateInfoForCurrentFrame'.
            _cursorPositionInPreviousFrame = Vector2.zero;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the mouse information for the current frame update.
        /// </summary>
        public void UpdateInfoForCurrentFrame()
        {
            // Note: Order is important.
            UpdateMouseButtonStatesForCurrentFrame();
            UpdateCursorPositionAndOffsetInfoForCurrentFrame();
        }

        /// <summary>
        /// Resets the mouse cursor position in the previous frame update to the current mouse position. 
        /// May be useful when you need to tackle more tricky situations like the application loosing
        /// and gaining focus.
        /// </summary>
        public void ResetCursorPositionInPreviousFrame()
        {
            _cursorPositionInPreviousFrame = Input.mousePosition;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the mouse button states for the current frame update.
        /// </summary>
        private void UpdateMouseButtonStatesForCurrentFrame()
        {
            // Check which mouse buttons were pressed during the current frames
            _wasLeftMouseButtonPressedInCurrentFrame = InputHelper.WasLeftMouseButtonPressedInCurrentFrame();
            _wasRightMouseButtonPressedInCurrentFrame = InputHelper.WasRightMouseButtonPressedInCurrentFrame();
            _wasMiddleMouseButtonPressedInCurrentFrame = InputHelper.WasMiddleMouseButtonPressedInCurrentFrame();

            // Check which mouse buttons were released during the current frame
            _wasLeftMouseButtonReleasedInCurrentFrame = InputHelper.WasLeftMouseButtonReleasedInCurrentFrame();
            _wasRightMouseButtonReleasedInCurrentFrame = InputHelper.WasRightMouseButtonReleasedInCurrentFrame();
            _wasMiddleMouseButtonReleasedInCurrentFrame = InputHelper.WasMiddleMouseButtonReleasedInCurrentFrame();

            // If a mouse button was pressed, update the state of the button accordingly
            if (_wasLeftMouseButtonPressedInCurrentFrame) _isLeftMouseButtonDown = true;
            if (_wasRightMouseButtonPressedInCurrentFrame) _isRightMouseButtonDown = true;
            if (_wasMiddleMouseButtonPressedInCurrentFrame) _isMiddleMouseButtonDown = true;

            // If a mouse button was released, update the state of the button accordingly
            if (_wasLeftMouseButtonReleasedInCurrentFrame) _isLeftMouseButtonDown = false;
            if (_wasRightMouseButtonReleasedInCurrentFrame) _isRightMouseButtonDown = false;
            if (_wasMiddleMouseButtonReleasedInCurrentFrame) _isMiddleMouseButtonDown = false;
        }

        /// <summary>
        /// Updates the cursor position and offset information for the current frame update.
        /// </summary>
        private void UpdateCursorPositionAndOffsetInfoForCurrentFrame()
        {
            Vector2 cursorPosition = Input.mousePosition;

            // Update the cursor offset since the last frame
            _cursorOffsetSinceLastFrame = cursorPosition - _cursorPositionInPreviousFrame;

            // Update the cursor offsets since the left/right/middle buttons were pressed.
            // Note: We only do this for the buttons which are currently pressed.
            if (_isLeftMouseButtonDown) _cursorOffsetSinceLeftMouseButtonDown += _cursorOffsetSinceLastFrame;
            if (_isRightMouseButtonDown) _cursorOffsetSinceRightMouseButtonDown += _cursorOffsetSinceLastFrame;
            if (_isMiddleMouseButtonDown) _cursorOffsetSinceMiddleMouseButtonDown += _cursorOffsetSinceLastFrame;

            // When a button is released during the current frame update, we will reset the corresponding
            // cursor offsets so that we can start anew the next time the buttons are pressed.
            if (_wasLeftMouseButtonReleasedInCurrentFrame) _cursorOffsetSinceLeftMouseButtonDown = Vector2.zero;
            if (_wasRightMouseButtonReleasedInCurrentFrame) _cursorOffsetSinceRightMouseButtonDown = Vector2.zero;
            if (_wasMiddleMouseButtonReleasedInCurrentFrame) _cursorOffsetSinceMiddleMouseButtonDown = Vector2.zero;

            // Store the mouse position in the current frame update so that we can use it in the next one
            _cursorPositionInPreviousFrame = cursorPosition;
        }
        #endregion
    }
}
