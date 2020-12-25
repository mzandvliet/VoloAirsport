namespace RTEditor
{
    /// <summary>
    /// Identifies different types of camera focus modes.
    /// </summary>
    public enum EditorCameraFocusMode
    {
        /// <summary>
        /// Standard focus. The camera position is moved to the established
        /// destination point using a constant speed.
        /// </summary>
        ConstantSpeed = 0,

        /// <summary>
        /// Smooth focus. The camera speed slowly decreases over time.
        /// </summary>
        Smooth,

        /// <summary>
        /// Instant focus. The camera is instantly moved to the established focus
        /// destination position.
        /// </summary>
        Instant
    }
}
