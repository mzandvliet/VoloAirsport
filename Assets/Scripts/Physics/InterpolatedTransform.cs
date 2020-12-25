using UnityEngine;

public class InterpolatedTransform : MonoBehaviour {
    [SerializeField] private Transform _transformA;
    [SerializeField] private Transform _transformB;
    [SerializeField] private float _lerp = 0.5f;
    [SerializeField] private Aligner _aligner;

    private void Start() {
        _aligner.OnAlign += UpdateTransform;
    }
	
	private void UpdateTransform () {
	    transform.position = Vector3.Lerp(_transformA.position, _transformB.position, _lerp);
        transform.rotation = Quaternion.Lerp(_transformA.rotation, _transformB.rotation, _lerp);
	}
}