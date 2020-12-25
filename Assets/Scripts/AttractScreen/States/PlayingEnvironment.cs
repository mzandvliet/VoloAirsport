using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.States
{
    public struct PlayingEnvironment {
        public readonly SpawnpointLocation Spawnpoint;
        public readonly Wingsuit Pilot;
        public readonly ParachuteConfig ParachuteConfig;
        public readonly PilotCameraMountId SelectedCameraMount;

        public PlayingEnvironment(SpawnpointLocation spawnpoint, Wingsuit pilot, ParachuteConfig parachuteConfig,
            PilotCameraMountId selectedMount = PilotCameraMountId.Orbit) {
            Spawnpoint = spawnpoint;
            Pilot = pilot;
            ParachuteConfig = parachuteConfig;
            SelectedCameraMount = selectedMount;
        }

        public PlayingEnvironment UpdateParachuteConfig(ParachuteConfig parachuteConfig) {
            return new PlayingEnvironment(Spawnpoint, Pilot, parachuteConfig, SelectedCameraMount);
        }

        public PlayingEnvironment SelectMount(PilotCameraMountId cameraMount) {
            return new PlayingEnvironment(Spawnpoint, Pilot, ParachuteConfig, cameraMount);
        }

        public PlayingEnvironment NextMount() {
            var cameraMount = EnumUtils.GetValues<PilotCameraMountId>().GetNext(SelectedCameraMount);
            return new PlayingEnvironment(Spawnpoint, Pilot, ParachuteConfig, cameraMount);
        }

        public PlayingEnvironment UpdatePilot(Wingsuit activePilot) {
            return new PlayingEnvironment(Spawnpoint, activePilot, ParachuteConfig, SelectedCameraMount);
        }
    }
}
