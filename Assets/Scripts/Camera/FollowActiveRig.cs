using UnityEngine;

public class FollowActiveRig : MonoBehaviour {
    [SerializeField] private Transform _target;
    [SerializeField] private bool _followPosition = true;
    [SerializeField] private bool _followRotation = false;
    private ICameraRig _cameraRig;

	private void Update () {
	    if (_followPosition) {
            transform.position = _target.position;
	    }
	    if (_followRotation) {
            transform.rotation = _target.rotation;
	    }
	}
}
