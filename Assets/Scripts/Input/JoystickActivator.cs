using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using InControl;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;

public class JoystickActivator : MonoBehaviour {

    private ISubject<bool> _suspension;
    private IObservable<Controller?> _controllerActivity;
    private IDisposable _subscription;

    void Awake() {
        Initialize();
    }

    void OnDestroy() {
        _subscription.Dispose();
    }

    void OnEnable() {
        _suspension.OnNext(false);
    }

    void OnDisable() {
        _suspension.OnNext(true);
    }

    public IObservable<Controller?> ActiveController {
        get {
            Initialize();
            return _controllerActivity;
        }
    }

    private void Initialize() {
        if (_controllerActivity == null) {
            _suspension = new BehaviorSubject<bool>(!enabled);

//            IObservable<ButtonEvent> keyboardActivity = gameObject.AddComponent<KeyboardPoller>().ButtonEvents;
//            IObservable<Controller?> keyboardSelection = keyboardActivity
//                .Where(e => e == ButtonEvent.Down)
//                .Select(e => (Controller?)null);

            IObservable<Controller?> controllerSelection = ControllerDetection.ActiveJoystick()
                .Select<ControllerDetection.NamedControllerId, Controller?>(
                    namedControllerId => {
                        var id = namedControllerId.ControllerId;
                        var name = namedControllerId.Name;
                        if (id is ControllerId.XInput) {
                            return new Controller(ControllerType.Xbox360, id, Maybe.Nothing<UnityInputDeviceProfile>(), name);
                        }
                        var inputDeviceProfile = Controllers.UnityIdToDeviceProfile(namedControllerId.Name);
                        var controllerType = Controllers.UnityIdToControllerType(namedControllerId.Name);
                        return new Controller(controllerType, namedControllerId.ControllerId, inputDeviceProfile, name);
                    });

            Controller? keyboardPeripheral = null;
            var deviceSelection = controllerSelection
                .DistinctUntilChanged(EqualityComparer<Controller?>.Default)
                .Suspenable(_suspension)
                 // Initially there is no controller active
                 // so we report that
                .StartWith(keyboardPeripheral)
                .Replay(1);

            _subscription = deviceSelection.Connect();
            _controllerActivity = deviceSelection
                .DistinctUntilChanged(EqualityComparer<Controller?>.Default);

            _controllerActivity.Subscribe(controller => {
                Debug.Log("Controller " + controller + " activated");
            });
        }
    }

    public struct Controller : IEquatable<Controller> {
        public readonly Maybe<UnityInputDeviceProfile> DeviceProfile;
        public readonly ControllerType ControllerType;
        public readonly ControllerId Id;
        public readonly string Name;

        public Controller(ControllerType controllerType, ControllerId id, Maybe<UnityInputDeviceProfile> deviceProfile, string name) {
            ControllerType = controllerType;
            Id = id;
            DeviceProfile = deviceProfile;
            Name = name;
        }

        public bool Equals(Controller other) {
            return Equals(Id, other.Id) && ControllerType == other.ControllerType;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Controller && Equals((Controller) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (int) ControllerType;
            }
        }

        public static bool operator ==(Controller left, Controller right) {
            return left.Equals(right);
        }

        public static bool operator !=(Controller left, Controller right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return string.Format("{0}, ControllerType: {1}", Id, ControllerType);
        }
    }
}
