namespace RTEditor
{
    /// <summary>
    /// Identifies different editor camera rotation modes.
    /// </summary>
    public enum EditorCameraRotationMode
    {
        /// <summary>
        /// Simple look-around style rotation in which the camera rotates
        /// around the global Y axis and its own X axis. Simulates the act
        /// of a person looking around in the surrounding environment.
        /// </summary>
        LookAround = 0,

        /// <summary>
        /// The camera is rotated around a point in space. Both the camera 
        /// position and orientation are affected. The camera will always
        /// look at the orbit point.
        /// </summary>
        Orbit
    }
}
