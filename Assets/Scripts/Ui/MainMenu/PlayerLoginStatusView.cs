using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Padrone.Client;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    // TODO Add retry mechanism to login

    public class PlayerLoginStatusView : MonoBehaviour {
        [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;
        [SerializeField] private Image _background;
        [SerializeField] private Text _personaName;
        [SerializeField] private GameObject _developer;
        [SerializeField] private RawImage _avatarImage;

        [SerializeField] private Color _normalBackgroundColor;
        [SerializeField] private Color _developerBackgroundColor;

        private IDisposable _runningAvatarDownload;

        void Awake() {
            _runningAvatarDownload = Disposables.Empty;
            _avatarImage.texture = new Texture2D(64, 64);
            gameObject.SetActive(false);
        }

        public void UpdateStatus(Maybe<PlayerInfo> playerInfo) {
            _runningAvatarDownload.Dispose();

            if (playerInfo.IsJust) {
                var playerName = playerInfo.Value.Name != null ? playerInfo.Value.Name.Trim().Limit(50) : "";
                _personaName.supportRichText = string.IsNullOrEmpty(playerName);
                _personaName.text = string.IsNullOrEmpty(playerName) ? "<i>Anonymous player</i>" : playerName;

                var avatarUrl = playerInfo.Value.AvatarUrl;
                if (avatarUrl != null) {
                    _runningAvatarDownload = _coroutineScheduler.Run(DownloadAvatar(avatarUrl));    
                }

                if (playerInfo.Value.IsDeveloper) {
                    _developer.SetActive(true);
                    _background.color = _developerBackgroundColor;
                } else {
                    _developer.SetActive(false);
                    _background.color = _normalBackgroundColor;
                }
            } else {
                _developer.SetActive(false);
                _personaName.supportRichText = true;
                _personaName.text = "<i>Not logged in</i>";
                _avatarImage.enabled = false;
                _runningAvatarDownload = Disposables.Empty;
                _background.color = _normalBackgroundColor;
            }
        }

        private IEnumerator<WaitCommand> DownloadAvatar(string url) {
            _avatarImage.enabled = false;

            if (!string.IsNullOrEmpty(url)) {
                var request = new WWW(url);
                while (!request.isDone) {
                    yield return WaitCommand.WaitForNextFrame;
                }
                var avatarTexture = (Texture2D) _avatarImage.texture;
                request.LoadImageIntoTexture(avatarTexture);
                _avatarImage.enabled = true;
            }
        } 
    }
}
