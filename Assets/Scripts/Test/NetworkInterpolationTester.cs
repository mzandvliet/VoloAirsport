using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;

public class NetworkInterpolationTester : MonoBehaviour {
//    [SerializeField] private GameObject _remotePilot;
//    [SerializeField] private GameObject _localPilot;
//
//    [SerializeField]
//    private AbstractUnityClock _gameClock;
//    [SerializeField]
//    private AbstractUnityClock _fixedClock;
//    [SerializeField]
//    private PlayerInputRepository _inputRepository;
//    [SerializeField]
//    private AbstractUnityEventSystem _eventSystem;
//    [SerializeField]
//    private WindManager _windManager;
//    [SerializeField]
//    private CameraManager _cameraManager;
//    private ActionMap<PilotAction> _pilotActionMap;
//    private DependencyContainer _dependencies;
//
//    private IList<Rigidbody> _bodiesRemote;
//    private IList<Rigidbody> _bodiesLocal;
//
//    private CircularBuffer<RagdollState> _remoteStates;
//    private const float LocalDelay = 0.7f;
//    private const float SendInterval = 0.1f;
//
//    private void Awake() {
//        CreateContainer();
//        DependencyInjection.Inject(_remotePilot, _dependencies);
//        DependencyInjection.Inject(_localPilot, _dependencies);
//        
//        _bodiesRemote = _remotePilot.GetComponentsInChildren<Rigidbody>(true);
//        _bodiesLocal = _localPilot.GetComponentsInChildren<Rigidbody>(true);
//
//        int bufferSize =  Mathf.RoundToInt(1.0f / Time.fixedDeltaTime);
//        _remoteStates = new CircularBuffer<RagdollState>(bufferSize);
//    }
//
//    private void Start() {
//        PrefabPool.InvokeSpawnables(_remotePilot);
//        PrefabPool.InvokeSpawnables(_localPilot);
//    }
//
//    private void CreateContainer() {
//        CreateActionMap();
//
//        _dependencies = new DependencyContainer(_eventSystem, _windManager, _cameraManager);
//        _dependencies.AddDependency("actionMap", new Ref<ActionMap<PilotAction>>(_pilotActionMap));
//        _dependencies.AddDependency("gameClock", _gameClock);
//        _dependencies.AddDependency("fixedClock", _fixedClock);
//    }
//
//    private void CreateActionMap() {
//        var inputMapping = new InputSourceMapping<PilotAction>()
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Positive, 1), PilotAction.PitchUp)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Negative, 1), PilotAction.PitchDown)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Negative, 0), PilotAction.RollLeft)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Positive, 0), PilotAction.RollRight)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Positive, 2), PilotAction.YawLeft)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Negative, 2), PilotAction.YawRight)
//
//            // Close arms
//            .Map(InputSource.Button(Peripheral.Joystick, 5), PilotAction.CloseArms)
//            .Map(InputSource.Button(Peripheral.Joystick, 3), PilotAction.CloseArms)
//            .Map(InputSource.Button(Peripheral.Joystick, 2), PilotAction.CloseLeftArm)
//            .Map(InputSource.Button(Peripheral.Joystick, 1), PilotAction.CloseRightArm)
//            .Map(InputSource.Button(Peripheral.Joystick, 4), PilotAction.Cannonball)
//
//            // Look
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Negative, 4), PilotAction.LookUp)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Positive, 4), PilotAction.LookDown)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Negative, 3), PilotAction.LookLeft)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Positive, 3), PilotAction.LookRight)
//            .Map(InputSource.Button(Peripheral.Joystick, 9), PilotAction.ChangeViewpoint)
//            .Map(InputSource.Button(Peripheral.Joystick, 6), PilotAction.Respawn)
//            .Map(InputSource.PolarizedAxis(Peripheral.Joystick, AxisPolarity.Negative, 6), PilotAction.ToSpawnpointMenu);
//
//        _pilotActionMap = VoloInputMaps.CreatePilotActionMap(0, inputMapping,
//            new Ref<GameSettings>(GameSettings.ReadSettingsFromDisk()));
//    }
//
//    private float _lastTime;
//    private void FixedUpdate() {
//        if (Time.time > _lastTime + SendInterval) {
//            var remoteState = PlayerOwner.GetCurrentState(_bodiesRemote, Time.time);
//            _remoteStates.Add(remoteState);
//            _lastTime = Time.time;
//        }
//        
//        if (_remoteStates.Count < 2) {
//            return;
//        }
//
//        var interpolatedRemoteState = PlayerOwner.GetInterpolatedState(_remoteStates, Time.time - LocalDelay);
//        PlayerOwner.ApplyState(interpolatedRemoteState, _bodiesLocal);
//    }
}
