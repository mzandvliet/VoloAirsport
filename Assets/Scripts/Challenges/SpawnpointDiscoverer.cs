using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using Newtonsoft.Json;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using UnityExecutionOrder;

namespace RamjetAnvil.Volo {

    [Run.After(typeof(Spawnpoint))]
    public class SpawnpointDiscoverer : MonoBehaviour {

        public static readonly Lazy<string> DiscoveredSpawnpointsPath = new Lazy<string>(() => {
            return Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "DiscoveredSpawnpoints.json"); 
        });

        [SerializeField, Dependency] private List<Spawnpoint> _spawnpoints;
        [SerializeField, Dependency] private ChallengeAnnouncerUi _announcerUi;
        [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;
        [SerializeField] private List<MonoBehaviour> _spawnpointDependencies;
        [SerializeField, Dependency("fileWriterScheduler")] private IScheduler _fileWriterScheduler;
        [SerializeField] private Color _challengeColor = Color.white;

        private IImmutableList<string> _discoveredSpawnpoints;
        private JsonSerializer _json;

        void Awake() {
            _json = new JsonSerializer();
            if (!File.Exists(DiscoveredSpawnpointsPath.Value)) {
                _discoveredSpawnpoints = ImmutableList.Create<string>();
                _json.SerializeToFile(DiscoveredSpawnpointsPath.Value, _discoveredSpawnpoints);
            } else {
                _discoveredSpawnpoints = _json.DeserializeFromFile<IImmutableList<string>>(DiscoveredSpawnpointsPath.Value);    
            }

            var spawnpointDependencyContainer = new DependencyContainer();
            foreach (var spawnpointDependency in _spawnpointDependencies) {
                spawnpointDependencyContainer.AddDependency(spawnpointDependency.name, spawnpointDependency);    
            }
            for (int i = 0; i < _spawnpoints.Count; i++) {
                var spawnpoint = _spawnpoints[i];
                DependencyInjector.Default.Inject(spawnpoint.gameObject, spawnpointDependencyContainer);    
                spawnpoint.IsDiscovered = _discoveredSpawnpoints.Contains(spawnpoint.Id) || spawnpoint.IsDiscovered;
                spawnpoint.OnDiscover += () => OnSpawnpointDiscovered(spawnpoint);
            }
        }

        private void OnSpawnpointDiscovered(Spawnpoint s) {
            s.IsDiscovered = true;

            _discoveredSpawnpoints = _discoveredSpawnpoints.Add(s.Id);
            _fileWriterScheduler.Schedule(_discoveredSpawnpoints, SerializeDiscoveredSpawnpoints);

            var spawnpointsDiscovered = 0;
            for (int i = 0; i < _spawnpoints.Count; i++) {
                var spawnpoint = _spawnpoints[i];
                if (spawnpoint.IsDiscovered) {
                    spawnpointsDiscovered++;
                }
            }

            _coroutineScheduler.Run(_announcerUi.Introduce(
                "Spawnpoint discovered:", 
                _challengeColor, 
                s.Name,
                spawnpointsDiscovered + "/" + _spawnpoints.Count + " spawnpoints discovered"));
        }

        private IDisposable SerializeDiscoveredSpawnpoints(IScheduler scheduler, IImmutableList<string> discoveredSpawnpoints) {
            _json.SerializeToFile(DiscoveredSpawnpointsPath.Value, _discoveredSpawnpoints);
            return Disposables.Empty;
        }

        public List<Spawnpoint> Spawnpoints {
            get { return _spawnpoints; }
        }
    }
}
