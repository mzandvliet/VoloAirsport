using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo
{
    public class InstancingTest : MonoBehaviour {
        [SerializeField] private GameObject _instancedPrefab;
        [SerializeField] private GameObject _normalPrefab;
        [SerializeField] private int _totalObjects = 1000;
        [SerializeField] private int _movableObjects = 100;

        private IList<GameObject> _activeObjects;

        void Awake() {
            _activeObjects = new List<GameObject>(_totalObjects);
        }

        void Update() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.H)) {
                CreateInstances(_normalPrefab);
            } else if (UnityEngine.Input.GetKeyDown(KeyCode.J)) {
                CreateInstances(_instancedPrefab);
            }

            for (int i = 0; i < Mathf.Min(_activeObjects.Count, _movableObjects); i++) {
                var o = _activeObjects[i];
                o.transform.position += new Vector3(1 * Time.deltaTime, 0f, 0f);
            }
        }

        void CreateInstances(GameObject prefab) {
            DestroyAllObjects();
            for (int i = 0; i < _totalObjects; i++) {
                var instance = GameObject.Instantiate(prefab);
                instance.transform.position = RandomVector();
                _activeObjects.Add(instance);
            }
        }

        void DestroyAllObjects() {
            foreach (var activeObject in _activeObjects) {
                GameObject.Destroy(activeObject);
            }
            _activeObjects.Clear();
        }

        Vector3 RandomVector() {
            return new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
        }

    }
}
