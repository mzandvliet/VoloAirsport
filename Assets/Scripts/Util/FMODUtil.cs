using System;
using FMOD;
using FMOD.Studio;
using Fmod = FMODUnity.RuntimeManager;
using RamjetAnvil.Reactive;

namespace RamjetAnvil.Volo.Util {
    public static class FMODUtil {

        public static Bus GetBus(string path) {
            Bus bus;
            var returnCode = Fmod.StudioSystem.getBus(path, out bus);
            if (returnCode != RESULT.OK) {
                throw new FMODException("FMOD call failed", returnCode);
            }
            return bus;
        }

        public static IObservable<FMOD.Studio.System> StudioSystem() {
            return UnityObservable.CreateUpdate<FMOD.Studio.System>(observer => {
                if (Fmod.StudioSystem.isValid()) {
                    observer.OnNext(Fmod.StudioSystem);
                    observer.OnCompleted();
                }
            });
        }

    }
}

