using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InControl {
    public static class UnityInputDeviceProfiles {
        public static readonly IDictionary<string, IDictionary<string, UnityInputDeviceProfile>> DeviceProfiles; 
        public static readonly IDictionary<Type, IDictionary<InputControlSource, InputControlMapping>> InputSources; 
        static UnityInputDeviceProfiles() {
            DeviceProfiles = new Dictionary<string, IDictionary<string, UnityInputDeviceProfile>>();
            InputSources = new Dictionary<Type, IDictionary<InputControlSource, InputControlMapping>>();

            var deviceProfiles = Assembly.GetAssembly(typeof (UnityInputDeviceProfile))
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof (UnityInputDeviceProfile)) &&
                               !type.IsAbstract)
                .Select(type => Activator.CreateInstance(type))
                .Cast<UnityInputDeviceProfile>();

            foreach (var deviceProfile in deviceProfiles) {
                foreach (var supportedPlatform in deviceProfile.SupportedPlatforms) {
                    IDictionary<string, UnityInputDeviceProfile> storedProfiles;
                    if (!DeviceProfiles.TryGetValue(supportedPlatform, out storedProfiles)) {
                        storedProfiles = new Dictionary<string, UnityInputDeviceProfile>();
                        DeviceProfiles[supportedPlatform] = storedProfiles;
                    }

                    var inputSources = new Dictionary<InputControlSource, InputControlMapping>();
                    InputSources[deviceProfile.GetType()] = inputSources;

                    foreach (var inputMapping in deviceProfile.AnalogMappings.Concat(deviceProfile.ButtonMappings)) {
                        inputSources[inputMapping.Source] = inputMapping;
                    }

                    // TODO Add support for regex
                    if (deviceProfile.JoystickNames != null) {
                        foreach (var joystickName in deviceProfile.JoystickNames) {
                            storedProfiles[joystickName.ToLowerInvariant()] = deviceProfile;
                        }
                    }
                }
            }
        }
    }
}
