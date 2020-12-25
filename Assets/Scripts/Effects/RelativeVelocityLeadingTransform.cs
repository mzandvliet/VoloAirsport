using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using UnityEngine;

public class RelativeVelocityLeadingTransform : MonoBehaviour {
    [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;
    [SerializeField] private AbstractUnityEventSystem _eventSystem;
    [SerializeField, Dependency] private WindManager _wind;
    [SerializeField, Dependency] private FlightStatistics _target;
    [SerializeField] private float _leadTime = 1f;

    private IDisposable _playerSpawns;

    private Transform _transform;

    void Awake() {
        _transform = GetComponent<Transform>();
    }

    private void OnPlayerSpawned(Events.PlayerSpawned evt) {
        _target = evt.Player.FlightStatistics;
        enabled = true;
    }

    private void LateUpdate() {
        if (_target != null) {
            var state = _target.GetInterpolatedTrajectory(_leadTime);
            _transform.position = state.Position;
        }
    }

    [Dependency]
    public AbstractUnityEventSystem EventSystem {
        get { return _eventSystem; }
        set {
            if (_playerSpawns != null) {
                _playerSpawns.Dispose();
            }

            _eventSystem = value;
            _playerSpawns = _eventSystem.Listen<Events.PlayerSpawned>(OnPlayerSpawned);
        }
    }
}