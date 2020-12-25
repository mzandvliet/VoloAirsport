using UnityEngine;

public class Boid : MonoBehaviour {
    [SerializeField]
    private Transform _transform;
    [SerializeField]
    private Animation _animation;

    public Transform Transform {
        get { return _transform; }
    }

    public Animation Animation {
        get { return _animation; }
    }
}
