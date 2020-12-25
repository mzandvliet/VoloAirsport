using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Util {
    public class ParticleSystemPool : MonoBehaviour, IObjectPool<ParticleSystem> {
        [SerializeField] private int _initialMaxParticleSystems = 3;
        [SerializeField] private GameObject _particleSystemPrefab;

        private bool _isInitialized;
        private IObjectPool<ParticleSystem> _pool;

        void Awake() {
            if (!_isInitialized) {
                _pool = new ObjectPool<ParticleSystem>(
                    factory: () => {
                        var ps = GameObject.Instantiate(_particleSystemPrefab).GetComponent<ParticleSystem>();
                        ps.transform.SetParent(this.gameObject.transform);
                        return ps;
                    },
                    onReturnedToPool: s => s.Clear(),
                    growthStep: _initialMaxParticleSystems);
                _isInitialized = true;
            }
        }

        public IPooledObject<ParticleSystem> Take() {
            Awake();
            return _pool.Take();
        }
    }
}
