namespace RTEditor
{
    /// <summary>
    /// This enum holds the possible pivot points which can be used while 
    /// transforming objects with gizmos.
    /// </summary>
    public enum TransformPivotPoint
    {
        /// <summary>
        /// Gizmos will transform the objects using the objects' mesh pivot point.
        /// </summary>
        MeshPivot = 0,

        /// <summary>
        /// Gizmos will transform the objects around the center of the selection.
        /// </summary>
        Center
    }
}
