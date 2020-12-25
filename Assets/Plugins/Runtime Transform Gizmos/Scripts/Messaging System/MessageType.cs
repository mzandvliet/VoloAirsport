namespace RTEditor
{
    /// <summary>
    /// This enum holds the possible types of messages that can be sent to listeners.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// This message can be sent when a gizmo transforms the game objects it controls.
        /// </summary>
        GizmoTransformedObjects = 0,

        /// <summary>
        /// This message is sent when the transform applied to a collection of game objects
        /// via a transform gizmo is undone.
        /// </summary>
        GizmoTransformOperationWasUndone,

        /// <summary>
        /// This message is sent when the transform applied to a collection of game objects
        /// via a transform gizmo is redone.
        /// </summary>
        GizmoTransformOperationWasRedone,

        /// <summary>
        /// This message is sent when the object selection is changed (i.e. when objects are
        /// selected or deselected).
        /// </summary>
        ObjectSelectionChanged,

        /// <summary>
        /// This message is sent when the object selection mode is changed.
        /// </summary>
        ObjectSelectionModeChanged,

        /// <summary>
        /// This message is sent when a collection of game objects is added to the selection mask.
        /// </summary>
        ObjectsAddedToSelectionMask,

        /// <summary>
        /// This message is sent when a collection of game objects is removed from the selection mask.
        /// </summary>
        ObjectsRemovedFromSelectionMask,

        /// <summary>
        /// This message is sent when the transform space is changed. This is the transform space
        /// in which the objects are transformed by the gizmos.
        /// </summary>
        TransformSpaceChanged,

        /// <summary>
        /// This message is sent when the gizmos are turned off.
        /// </summary>
        GizmosTurnedOff,

        /// <summary>
        /// This message is sent when the transform pivot point is changed.
        /// </summary>
        TransformPivotPointChanged,

        /// <summary>
        /// This message is sent when the type of the active gizmo is changed (e.g. when switching
        /// from a translation gizmo to a rotation gizmo).
        /// </summary>
        ActiveGizmoTypeChanged,

        /// <summary>
        /// This message is sent when vertex snapping is enabled.
        /// </summary>
        VertexSnappingEnabled,

        /// <summary>
        /// This message is sent when vertex snapping is disabled.
        /// </summary>
        VertexSnappingDisabled,
    }
}