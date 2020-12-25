namespace RTEditor
{
    /// <summary>
    /// Represents a gizmo axis. This enum is used by all gizmo types to
    /// differentiate between their X, Y and Z axes.
    /// </summary>
    public enum GizmoAxis
    {
        /// <summary>
        /// The gizmo X axis.
        /// </summary>
        X = 0,

        /// <summary>
        /// The gizmo Y axis.
        /// </summary>
        Y,

        /// <summary>
        /// The gimzo Z axis.
        /// </summary>
        Z,

        /// <summary>
        /// Used in certain situations like specifying that no gizmo axis is currently selected.
        /// </summary>
        None
    }
}
