namespace RTEditor
{
    /// <summary>
    /// This enum defines different object selection box styles.
    /// </summary>
    public enum ObjectSelectionBoxStyle
    {
        /// <summary>
        /// This represents a selection box where 3 lines meet at each corner of the box.
        /// </summary>
        CornerLines = 0,

        /// <summary>
        /// This represents a wire selection box. A line exists between each pair of adjacent 
        /// corner points.
        /// </summary>
        WireBox
    }
}
