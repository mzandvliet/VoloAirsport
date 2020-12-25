namespace RTEditor
{
    /// <summary>
    /// Used in conjunction with the translation gizmo and it identifies a multi-axis
    /// translation square that can be used by the user to translate objects along 2 
    /// axes simultaneously.
    /// </summary>
    public enum MultiAxisSquare
    {
        /// <summary>
        /// Multi-axis square which translates along the X and Y axes.
        /// </summary>
        XY = 0,

        /// <summary>
        /// Multi-axis square which translates along the X and Z axes.
        /// </summary>
        XZ,

        /// <summary>
        /// Multi-axis square which translates along the Y and Z axes.
        /// </summary>
        YZ,

        /// <summary>
        /// Used in certain situations like specifying that no multi-axis square is currently selected.
        /// </summary>
        None
    }
}
