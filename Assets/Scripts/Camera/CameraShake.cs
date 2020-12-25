using System;
using RamjetAnvil.DependencyInjection;
using UnityEngine;
using RamjetAnvil.Volo;
using UnityEngine.VR;

/*
 * Create generic shake interface. Refrain from implementing game/vehicle specific logic here
 * 
 * Game specific tricks to implement elsewhere:
 * - Impulse on collisions
 * - Acceleration
 * - Intensity on speeding past close-by geometry
 * 
 * Those bits of logic need to be pointed to the relevant active vehicle, the data source.
 * Camera Manager?
 */
public class CameraShake : MonoBehaviour {

    [SerializeField, Dependency] private AbstractUnityEventSystem _eventSystem;
    [SerializeField] private Wingsuit _target;

    [SerializeField] private float _baseIntensity = 0.01f;
    [SerializeField] private float _baseFrequency = 10f;
    [SerializeField] private float _maxResidualImpulse = 30f;
    [SerializeField] private float _residualImpulseDampeningSpeed = 5f;
    [SerializeField] private float _maxCollisionSpeed = 50f;
    [SerializeField] private float _collisionIntensity = 7f;
    [SerializeField] private float _groundShakeAltitude = 100f;
    [SerializeField] private float _groundShakeIntensity = 1f;
    [SerializeField] private float _maxAirspeed = 80f;
    [SerializeField] private float _airspeedIntensity = .5f;
    [SerializeField] private float _maxAngularSpeed = 3f;
    [SerializeField] private float _angularSpeedIntensity = .5f;

    private Transform _transform;
    private float _amplitude = 1f;
    private float _residualImpulse;

    public float BaseIntensity {
        get { return _baseIntensity; }
        set { _baseIntensity = value; }
    }

    public float Amplitude {
        get { return _amplitude; }
        set { _amplitude = value; }
    }
	
    public void AddImpulse(float impulse) {
        _residualImpulse = Mathf.Clamp(_residualImpulse + impulse, 0f, _maxResidualImpulse);
    }

    void Awake() {
        _transform = gameObject.GetComponent<Transform>();
    }

    public void SetTarget(Wingsuit pilot) {
        // Unsubscribe from old player's events
        if (_target && _target.CollisionEventSource) {
            _target.CollisionEventSource.OnCollisionEntered -= OnPlayerCollisionEntered;
        }

        _target = pilot;

        // Subscribe to new player's events
        _target.CollisionEventSource.OnCollisionEntered += OnPlayerCollisionEntered;
    }

    private void OnPlayerCollisionEntered(CollisionEventSource source, Collision collision) {
        float impulse = Mathf.Pow(Mathf.Clamp01(collision.relativeVelocity.magnitude/_maxCollisionSpeed), 2f) *_collisionIntensity;
        AddImpulse(impulse);
    }

	private void Update () {
	    _residualImpulse = Mathf.Lerp(_residualImpulse, 0f, Time.deltaTime*_residualImpulseDampeningSpeed);
	    
	    float intensity = _baseIntensity + _residualImpulse;

        if (_target) {
            float speedIntensity = Mathf.Clamp01(_target.FlightStatistics.TrueAirspeed / _maxAirspeed);
            speedIntensity = Mathf.Pow(speedIntensity, 2);
            speedIntensity *= _airspeedIntensity;

            float angularIntensity = Mathf.Clamp01(_target.FlightStatistics.LocalAngularVelocity.magnitude / _maxAngularSpeed);
            angularIntensity = Mathf.Pow(angularIntensity, 2f);
            angularIntensity *= _angularSpeedIntensity;

            float groundIntensity = Mathf.Pow(1f - Mathf.Clamp01(_target.FlightStatistics.AltitudeGround / _groundShakeAltitude), 1.5f);
            groundIntensity = Mathf.Pow(groundIntensity, 2f);
            groundIntensity *= _groundShakeIntensity;
            groundIntensity *= speedIntensity; // Only do the ground shake when we're actually flying fast

            intensity += speedIntensity + angularIntensity + groundIntensity;
        }

	    intensity *= _amplitude;

        float time = Time.time * _baseFrequency; // Todo: also modulate with intensity

	    if (!VRSettings.enabled) {
            _transform.localRotation = Quaternion.Euler(
            Mathf.PerlinNoise(time * 0.25f, time * 0.50f) * intensity,
            Mathf.PerlinNoise(time * 0.10f, time * 0.25f) * intensity,
            Mathf.PerlinNoise(time * 0.50f, time * 0.10f) * intensity);
        }
	}
}
