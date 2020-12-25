using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using InControl;
using RamjetAnvil.Impero.Util;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Impero.Unity {

    public enum ControllerType {
        Xbox360,
        XboxOne,
        SteamController,
        Playstation4,
        Other
    }

    public static class Controllers {

        private static readonly IImmutableList<Type> Xbox360Profiles = new Type[] {
            typeof(Xbox360WinProfile),
            typeof(Xbox360MacProfile),
            typeof(Xbox360LinuxProfile),
            typeof(Xbox360AndroidProfile),
        }.ToImmutableList();

        // TODO Create xbox one linux profile
        private static readonly IImmutableList<Type> XboxOneProfiles = new Type[] {
            typeof(XboxOneWinProfile),
            typeof(XboxOneMacProfile),
            typeof(XboxOneProfile),
        }.ToImmutableList();

        private static readonly IImmutableList<Type> Playstation4Profiles = new Type[] {
            typeof(PlayStation4AndroidProfile),
            typeof(PlayStation4LinuxProfile),
            typeof(PlayStation4MacBTProfile),
            typeof(PlayStation4MacUSBProfile),
            typeof(PlayStation4Profile),
            typeof(PlayStation4WinProfile),
        }.ToImmutableList();

        public static readonly Func<string, ControllerType> UnityIdToControllerType = Memoization.Memoize<string, ControllerType>(
            unityControllerId => {
                ControllerType controllerType;
                var deviceProfile = UnityIdToDeviceProfile(unityControllerId);
                if (deviceProfile.IsJust) {
                    var deviceProfileType = deviceProfile.Value.GetType();
                    if (Xbox360Profiles.Contains(deviceProfileType)) {
                        controllerType = ControllerType.Xbox360;
                    } else if (XboxOneProfiles.Contains(deviceProfileType)) {
                        controllerType = ControllerType.XboxOne;
                    } else if (Playstation4Profiles.Contains(deviceProfileType)) {
                        controllerType = ControllerType.Playstation4;
                    } else {
                        controllerType = ControllerType.Other;
                    }
                } else {
                    controllerType = ControllerType.Other;
                }
                return controllerType;
            });

        public static readonly Func<string, Maybe<UnityInputDeviceProfile>> UnityIdToDeviceProfile = Memoization
            .Memoize<string, Maybe<UnityInputDeviceProfile>>(
                unityControllerId => {
                    string os;
                    switch (PlatformUtil.CurrentOs()) {
                        case PlatformUtil.OperatingSystem.Windows:
                            os = "Windows";
                            break;
                        case PlatformUtil.OperatingSystem.MacOsx:
                            os = "OS X";
                            break;
                        case PlatformUtil.OperatingSystem.Linux:
                            os = "Linux";
                            break;
                        default:
                            throw new Exception("Operating system " + PlatformUtil.CurrentOs() + " not supported by input library");
                    }

                    var deviceProfiles = UnityInputDeviceProfiles.DeviceProfiles[os];
                    return deviceProfiles.TryGetValue(unityControllerId.ToLowerInvariant());
                });
    }
}
