using System;
using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.GameObjectFactories;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;

/// <summary>
/// Spawns pilots without a player controller
/// The player controller can be attached at a later point in time when the pilot
/// needs to be controlled by a human. Or another kind of controller can be attached
/// if the pilot for example needs to be controlled by an AI.
/// </summary>
public class PilotSpawner : MonoBehaviour {
        // deps
    [Dependency("gameClock"), SerializeField] private AbstractUnityClock _gameClock;
    [Dependency("fixedClock"), SerializeField] private AbstractUnityClock _fixedClock;
    [Dependency, SerializeField] private CameraManager _cameraManager;
    [SerializeField] private GameObject pilotPrefab;
    [Dependency, SerializeField] private AbstractUnityEventSystem _eventSystem;
    [Dependency, SerializeField] private WindManager _windManager;

    private bool _isInitialized;
    private Func<GameObject> _pilotFactory;

    public Func<GameObject> PilotFactory {
        get {
            if (!_isInitialized) {
                var dependencyContainer = new DependencyContainer(new Dictionary<string, object> {
                    {"gameClock", _gameClock},
                    {"fixedClock", _fixedClock},
                    {"eventSystem", _eventSystem},
                    {"cameraManager", _cameraManager},
                    {"windManager", _windManager}
                });

                _pilotFactory = GameObjectFactory.FromPrefab(pilotPrefab, turnOff: true)
                    .Adapt(go => {
                        DependencyInjector.Default.Inject(go, dependencyContainer);
                        go.GetComponentInHierarchy<PlayerController>().enabled = false;
                    })
                    .TurnOn();

                _isInitialized = true;
            }
            return _pilotFactory;
        }
    }
}
