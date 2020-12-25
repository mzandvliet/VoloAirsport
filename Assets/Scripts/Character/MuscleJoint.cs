using UnityEngine;

public class MuscleJoint : MonoBehaviour
{
    private const float Radius = 0.1f;

    [SerializeField] private MuscleJointLimits _limits;

    private Quaternion _baseLocalRotation;

    public MuscleJointLimits Limits
    {
        get { return _limits; }
    }

    public Quaternion BaseLocalRotation
    {
        get { return _baseLocalRotation; }
    }

    public void Initialize(MuscleJoint joint)
    {
        _baseLocalRotation = joint.BaseLocalRotation;
        _limits = joint.Limits;
    }

    private void Awake()
    {
        _baseLocalRotation = transform.localRotation;

        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = Radius;
    }
}

[System.Serializable]
public class MuscleJointLimits
{
    [SerializeField] public AngularLimit X;
    [SerializeField] public AngularLimit Y;
    [SerializeField] public AngularLimit Z;
}

[System.Serializable]
public class AngularLimit
{
    [SerializeField] public float Min = -90f;
    [SerializeField] public float Max = 90f;
}
