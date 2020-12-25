using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Networking;
using RamjetAnvil.Volo.Ui;
using UnityEngine;

namespace RamjetAnvil.Volo.States {
    public class ServerBrowser  : State {

        [Serializable]
        public class Data {
            [SerializeField] public ServerBrowserView View;
            [SerializeField] public ServerBrowserViewModel ViewModel;
            [SerializeField] public UnityMasterServerClient MasterServerClient;
            [SerializeField] public ActiveNetwork ActiveNetwork;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public VersionInfo VersionInfo;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public FaderSettings FaderSettings;
            [SerializeField] public AbstractUnityEventSystem EventSystem;
        }

        private readonly Data _data;
        private IDisposable _onPausePressed;

        public ServerBrowser(IStateMachine machine, Data data) : base(machine) {
            _data = data;
        }

        IEnumerator<WaitCommand> OnEnter() {
            _data.View.gameObject.SetActive(true);

            _data.ViewModel.CheckMasterServerHealth();
            _data.ViewModel.Refresh();

            yield return CameraTransitions.FadeIn(_data.CameraManager.Rig.ScreenFader, _data.GameClock, _data.FaderSettings).AsWaitCommand();

            _onPausePressed = _data.EventSystem.Listen<Events.OnBackPressed>(OnBackPressed);
            
            _data.ViewModel.RequestJoin += OnRequestJoin;
            _data.ViewModel.RequestCancelJoin += OnRequestCancelJoin;
        }

        private void OnBackPressed() {
            Machine.Transition(VoloStateMachine.States.MainMenu);        
        }

        private void OnRequestCancelJoin() {
            _data.ActiveNetwork.CancelJoin();
        }

        private void OnRequestJoin(HostEntry hostEntry) {
            var gameSettings = _data.GameSettingsProvider.ActiveSettings;
            var clientPort = gameSettings.Other.AutomaticNetworkPort ? -1 : gameSettings.Other.NetworkPort;
            _data.ActiveNetwork.JoinServer(
                hostEntry.Host.PeerInfo.External,
                clientPort,
                (connectionId, endpoint) => {
                    Debug.Log("connection succeeded to " + endpoint + " " + connectionId);
                    _data.ViewModel.JoinSucceeded();
                    Machine.Transition(VoloStateMachine.States.SpawnScreen);        
                },
                (endpoint, exception) => {
                    // Handle connection failure
                    _data.ViewModel.JoinFailed();
                },
                (endpoint) => {
                    // Handle disconnect
                    _data.ViewModel.JoinFailed();
                });
        }

        IEnumerator<WaitCommand> OnExit() {
            _onPausePressed.Dispose();
            _data.ViewModel.RequestJoin -= OnRequestJoin;
            _data.ViewModel.RequestCancelJoin -= OnRequestCancelJoin;

            yield return CameraTransitions.FadeOut(_data.CameraManager.Rig.ScreenFader, _data.GameClock, _data.FaderSettings).AsWaitCommand();
            _data.View.gameObject.SetActive(false);
        }
    }
}
