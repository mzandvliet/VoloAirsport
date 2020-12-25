using System;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.RamNet;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Networking;
using UnityEngine;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Volo.States;

/* Todo: Create a shared Data container for things like Clocks, because every state manipulates them */

public class VoloStateMachine : MonoBehaviour {
    [SerializeField] private NewsFlash.Data _newsFlashData;
    [SerializeField] private TitleScreen.Data _titleScreenData;
    [SerializeField] private SpawnScreen.Data _spawnScreenData;
    [SerializeField] private MainMenu.Data _mainMenuData;
    [SerializeField] private ServerBrowser.Data _serverBrowserData;
    [SerializeField] private Playing.Data _playingData;
    [SerializeField] private ParachuteStates.Data _flyParachuteData;
    [SerializeField] private FlyWingsuit.Data _flyWingsuitData;
    [SerializeField] private OptionsMenuState.Data _optionsMenuData;
    [SerializeField] private Frozen.Data _frozenData;
    [SerializeField] private SpectatorMode.Data _spectatorModeData;
    [SerializeField] private InCourseEditor.Data _inCourseEditorData;
    [SerializeField] private MenuActionMapProvider _menuActionMapProvider;

    [SerializeField] private AbstractUnityClock GameClock;
    [SerializeField] private AbstractUnityClock FixedClock;
    [SerializeField] private GameSettingsProvider _gameSettingsProvider;

    public class States {
        public static readonly StateId NewsFlash = new StateId("NewsFlash");
        public static readonly StateId TitleScreen = new StateId("TitleScreen");
        public static readonly StateId MainMenu = new StateId("MainMenu");
        public static readonly StateId ServerBrowser = new StateId("ServerBrowser");
        public static readonly StateId SpawnScreen = new StateId("SpawnScreen");
        public static readonly StateId Playing = new StateId("Playing");
        public static readonly StateId OptionsMenu = new StateId("OptionsMenu");
        public static readonly StateId Frozen = new StateId("Frozen");
    }

    private StateMachine<VoloStateMachine> _machine;

    private void Start() {
        _spawnScreenData.MenuActionMap = _menuActionMapProvider.ActionMap; 
    }

    public void StartMachine(ICoroutineScheduler scheduler, DependencyContainer globalDependencies) {
#region Networking

        var gameSettings = _gameSettingsProvider.ActiveSettings;

        var networking = GameObject.Find("Networking");
        var lidgrenTransporter = networking.GetComponentInChildren<LidgrenNetworkTransporter>();
        var networkMessageSender = networking.GetComponentInChildren<QueueingMessageSender>();
        networkMessageSender.Transporter = lidgrenTransporter;

        var connectionIdPool = new ConnectionIdPool(maxConnectionIds: 64);
        //var connectionManager = new DefaultConnectionManager(lidgrenTransporter, connectionIdPool);
        var natFacilitatorEndpoint = Ipv4Endpoint.Parse(gameSettings.Other.NatFacilitatorEndpoint).ToIpEndPoint();
        var natFacilitatorConnection = new LidgrenNatFacilitatorConnection(natFacilitatorEndpoint, lidgrenTransporter);
        var natPunchClient = new LidgrenNatPunchClient(scheduler, natFacilitatorConnection);
        var connectionAttemptTimeout = gameSettings.Other.ConnectionAttemptTimeout;
        var natPunchConnectionManager = new LidgrenPunchThroughFacilitator(lidgrenTransporter, scheduler, natPunchClient,
            connectionAttemptTimeout, connectionIdPool);

        lidgrenTransporter.ConnectionIdPool = connectionIdPool;
        var latencyInfo = new LatencyInfo(connectionIdPool.MaxConnectionIds);

        var groupRouterConfig = ConnectionGroups.RouterConfig;
        var networkSystems = NetworkSystem.Create(
            lidgrenTransporter,
            lidgrenTransporter,
            groupRouterConfig,
            ConnectionGroups.Default,
            ReplicationObjects.Factories,
            networkMessageSender,
            natPunchConnectionManager,
            latencyInfo,
            globalDependencies);
        var voloServer = networking.GetComponentInChildren<VoloNetworkServer>();
        voloServer.NetworkSystems = networkSystems;
        voloServer.NatFacilitatorConnection = natFacilitatorConnection;
        var voloClient = networking.GetComponentInChildren<VoloNetworkClient>();
        voloClient.NetworkSystems = networkSystems;
        voloClient.LatencyInfo = latencyInfo;

        var activeNetwork = new ActiveNetwork(networkSystems, voloServer, voloClient, scheduler);
        _playingData.ActiveNetwork = activeNetwork;
        _mainMenuData.ActiveNetwork = activeNetwork;
        _serverBrowserData.ActiveNetwork = activeNetwork;

        /*
         * Create lean and mean main menu system that allows a player to choose between single and multiplayer
         *  
         * Notes:
         *   - Keep as much logic that is generic to both single and multiplayer generic. Don't make a special code path
         *     for singleplayer unless it is a feature that is only available in singleplayer.
         *   - Options menu should be as game independent as possible, not relying on a specific singleplayer state for example.
         *   - State machine transitions should also become separate states such that we can handle them in a better, cleaner way
         * 
         * 
         * Case 1: Single player game
         * - Boot game, start singleplayer, fly for one round, quit game
         *   - On boot: create instance of a network system
         *   - Pass network system into the state machine
         *   - When going into the playing state, spawn/despawn/respawn pilot through replicator
         *   
         * Case 2: Multiplayer game - server
         * - Boot game, main screen: choose to create multiplayer server,
         *   - on boot create instance of network system
         *   - when in starting multiplayer server state open transporter
         *   - when a spawnpoint is selected replicate the player
         * - separate state machine for multiplayer logic
         * 
         * Case 3: Multiplayer game - client
         * - Boot game, main screen: choose to join a multiplayer server
         *   - server list: allow client to join a game, show if join/connect is in progress, allow for cancellation
         *   - when a spawnpoint is selected request a spawn to the server
         */ 


#endregion

        _machine = new StateMachine<VoloStateMachine>(this, scheduler);

        _machine.AddState(States.NewsFlash, new NewsFlash(_machine, _newsFlashData))
            .Permit(States.TitleScreen);

        _machine.AddState(States.TitleScreen, new TitleScreen(_machine, _titleScreenData))
            .PermitChild(States.Frozen)
            .Permit(States.MainMenu);

        _machine.AddState(States.MainMenu, new MainMenu(_machine, _mainMenuData))
            .PermitChild(States.Frozen)
            .Permit(States.TitleScreen)
            .Permit(States.ServerBrowser)
            .Permit(States.SpawnScreen);

        _machine.AddState(States.ServerBrowser, new ServerBrowser(_machine, _serverBrowserData))
            .PermitChild(States.Frozen)
            .Permit(States.MainMenu)
            .Permit(States.SpawnScreen);

        _machine.AddState(States.SpawnScreen, new SpawnScreen(_machine, _spawnScreenData))
            .PermitChild(States.Frozen)
            .PermitChild(States.OptionsMenu)
            .Permit(States.MainMenu)
            .Permit(States.Playing);

        _machine.AddState(States.Playing, new Playing(_machine, _playingData, _flyWingsuitData, _flyParachuteData, _spectatorModeData))
            .PermitChild(States.Frozen)
            .PermitChild(States.OptionsMenu)
            .Permit(States.SpawnScreen);

        _machine.AddState(States.OptionsMenu, new OptionsMenuState(_machine, _optionsMenuData))
            .PermitChild(States.Frozen)
            .Permit(States.SpawnScreen)
            .Permit(States.MainMenu);

        _machine.AddState(States.Frozen, new Frozen(_machine, _frozenData));

        _machine.Transition(States.NewsFlash);
    }

    [StateEvent("Update")]
    public event Action OnUpdate;

    private void Update() {
        if (OnUpdate != null) {
            OnUpdate();
        }
    }
}
