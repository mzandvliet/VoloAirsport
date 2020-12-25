using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using UnityEngine;

public class PlayerSettingsApplier : MonoBehaviour {

    [Dependency, SerializeField] private AbstractUnityEventSystem _eventSystem;

    [SerializeField] private TrajectoryVisualizer _trajectoryVisualizer;
    [SerializeField] private AerodynamicsVisualizationManager _aerodynamicsVisualizationManager;

    void Awake() {
        //_eventSystem.Listen<Events.SettingsUpdated>(this, OnSettingsUpdated);
    }

    void Start() {
        // TODO Apply currently active settings
    }

    private void OnSettingsUpdated(Events.SettingsUpdated @event) {
        var settings = @event.Settings;
        _trajectoryVisualizer.enabled = settings.Gameplay.VisualizeTrajectory;
        _aerodynamicsVisualizationManager.enabled = settings.Gameplay.VisualizeAerodynamics;
    }
}