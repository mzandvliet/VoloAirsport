using System.Collections.Generic;
using RamjetAnvil.RamNet;
using UnityEngine;

public class GameObjectNetworkInfo : MonoBehaviour {
    [SerializeField] private uint _objectType;
    [SerializeField] private uint _objectId;
    [SerializeField] private List<ObjectRole> _roles;
    [SerializeField] private float _lastReceivedMessageTimestamp;

    private ObjectRole _objectRole;

    void Awake() {
        _roles = _roles ?? new List<ObjectRole>(3);
    }

    public ObjectType ObjectType {
        get { return new ObjectType(_objectType); }
        set { _objectType = value.Value; }
    }

    public ObjectId ObjectId {
        get { return new ObjectId(_objectId); }
        set { _objectId = value.Value; }
    }

    public float LastReceivedMessageTimestamp {
        get { return _lastReceivedMessageTimestamp; }
        set { _lastReceivedMessageTimestamp = value; }
    }

    public ObjectRole Role {
        get { return _objectRole; }
        set {
            _objectRole = value;
            _roles = _roles ?? new List<ObjectRole>(3);
            value.IntoList(_roles);
        }
    }
}
