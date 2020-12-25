namespace RTEditor
{
    /// <summary>
    /// Used in conjunction with the scale gizmo and it identifies a multi-axis scale triangle
    /// that can be used by the user to scale objects along 2 axes simultaneously.
    /// </summary>
    public enum MultiAxisTriangle
    {
        /// <summary>
        /// Multi-axis triangle which scales along the X and Y axes.
        /// </summary>
        XY = 0,

        /// <summary>
        /// Multi-axis triangle which scales along the X and Z axes.
        /// </summary>
        XZ,

        /// <summary>
        /// Multi-axis triangle which scales along the Y and Z axes.
        /// </summary>
        YZ,

        /// <summary>
        /// Used in certain situations like specifying that no multi-axis triangle is currently selected.
        /// </summary>
        None
    }
}
