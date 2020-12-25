using System.Collections.Generic;
using UnityEngine;

public class IKJoint : MonoBehaviour
{
    private MuscleJoint _joint;
    private IKJoint _parent = null;
    private IList<IKJoint> _children;

    public MuscleJoint Joint
    {
        get { return _joint; }
    }

    public IKJoint Parent
    {
        get { return _parent; }
    }

    public IList<IKJoint> Children
    {
        get { return _children; }
    }

    public void Initialize(MuscleJoint joint)
    {
        _joint = joint;
        _children = new List<IKJoint>();
    }
}
