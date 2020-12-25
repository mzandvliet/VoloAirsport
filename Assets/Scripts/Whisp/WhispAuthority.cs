using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityExecutionOrder;
using Random = UnityEngine.Random;

/* Todo: 
 * Direction as quaternion is more efficient for network compression
 * Whisp should never move into illegal positions, as they should be expected to run for quite a while on the server
 * Change whisp to become a dynamically spawned object. On joining a server, despawn local whisps and spawn new ones in slave role.
 */
[Run.Before(typeof(WhispClient))]
public class WhispAuthority : MonoBehaviour, INetworkBehavior {
    [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;
    [SerializeField] private WhispConfiguration _configuration;
    [SerializeField] private float _navigationInterval = 0.1f;
    [SerializeField] private float _randomness = 0.05f;
    [SerializeField] private float _raycastLength = 200f;
    [SerializeField] private float _maxAltitude = 100f;
    [SerializeField] private float _altitudeLimitPower = 6f;

    [Dependency] private INetworkMessagePool<WhispMessage.UpdateDirection> _directionMessages;
    [Dependency] private IObjectMessageSender _messageSender;

    private Transform _transform;
    private Vector3 _direction;

    private double _lastUpdateTime;

    void Awake() {
        _transform = gameObject.GetComponent<Transform>();
    }

    // Checks which direction to go in every once in a while

    void Update() {
        /* TODO We can tweak the update rate at which we send whisp updates
                updates based on the player's position
                sending really few updates for players that are far away
                and relatively many updates to the players that are close by */
        if(_clock.CurrentTime > _lastUpdateTime + _navigationInterval) {
            _direction = Vector3.Slerp(_direction, Random.insideUnitSphere, _randomness).normalized;

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, _raycastLength);
            if (hit) {
                float normalizedAltitude = Mathf.Clamp01((transform.position.y - hitInfo.point.y) / _maxAltitude);

                _direction = Vector3.Lerp(_direction, Vector3.down, Mathf.Pow(normalizedAltitude, _altitudeLimitPower));
                _direction = Vector3.Lerp(_direction, Vector3.up, Mathf.Pow(1f - normalizedAltitude, _altitudeLimitPower));
            }

            _lastUpdateTime = _clock.CurrentTime;

            var updateDirectionMessage = _directionMessages.Create();
            updateDirectionMessage.Content.Position = transform.position;
            updateDirectionMessage.Content.Direction = _direction;
            _messageSender.Send(updateDirectionMessage, ObjectRoles.NonAuthoritive);
        }

        _transform.position = WhispSimulation.Simulate(_transform.position, _direction, _configuration, _clock.DeltaTime);
    }

    public ObjectRole Role {
        get { return ObjectRole.Authority; }
    }

    public void OnRoleEnabled(ObjectRole role) {}
    public void OnRoleDisabled(ObjectRole role) {}
}
