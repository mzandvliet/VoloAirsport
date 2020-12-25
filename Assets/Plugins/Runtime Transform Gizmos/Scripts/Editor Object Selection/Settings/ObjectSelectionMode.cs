namespace RTEditor
{
    /// <summary>
    /// Identifies different modes in which object selection can be performed.
    /// </summary>
    public enum ObjectSelectionMode
    {
        /// <summary>
        /// Individual objects are added to the selection as they are clicked in the scene
        /// or when they enter the area of the object selection shape.
        /// </summary>
        IndividualObjects = 0,

        /// <summary>
        /// When a game object is selected in the scene, the entire hierarchy to which the
        /// object belongs becomes selected. 
        /// </summary>
        EntireHierarchy,

        /// <summary>
        /// Custom object selection. When this mode is active, it is the responsibility of the
        /// developer to implement the necessary selection handlers.
        /// </summary>
        Custom
    }
}
