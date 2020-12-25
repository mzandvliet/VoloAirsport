using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class SpawnpointCameraAnimator : MonoBehaviour {

    [SerializeField, Dependency] private AbstractUnityClock _clock;
    [SerializeField, Dependency] private GameSettingsProvider _gameSettingsProvider;
    [SerializeField] private Transform _lookTarget;
    [SerializeField] private Vector3 _radii = new Vector3(1000f, 1000f, 1000f);
    [SerializeField] private float _translationSpeed = 0.1f;
    [SerializeField] private float _rotationSpeed = 0.1f;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public Transform LookTarget {
        get { return _lookTarget; }
        set { _lookTarget = value; }
    }

    private void Awake() {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

	private void Update () {
	    if (!_gameSettingsProvider.IsVrActive) {
	        transform.position = _originalPosition +
                                 new Vector3(
                                     (float) (Math.Sin(_clock.CurrentTime * _translationSpeed) / Math.PI * _radii.x),
                                     (float) (Math.Sin(_clock.CurrentTime * _translationSpeed) / Math.PI * _radii.y),
                                     (float) (Math.Cos(_clock.CurrentTime * _translationSpeed) / Math.PI * _radii.z));

            Quaternion targetRotation = _lookTarget ? Quaternion.LookRotation(_lookTarget.position - transform.position, Vector3.up) : transform.rotation;
            targetRotation = Quaternion.Slerp(_originalRotation, targetRotation, 0.85f);
            Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, _clock.DeltaTime * _rotationSpeed);

	        transform.rotation = smoothRotation;
	    }
	}
}
