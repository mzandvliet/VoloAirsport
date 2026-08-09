using System;
using System.Collections.Generic;
using InControl;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    // Todo: everything in this file is an inert placeholder so the options/rebinding UI
    // compiles again after InControl+Impero were removed. None of it does anything real yet -
    // no controller detection, no live rebinding, no persistence. This is deliberately deferred
    // work (see Documentation/unity6-port-roadmap.md), matching how FMOD calls were stubbed
    // rather than implemented during the engine port.

    public enum ControllerId { XInput, DirectInput, Other }

    public enum ControllerType { Xbox360, XboxOne, SteamController, Playstation4, Other }

    public struct ConnectedController {
        public string Name;
        public ControllerId Id;
        public Maybe<UnityInputDeviceProfile> DeviceProfile;
    }

    public enum InputBindingGroup { Menu, Wingsuit, Spectator, Parachute }

    public struct InputBindingId : IEquatable<InputBindingId> {
        public readonly InputBindingGroup Group;
        public readonly object ActionId;

        public InputBindingId(InputBindingGroup group, object actionId) {
            Group = group;
            ActionId = actionId;
        }

        public bool Equals(InputBindingId other) {
            return Group == other.Group && Equals(ActionId, other.ActionId);
        }

        public override bool Equals(object obj) {
            return obj is InputBindingId && Equals((InputBindingId) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) Group * 397) ^ (ActionId != null ? ActionId.GetHashCode() : 0);
            }
        }

        public static bool operator ==(InputBindingId a, InputBindingId b) {
            return a.Equals(b);
        }

        public static bool operator !=(InputBindingId a, InputBindingId b) {
            return !a.Equals(b);
        }
    }

    public struct InputBindingViewModel {
        public InputBindingId Id;
        public InputBindingGroup Group;
        public string Name;
        public string BindingType;
        public string Binding;
    }

    public struct InputBindingSource {
        public string Description;
    }

    public class InputSourceMapping<TAction> {
        public void Serialize2Disk(string path) {
        }
    }

    public class ActionMapConfig<TAction> {
        public InputSourceMapping<TAction> InputMapping;
    }

    public struct InputSettings {
        public static InputSettings FromGameSettings(object gameInputSettings) {
            return new InputSettings();
        }
    }

    public class InputBinder : MonoBehaviour {
        public void StartRebind(Action<Maybe<InputBindingSource>> onComplete) {
            onComplete(Maybe.Nothing<InputBindingSource>());
        }
    }

    public class JoystickActivator : MonoBehaviour {
        public IObservable<ConnectedController?> ActiveController {
            get { return System.Reactive.Linq.Observable.Never<ConnectedController?>(); }
        }
    }

    public class InputBindings<TAction> : IDisposable {
        private readonly InputSourceMapping<TAction> _initialMapping;

        public InputBindings(
            InputSourceMapping<TAction> initialMapping,
            IObservable<InputSettings> inputSettings,
            IReadOnlyDictionary<ControllerType, InputSourceMapping<TAction>> defaultControllerMappings) {
            _initialMapping = initialMapping;
        }

        public IObservable<ActionMapConfig<TAction>> InputMappingChanges {
            get {
                return System.Reactive.Linq.Observable.Return(new ActionMapConfig<TAction> { InputMapping = _initialMapping });
            }
        }

        public void UpdateControllerId(ConnectedController? controller) {
        }

        public void UpdateMapping(TAction action, InputBindingSource source) {
        }

        public void LoadDefaultActionMap(InputDefaults defaultsType) {
        }

        public void Dispose() {
        }
    }

    // Todo: real controls got wiped along with InControl/Impero - these all resolve to empty
    // for now, so the rebinding UI shows nothing bound rather than crashing.
    public static class MenuInput {
        public static class Bindings {
            public static readonly Lazy<string> CustomInputMappingFilePath =
                new Lazy<string>(() => System.IO.Path.Combine(Application.persistentDataPath, "menu_input.json"));

            public static readonly Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<MenuAction>>> DefaultControllerMappings =
                new Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<MenuAction>>>(
                    () => new Dictionary<ControllerType, InputSourceMapping<MenuAction>>());

            public static InputSourceMapping<MenuAction> InitialMapping() {
                return new InputSourceMapping<MenuAction>();
            }

            public static IEnumerable<InputBindingViewModel> ToBindings(
                LanguageTable languageTable, InputSourceMapping<MenuAction> mapping, Maybe<UnityInputDeviceProfile> deviceProfile) {
                yield break;
            }
        }
    }

    public static class PilotInput {
        public static class Bindings {
            public static readonly Lazy<string> CustomInputMappingFilePath =
                new Lazy<string>(() => System.IO.Path.Combine(Application.persistentDataPath, "pilot_input.json"));

            public static readonly Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<WingsuitAction>>> DefaultControllerMappings =
                new Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<WingsuitAction>>>(
                    () => new Dictionary<ControllerType, InputSourceMapping<WingsuitAction>>());

            public static InputSourceMapping<WingsuitAction> InitialMapping() {
                return new InputSourceMapping<WingsuitAction>();
            }

            public static IEnumerable<InputBindingViewModel> ToBindings(
                LanguageTable languageTable, InputSourceMapping<WingsuitAction> mapping, Maybe<UnityInputDeviceProfile> deviceProfile) {
                yield break;
            }
        }
    }

    public static class SpectatorInput {
        public static class Bindings {
            public static readonly Lazy<string> CustomInputMappingFilePath =
                new Lazy<string>(() => System.IO.Path.Combine(Application.persistentDataPath, "spectator_input.json"));

            public static readonly Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<SpectatorAction>>> DefaultControllerMappings =
                new Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<SpectatorAction>>>(
                    () => new Dictionary<ControllerType, InputSourceMapping<SpectatorAction>>());

            public static InputSourceMapping<SpectatorAction> InitialMapping() {
                return new InputSourceMapping<SpectatorAction>();
            }

            public static IEnumerable<InputBindingViewModel> ToBindings(
                LanguageTable languageTable, InputSourceMapping<SpectatorAction> mapping, Maybe<UnityInputDeviceProfile> deviceProfile) {
                yield break;
            }
        }
    }

    public static class ParachuteControls {
        public static readonly Lazy<string> CustomInputMappingFilePath =
            new Lazy<string>(() => System.IO.Path.Combine(Application.persistentDataPath, "parachute_input.json"));

        public static readonly Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<ParachuteAction>>> DefaultMappings =
            new Lazy<IReadOnlyDictionary<ControllerType, InputSourceMapping<ParachuteAction>>>(
                () => new Dictionary<ControllerType, InputSourceMapping<ParachuteAction>>());

        public static InputSourceMapping<ParachuteAction> InitialMapping() {
            return new InputSourceMapping<ParachuteAction>();
        }

        public static IEnumerable<InputBindingViewModel> ToBindings(
            LanguageTable languageTable, InputSourceMapping<ParachuteAction> mapping, Maybe<UnityInputDeviceProfile> deviceProfile) {
            yield break;
        }
    }
}
