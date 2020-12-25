using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Impero.Util;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Input {

    public struct ActionMapConfig<TAction> {
        public ControllerId ControllerId;
        public InputSettings InputSettings;
        public InputSourceMapping<TAction> InputMapping;
    }

    public static class ActionMapConfig {
        public static IObservable<ActionMapConfig<TAction>> CreateActionMapConfig<TAction>(
            IObservable<ControllerId> activeJoystick, 
            IObservable<InputSettings> inputSettings,
            IObservable<InputSourceMapping<TAction>> inputMappingChanges) {

            return activeJoystick.CombineLatest(inputMappingChanges, inputSettings, (controllerId, inputMapping, settings) => {
                return new ActionMapConfig<TAction> {
                    ControllerId = controllerId,
                    InputSettings = settings,
                    InputMapping = inputMapping
                };
            });
        }
    }
}
