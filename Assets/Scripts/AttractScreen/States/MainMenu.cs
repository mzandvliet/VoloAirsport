using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.InputModule;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Padrone.Client;
using RamjetAnvil.Volo.Networking;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RamjetAnvil.Volo.States {
    public class MainMenu : State {

        [Serializable]
        public class Data {
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public FaderSettings FaderSettings;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            [SerializeField] public RamjetInputModule InputModule;
            [SerializeField] public MusicPlayer MusicPlayer;

            [SerializeField] public MainMenuView MainMenuView;
            [SerializeField] public UnityMasterServerClient MasterServerClient;
            public ActiveNetwork ActiveNetwork;
        }

        private readonly Data _data;

        public MainMenu(IStateMachine machine, Data data) : base(machine) {
            _data = data;
            _data.MasterServerClient.Client.Me((statusCode, playerInfo) => {
                if (statusCode == HttpStatusCode.OK) {
                    _data.MainMenuView.UpdateLoginStatus(Maybe.Just(playerInfo));
                    Debug.Log("Logged in as: " + playerInfo.Name + ", avatar url: " + playerInfo.AvatarUrl);
                } else {
                    _data.MainMenuView.UpdateLoginStatus(Maybe.Nothing<PlayerInfo>());
                    Debug.LogError("Failed to fetch player details from Master server, error: " + statusCode);
                }
            });
        }

        IEnumerator<WaitCommand> OnEnter() {
            yield return CameraTransitions.FadeIn(_data.CameraManager.Rig.ScreenFader, _data.GameClock, _data.FaderSettings).AsWaitCommand();

            _data.ActiveNetwork.Shutdown();
            OnResume();
        }

        private void OnResume() {
            Cursor.lockState = CursorLockMode.None;
            _data.MainMenuView.OnStartGame += OnStartGame;
            _data.MainMenuView.ToTitleScreen += ToTitleScreen;
            _data.MainMenuView.Show();
            _data.InputModule.ReContextualize(_data.MainMenuView.FirstObject);
        }

        private void ToTitleScreen() {
            Machine.Transition(VoloStateMachine.States.TitleScreen);
        }

        private void OnStartGame(StartGame startGame) {
            var gameSettings = _data.GameSettingsProvider.ActiveSettings;

            // TODO Report to player if opening socket failed
            switch (startGame) {
                case StartGame.Singleplayer:
                    _data.ActiveNetwork.StartSingleplayerServer();

                    _data.MusicPlayer.StopAndFade();
                    _data.MusicPlayer.PlayRandomMusic();

                    Machine.Transition(VoloStateMachine.States.SpawnScreen);
                    break;
                case StartGame.HostMultiplayer:
                    var serverPort = gameSettings.Other.AutomaticNetworkPort ? -1 : gameSettings.Other.NetworkPort;
                    const bool isPrivate = false;
                    const int maxPlayers = 4;
                    string serverName = _data.MainMenuView.ServerName;
                    serverName = serverName.Trim();
                    serverName = serverName == "" ? "A server has no name" : serverName;
                    _data.ActiveNetwork.StartServer(serverName, maxPlayers, isPrivate, serverPort);
                    Machine.Transition(VoloStateMachine.States.SpawnScreen);
                    break;
                case StartGame.JoinMultiplayer:
                    Machine.Transition(VoloStateMachine.States.ServerBrowser);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("startGame");
            }
        }

        private void OnSuspend() {
            _data.MainMenuView.OnStartGame -= OnStartGame;
            _data.MainMenuView.ToTitleScreen -= ToTitleScreen;
            _data.MainMenuView.Hide();
        }

        IEnumerator<WaitCommand> OnExit() {
            OnSuspend();
            yield return CameraTransitions.FadeOut(_data.CameraManager.Rig.ScreenFader, _data.GameClock, _data.FaderSettings).AsWaitCommand();
            _data.MainMenuView.Hide();
        }
    }
}
