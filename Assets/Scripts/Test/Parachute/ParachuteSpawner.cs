using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace RamjetAnvil.Volo {

    /* Todo:
     * 
     * Chord curvature is reversed
     */
    
    public class ParachuteSpawner : MonoBehaviour {
        [SerializeField] private AirfoilDefinition _airfoil;
        [SerializeField] private ParachuteMeshConfig _parachuteMeshconfig;
        [SerializeField] private Material _parachuteMaterial;
        [SerializeField, Dependency] private GameSettingsProvider _gameSettingsProvider;
	    [SerializeField, Dependency] private WindManager _windManager;
        [SerializeField, Dependency("fixedClock")] private AbstractUnityClock _fixedClock;
        [SerializeField, Dependency("fixedScheduler")] private FixedUnityCoroutineScheduler _fixedScheduler;

        private AirfoilDefinition _airfoilDefinition;

	    private void Awake() {
	        _airfoilDefinition = Instantiate(_airfoil);
            UnityParachuteMeshFactory.Initialize(_parachuteMeshconfig);
        }

        public Parachute Create(ParachuteConfig config, string name = "Parachute") {
            return Create(config, ImmutableTransform.Identity, name);
        }

        public Parachute Create(ParachuteConfig config, ImmutableTransform spawnpoint, string name = "Parachute") {
            Profiler.BeginSample("CreateParachute");

            config.AirfoilDefinition = _airfoilDefinition;

            Profiler.BeginSample("CreateSimObject");
            var parachute = UnityParachuteFactory.Create(config, spawnpoint, name);
            Profiler.EndSample();

            Profiler.BeginSample("CreateSkinnedMesh");
            UnityParachuteMeshFactory.CreateSkinnedMesh(parachute, _parachuteMeshconfig, _parachuteMaterial);
            Profiler.EndSample();

            parachute.Inject(_windManager, _fixedClock, _fixedScheduler);

            Profiler.EndSample();

            return parachute;
        }
    }

}
