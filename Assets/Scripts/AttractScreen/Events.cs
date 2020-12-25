namespace RamjetAnvil.Volo {
    public static class Events {
        public struct OnConfirmPressed { }
        public struct OnPausePressed { }
        public struct OnBackPressed { }
        public struct FreezeGame { }
        public struct UnfreezeGame { }

        public struct PlayerSpawned {
            public readonly Wingsuit Player;

            public PlayerSpawned(Wingsuit player) {
                Player = player;
            }
        }

        public struct SpawnpointSelected {
            public readonly SpawnpointLocation Spawnpoint;

            public SpawnpointSelected(SpawnpointLocation spawnpoint) {
                Spawnpoint = spawnpoint;
            }
        }

        public struct SettingsUpdated {
            public readonly GameSettings Settings;

            public SettingsUpdated(GameSettings settings) {
                Settings = settings;
            }
        }
    }
}