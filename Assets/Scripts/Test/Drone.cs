using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using UnityEngine;

public class Drone : MonoBehaviour {
    [SerializeField, Dependency] private AbstractUnityEventSystem _eventSystem;
    [SerializeField, Dependency] private AbstractUnityClock _clock;

    [SerializeField] private float _speed = 100f;
    [SerializeField] private float _navigationInterval = 0.1f;
    [SerializeField] private float _randomness = 0.05f;
    [SerializeField] private float _raycastLength = 200f;
    [SerializeField] private float _maxAltitude = 100f;
    [SerializeField] private float _altitudeLimitPower = 3f;

    private TrailRenderer _trail;
    private Vector3 _startPosition;
    private Vector3 _direction;

    void Awake() {
        _trail = gameObject.GetComponentInChildren<TrailRenderer>();
    }

    void Start() {
         _startPosition = transform.position;
    }

    void OnEnable() {
        _eventSystem.Listen<Events.PlayerSpawned>(OnPlayerRespawned);
    }

    private void OnPlayerRespawned() {
        transform.position = _startPosition;
        _trail.Clear();
    }

    private double _lastUpdateTime;

    // Authority decides in which direction to go
    // Client follows this direction, recalculates his position and smoothly transitions to it

    private void Update() {
        if (_clock && _clock.CurrentTime > _lastUpdateTime + _navigationInterval) {
            _direction = Vector3.Slerp(_direction, Random.insideUnitSphere, _randomness).normalized;

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, _raycastLength);
            if (hit) {
                float normalizedAltitude = Mathf.Clamp01((transform.position.y - hitInfo.point.y) / _maxAltitude);

                _direction = Vector3.Lerp(_direction, Vector3.down, Mathf.Pow(normalizedAltitude, _altitudeLimitPower));
                _direction = Vector3.Lerp(_direction, Vector3.up, Mathf.Pow(1f - normalizedAltitude, _altitudeLimitPower));
            }

            _lastUpdateTime = _clock.CurrentTime;
        }

        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }
}
