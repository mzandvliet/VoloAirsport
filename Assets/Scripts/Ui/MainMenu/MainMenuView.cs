using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Padrone.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class MainMenuView : MonoBehaviour, IUiContext {

        public event Action<StartGame> OnStartGame;
        public event Action OpenOptionsMenu;
        public event Action ToTitleScreen;

        [SerializeField] private PlayerLoginStatusView _loginStatusView;
        [SerializeField] private CustomizableButton _playSingleplayer;
        [SerializeField] private InputField _serverName;
        [SerializeField] private CustomizableButton _hostMultiplayer;
        [SerializeField] private CustomizableButton _joinMultiplayer;
        //[SerializeField] private CustomizableButton _options; // Note: Disabled because resume options from it crashes game
        [SerializeField] private CustomizableButton _toTitleScreen;

        void Awake() {
            _playSingleplayer.OnSubmit.AddListener(() => Run(StartGame.Singleplayer));
            _hostMultiplayer.OnSubmit.AddListener(() => Run(StartGame.HostMultiplayer));
            _joinMultiplayer.OnSubmit.AddListener(() => Run(StartGame.JoinMultiplayer));
//            _options.OnSubmit.AddListener(() => {
//                if (OpenOptionsMenu != null) {
//                    OpenOptionsMenu();
//                }
//            });
            _toTitleScreen.OnSubmit.AddListener(() => {
                if (ToTitleScreen != null) {
                    ToTitleScreen();
                }
            });
            Hide();
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void UpdateLoginStatus(Maybe<PlayerInfo> playerInfo) {
            _loginStatusView.UpdateStatus(playerInfo);
            _loginStatusView.gameObject.SetActive(true);
        }

        public GameObject FirstObject {
            get { return _playSingleplayer.gameObject; }
        }

        private void Run(StartGame start) {
            if (OnStartGame != null) {
                OnStartGame(start);
            }
        }

        public string ServerName {
            get { return _serverName.text; }
        }
    }
}
