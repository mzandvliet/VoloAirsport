using Lidgren.Network;
using RamjetAnvil.RamNet;
using UnityEngine;
using RamjetAnvil.DependencyInjection;

public class WindTurbine : MonoBehaviour, INetworkBehavior {
    [SerializeField] private Transform _rotorTransform;
    [SerializeField, Dependency("gameClock")] private AbstractUnityClock _clock;
    [SerializeField, Dependency] private WindManager _wind;
    [SerializeField] private float _networkSyncSpeed = 3f;
    [SerializeField] private int _sendRate = 20;

    [Dependency] private INetworkMessagePool<UpdateRotorMessage> _updateMessages;
    [Dependency] private IObjectMessageSender _messageSender;

    private Quaternion _serverRotation;
    private double _serverTimestamp;

    private Try _trySendUpdate;
    private ObjectRole _role;

    void Awake() {
        _rotorTransform.Rotate(new Vector3(0f, -Random.value * 60f, 0f), Space.Self);
        _trySendUpdate = MessagingUtil.RateLimiter(_sendRate, () => {
            if ((_role & ObjectRole.Authority) == ObjectRole.Authority) {
                var updateMessage = _updateMessages.Create();
                updateMessage.Content.Rotation = _rotorTransform.localRotation;
                _messageSender.Send(updateMessage, ObjectRoles.NonAuthoritive);
            }
        });
    }

    void Update() {
        if (_clock && _wind) {
            //float effectiveWind = Vector3.Cross(_aero.GetWindVelocity(_rotorTransform.position), _rotorTransform.up).magnitude;
            _rotorTransform.Rotate(Simulate(_clock.DeltaTime), Space.Self);
            _trySendUpdate(_clock.CurrentTime);

            var timeDiff = (float)(_clock.CurrentTime - _serverTimestamp);
            var extrapolatedServerRotation = _serverRotation.eulerAngles + Simulate(timeDiff);

            var rotation = Quaternion.Lerp(
                _rotorTransform.localRotation, 
                Quaternion.Euler(extrapolatedServerRotation), 
                _networkSyncSpeed * _clock.DeltaTime);
            _rotorTransform.localRotation = rotation;
        }
    }

    [MessageHandler(allowedSenders: ObjectRole.Authority)]
    void HandleUpdateRotor(UpdateRotorMessage message, ObjectMessageMetadata metadata) {
        _serverRotation = message.Rotation;
        _serverTimestamp = _clock.CurrentTime - metadata.Latency;
    }

    private Vector3 Simulate(float timePassed) {
        return new Vector3(0f, -50f, 0f) * timePassed;
    }

    public ObjectRole Role { get { return ObjectRoles.Everyone; } }

    public void OnRoleEnabled(ObjectRole role) {
        _role = role;
    }

    public void OnRoleDisabled(ObjectRole role) {
        _role = ObjectRole.Nobody;
    }

    public class UpdateRotorMessage : IOrderedObjectMessage {
        public Quaternion Rotation;

        public void Serialize(NetBuffer writer) {
            writer.WriteRotation(Rotation);    
        }

        public void Deserialize(NetBuffer reader) {
            Rotation = reader.ReadRotation();
        }

        public NetDeliveryMethod QosType { get {return NetDeliveryMethod.Unreliable;} }
    }
}
