using System;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Guid = System.Guid;

namespace RamjetAnvil.Volo {
    public class Spawnpoint : MonoBehaviour {
        [SerializeField] private GameObject _discoverPrefab;

        [SerializeField] private GameObjectNetworkId _objectId;
        [SerializeField] private string _name = "";
        [SerializeField] private bool _isDiscovered;

        private GameObject _discoverableObject;

        public event Action OnDiscover;

        void Awake() {
            if (!_isDiscovered) {
                _discoverableObject = GameObject.Instantiate(_discoverPrefab);
                _discoverableObject.SetParent(this.gameObject);
                _discoverableObject.transform.position = transform.position;
                _discoverableObject.transform.rotation = transform.rotation;
                var collisionEventSource = _discoverableObject.GetComponent<CollisionEventSource>();
                collisionEventSource.OnTriggerEntered += (source, c) => OnTriggerEnter(c);
            }
        }

        void OnTriggerEnter(Collider collider) {
            if (collider.CompareTag("Player")) {
                Debug.Log("discovered spawnpoint " + _name);
                if (OnDiscover != null) {
                    OnDiscover();
                }
            }
        }

        public bool IsDiscovered {
            get { return _isDiscovered; }
            set {
                _isDiscovered = value;
                if (_isDiscovered) {
                    GameObject.Destroy(_discoverableObject);
                }
            }
        }

        public string Id {
            get { return _objectId.Id.ToString(); }
        }

        public string Name {
            get { return _name; }
        }

        public SpawnpointLocation Location {
            get { return SpawnpointLocation.ToLocation(transform); }
        }

    }
}
