using System;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Networking;
using UnityEngine;
using Event = RamjetAnvil.Coroutine.Event;

/*
 * Todo:
 * 
 * Respawning consists of many different facets, and it's currently a bit messy.
 * 
 * - Moving to a new position
 * - Restoring original pose
 * - Resetting rigidbodies
 * - Resetting any other components with invalidated state 
 */
public class PlayerPilotSpawner : MonoBehaviour {
    // deps
    [Dependency("gameClock"), SerializeField] private AbstractUnityClock _gameClock;
    [Dependency, SerializeField] private AbstractUnityEventSystem _eventSystem;
    [Dependency, SerializeField] private CameraManager _cameraManager;
    [Dependency, SerializeField] private PilotActionMapProvider _pilotActionMapProvider;
    [Dependency, SerializeField] private GameSettingsProvider _gameSettingsProvider;
    [Dependency, SerializeField] public ActiveNetwork _activeNetwork;

    private IReadonlyRef<PilotActionMap> _playerActionMap;

    // state
    private bool _isRespawning;
    //private IPrefabPool _playerPool;

    private ReplicatedObject _currentPlayer;
    private Wingsuit _wingsuit;
    private Event _onReplicatedObjectAdded;
    private Event _onReplicatedObjectRemoved;

    public Wingsuit ActivePilot {
        get {
            if (_wingsuit == null) {
                return null;
            }
            return _wingsuit;
        }
    }

    public void Awake() {
        _currentPlayer = null;

        _playerActionMap = _pilotActionMapProvider.ActionMapRef;

        if (_gameSettingsProvider != null) {
            _gameSettingsProvider.SettingChanges.Subscribe(ApplySettings);
        }
    }

    public IEnumerator<WaitCommand> Despawn() {
        var networkSystems = _activeNetwork.NetworkSystems;
        if (_onReplicatedObjectRemoved == null) {
            _onReplicatedObjectRemoved = new Event(networkSystems.ObjectStore, "ObjectRemoved");    
        }
        
        var despawnRequest = networkSystems.MessagePool.GetMessage<GameMessages.DespawnPlayerRequest>();
        _activeNetwork.SendToAuthority(despawnRequest);
        _currentPlayer = null;
        _wingsuit = null;
        //yield return asyncDespawn.WaitUntilReady;
        // TODO This is a hack, we actually have to wait until the despawn is confirmed
        // in a multiplayer setting before we can continue
        yield return WaitCommand.DontWait;
    }

    private static readonly Func<ReplicatedObject, bool> IsOwnedPilot =
        obj => obj.OwnerConnectionId == ConnectionId.Self && obj.Type == ReplicationObjects.Pilot;

    public IEnumerator<WaitCommand> Respawn(SpawnpointLocation spawnpoint) {
        if (!_isRespawning) {
            _isRespawning = true;

            if (_currentPlayer != null && _currentPlayer.GameObject.activeInHierarchy) {
                _currentPlayer = null;
                _wingsuit = null;
            }

            var networkSystems = _activeNetwork.NetworkSystems;
            
            if (_onReplicatedObjectAdded == null) {
                _onReplicatedObjectAdded = new Event(networkSystems.ObjectStore, "ObjectAdded");    
            }
            // Wait until a player has been created by the replicated object store
            var asyncPlayer = AsyncResult<ReplicatedObject>.SingleResultFromEvent(
                _onReplicatedObjectAdded,
                IsOwnedPilot);

            var respawnRequest = networkSystems.MessagePool.GetMessage<GameMessages.RespawnPlayerRequest>();
            respawnRequest.Content.Spawnpoint = spawnpoint.AsWingsuitLocation();
            respawnRequest.Content.InputPitch = _playerActionMap.V.PollAxis(WingsuitAction.Pitch) +
                                                _playerActionMap.V.PollMouseAxis(WingsuitAction.Pitch);
            respawnRequest.Content.InputRoll = _playerActionMap.V.PollAxis(WingsuitAction.Roll) +
                                               _playerActionMap.V.PollMouseAxis(WingsuitAction.Roll);
            _activeNetwork.SendToAuthority(respawnRequest);

            yield return asyncPlayer.WaitUntilReady;

            _currentPlayer = asyncPlayer.Result;

            var playerDeps = new DependencyContainer();
            playerDeps.AddDependency("gameClock", _gameClock);
            DependencyInjector.Default.Inject(_currentPlayer.GameObject, playerDeps, overrideExisting: true);

            _wingsuit = _currentPlayer.GameObject.GetComponent<Wingsuit>();
            _wingsuit.AlphaManager.SetAlpha(1f);

//            if (_activeNetwork.AuthorityConnectionId != ConnectionId.Self)
//            {
//                SwitchToActiveMount();
//            }
//            else
//            {
//                Debug.Log("Ain't gonna switch no mount for yourself yo, you be dedicated!");
//            }


            if (_gameSettingsProvider != null) {
                ApplySettings(_gameSettingsProvider.ActiveSettings);
            }

            var pilot = _currentPlayer.GameObject.GetComponent<Wingsuit>();
            _eventSystem.Emit(new Events.PlayerSpawned(pilot));

            _isRespawning = false;
        }
    }

//    public static void JumpForward(GameMessages.RespawnPlayerRequest request, ReplicatedObject player) {
//        var spawnpoint = request.Spawnpoint;
//        var inputPitch = Mathf.Clamp(request.InputPitch, -1f, 1f);
//        var inputRoll = Mathf.Clamp(request.InputRoll, -1f, 1f);
//
//        // Jump forward a bit, and add angular momentum based on pitch/roll input
//        const float jumpForwardSpeed = 4f;
//        const float jumpRotationSpeed = 7f;
//
//        Vector3 jumpVelocity = spawnpoint.Up * jumpForwardSpeed;
//        float angularSpeedPitch = inputPitch * jumpRotationSpeed;
//        angularSpeedPitch = Mathf.Min(jumpRotationSpeed, angularSpeedPitch);
//        Vector3 pitchAxis = spawnpoint.TransformDirection(Vector3.right);
//        float angularSpeedRoll = -inputRoll * jumpRotationSpeed;
//        angularSpeedRoll = Mathf.Min(jumpRotationSpeed, angularSpeedRoll);
//        Vector3 rollAxis = spawnpoint.TransformDirection(Vector3.up);
//
//        var rigidbodies = player.GameObject.GetComponent<Wingsuit>().Rigidbodies;
//        for (int i = 0; i < rigidbodies.Count; i++) {
//            var childBody = rigidbodies[i];
//            childBody.velocity = jumpVelocity;
//            childBody.AddAccelerationAroundPosition(pitchAxis, angularSpeedPitch, spawnpoint.Position, ForceMode.VelocityChange);
//            childBody.AddAccelerationAroundPosition(rollAxis, angularSpeedRoll, spawnpoint.Position, ForceMode.VelocityChange);
//        }   
//    }

    private void ApplySettings(GameSettings settings) {
        if (_wingsuit != null) {
            _wingsuit.TrajectoryVisualizer.enabled = settings.Gameplay.VisualizeTrajectory;
            _wingsuit.AerodynamicsVisualizationManager.enabled = settings.Gameplay.VisualizeAerodynamics;
            _wingsuit.PilotAnimator.UserConfig = new PilotInputConfig(
                inputSpeedScaling: settings.Input.InputSpeedScaling,
                stallLimiterStrength: settings.Gameplay.StallLimiterStrength,
                rollLimiterStrength: settings.Gameplay.RollLimiterStrength,
                pitchAttitude: settings.Gameplay.PitchAttitude);
        }
    }

    public ActiveNetwork ActiveNetwork {
        set { _activeNetwork = value; }
    }
}