namespace RTEditor
{
    /// <summary>
    /// Holds the possible spaces in which objects can be transformed using gizmos. 
    /// </summary>
    /// <remarks>
    /// The scale gizmo represents a special case because it is not affected by the
    /// active transform space. It will always perform scale operations in object 
    /// local space. This is because scaling in global space would require us to
    /// adjust an object's transform matrix directly which we can not do. It would
    /// be possible to achieve scale along the global axis using a trick described
    /// here: http://answers.unity3d.com/questions/13436/how-to-scale-an-object-along-an-arbitrary-axis-in.html
    /// Applying this trick in the Unity Editor seems to produce some undesirable
    /// results. Moreover, keeping things simple is always a good idea :D, so we will
    /// always scale along the objects' local axes.
    /// </remarks>
    public enum TransformSpace
    {
        /// <summary>
        /// The transform is applied using an object's local axes. 
        /// Here is the gizmo behavior when this transform space is active:
        ///     a) translation gizmo -> will perform translations along the objects' local axes;
        ///     b) rotation gizmo -> will peform rotations along the objects' local axes;
        ///     c) scale gizmo -> will perform scale operations along the objects' local axes.
        /// </summary>
        Local = 0,

        /// <summary>
        /// The transform is applied using the global world axes.
        /// Here is the gizmo behavior when this transform space is active:
        ///     a) translation gizmo -> will perform translations along the world axes;
        ///     b) rotation gizmo -> will peform rotations along the world axes;
        ///     c) scale gizmo -> will perform scale operations along the objects' local axes.
        /// </summary>
        Global
    }
}
