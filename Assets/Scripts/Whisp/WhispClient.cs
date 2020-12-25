using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Unity.Utils;
using RamjetAnvil.Volo;
using UnityEngine;

public class WhispClient : MonoBehaviour, INetworkBehavior {
    [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;
    [SerializeField] private WhispConfiguration _configuration;
    [SerializeField] private float _syncSpeed = 3f;

    private Transform _transform;
    
    private double _serverTimestamp;
    private Vector3 _serverPosition;
    private Vector3 _direction;

    private void Awake() {
        _transform = gameObject.GetComponent<Transform>();
    }

    void Update() {
        // Calculate where the whisp should be based on the received direction
        // Calculate where it is now
        //Smoothly transition between the two

        Vector3 newPos = WhispSimulation.Simulate(_transform.position, _direction, _configuration, _clock.DeltaTime);
        Vector3 correctedPos = WhispSimulation.Simulate(_serverPosition, _direction, _configuration, (float)(_clock.CurrentTime - _serverTimestamp));
        _transform.position = Vector3.Lerp(newPos, correctedPos, _syncSpeed * _clock.DeltaTime);
    }

    [MessageHandler(ObjectRole.Authority)]
    void HandleDirectionUpdate(WhispMessage.UpdateDirection message, ObjectMessageMetadata metadata) {
        _serverTimestamp = _clock.CurrentTime - metadata.Latency;
        _serverPosition = message.Position;
        _direction = message.Direction;
    }

    public ObjectRole Role {
        get { return ObjectRoles.NonAuthoritive; }
    }

    public void OnRoleEnabled(ObjectRole role) {}
    public void OnRoleDisabled(ObjectRole role) {}
}
