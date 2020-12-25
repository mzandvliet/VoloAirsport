using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Util;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RxUnity.Schedulers;
using UnityEngine;
using UnityEngine.Profiling;
using XInputDotNetPure;
using ButtonState = RamjetAnvil.Impero.StandardInput.ButtonState;

namespace RamjetAnvil.Volo
{
    public static class ControllerDetection {

        public static InputMap<ControllerInputSource, float> AllControllerAxes() {
            return UnityInputIds.ControllerIds
                .Select(controllerId => {
                    return Peripherals.Controller.PolarizedAxes(controllerId)
                        .ChangeId(inputSource => new ControllerInputSource(new ControllerId.Unity(controllerId), inputSource))
                        .Source;
                })
                .Merge()
                .ToInputMap();
        }

        /// <summary>
        /// All button input of all unity joysticks combined.
        /// </summary>
        /// <returns></returns>
        public static InputMap<ControllerInputSource, ButtonEvent> AllControllerButtons() {
            var unityControllers = UnityInputIds.ControllerIds
                .Select(controllerId => {
                    return Peripherals.Controller.Buttons(controllerId)
                        .ChangeId(inputSource => new ControllerInputSource(new ControllerId.Unity(controllerId), inputSource))
                        .Source;
                })
                .Merge()
                .ToInputMap();
            var xInputGamepads = EnumUtils.GetValues<PlayerIndex>()
                .Select(playerIndex => {
                    return Peripherals.Controller.XInputButtons(playerIndex)
                        .ChangeId(inputSource => new ControllerInputSource(new ControllerId.XInput(playerIndex), inputSource))
                        .Source;
                })
                .Merge()
                .ToInputMap();

            return ImperoCore.MergeAll(Adapters.MergeButtons, new[] {unityControllers, xInputGamepads})
                .Adapt(() => Adapters.ButtonEvents(() => Time.frameCount));
        }

        public static IObservable<NamedControllerId> ActiveJoystick() {
            return UnityObservable.CreateUpdate<NamedControllerId>(observer => {
                var xinputJoystickId = XInput.AnyJoystick();
                var unityJoystickId = UnityInputMaps.AnyJoystick();
                if (xinputJoystickId.HasValue) {
                    var controllerId = new ControllerId.XInput(xinputJoystickId.Value);
                    observer.OnNext(new NamedControllerId(controllerId, "XInput controller"));
                } else if (unityJoystickId.HasValue) {
                    var controllerId = new ControllerId.Unity(unityJoystickId.Value);
                    var controllerName = UnityEngine.Input.GetJoystickNames()[controllerId.Id];
                    observer.OnNext(new NamedControllerId(controllerId, controllerName));
                }
            })            
            // XInput takes presedence over unity input because we know how to handle it better.
            // So if we detect an XInput device we ignore the Unity device(s) detected during the
            // same window.
            .Window(TimeSpan.FromSeconds(0.8f), Scheduler.TaskPool)
            .SelectMany(inputSources => {
                return inputSources.Scan((selectedInputSource, inputSource) => {
                    if (selectedInputSource.ControllerId is ControllerId.XInput) {
                        return selectedInputSource;
                    }
                    return inputSource;
                }).TakeLast(1);
            })
            .ObserveOn(UnityThreadScheduler.MainThread);;
        }

        public struct NamedControllerId : IEquatable<NamedControllerId> {
            private readonly ControllerId _controllerId;
            private readonly string _name;

            public NamedControllerId(ControllerId controllerId, string name) {
                _controllerId = controllerId;
                _name = name;
            }

            public ControllerId ControllerId {
                get { return _controllerId; }
            }

            public string Name {
                get { return _name; }
            }

            public bool Equals(NamedControllerId other) {
                return Equals(_controllerId, other._controllerId);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                return obj is NamedControllerId && Equals((NamedControllerId) obj);
            }

            public override int GetHashCode() {
                return (_controllerId != null ? _controllerId.GetHashCode() : 0);
            }

            public static bool operator ==(NamedControllerId left, NamedControllerId right) {
                return left.Equals(right);
            }

            public static bool operator !=(NamedControllerId left, NamedControllerId right) {
                return !left.Equals(right);
            }

            public override string ToString() {
                return string.Format("{0} (id: '{1}')", _name, _controllerId);
            }
        }

        public class ControllerInputSource : IEquatable<ControllerInputSource> {
            private readonly ControllerId _controllerId;
            private readonly InputSource _inputSource;

            public ControllerInputSource(ControllerId controllerId, InputSource inputSource) {
                _controllerId = controllerId;
                _inputSource = inputSource;
            }

            public ControllerId ControllerId {
                get { return _controllerId; }
            }

            public InputSource InputSource {
                get { return _inputSource; }
            }

            public bool Equals(ControllerInputSource other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_controllerId, other._controllerId) && _inputSource.Equals(other._inputSource);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ControllerInputSource) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((_controllerId != null ? _controllerId.GetHashCode() : 0) * 397) ^ _inputSource.GetHashCode();
                }
            }

            public static bool operator ==(ControllerInputSource left, ControllerInputSource right) {
                return Equals(left, right);
            }

            public static bool operator !=(ControllerInputSource left, ControllerInputSource right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return string.Format("{0}, InputSource: {1}", _controllerId, _inputSource);
            }
        }


    }
    
    public abstract class ControllerId {
        private ControllerId() {}

        public sealed class Unity : ControllerId, IEquatable<Unity> {
            public readonly int Id;

            public Unity(int id) {
                Id = id;
            }

            public bool Equals(Unity other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id == other.Id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is Unity && Equals((Unity) obj);
            }

            public override int GetHashCode() {
                return Id;
            }

            public static bool operator ==(Unity left, Unity right) {
                return Equals(left, right);
            }

            public static bool operator !=(Unity left, Unity right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return string.Format("UnityId: {0}", Id);
            }
        }

        public sealed class XInput : ControllerId, IEquatable<XInput> {
            public readonly PlayerIndex Id;

            public XInput(PlayerIndex id) {
                Id = id;
            }

            public bool Equals(XInput other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id == other.Id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is XInput && Equals((XInput) obj);
            }

            public override int GetHashCode() {
                return (int) Id;
            }

            public static bool operator ==(XInput left, XInput right) {
                return Equals(left, right);
            }

            public static bool operator !=(XInput left, XInput right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return string.Format("XInputId: {0}", Id);
            }
        }
    }
}
