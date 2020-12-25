using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using RxUnity.Schedulers;
using UnityEngine;
using Resolution = RamjetAnvil.Volo.Resolution;

public class ScreenshotMaker : MonoBehaviour {

    [Dependency, SerializeField] private MenuActionMapProvider _menuActionMapProvider;
    [Dependency, SerializeField] private GameSettingsProvider _gameSettingsProvider;
    [Dependency, SerializeField] private CameraRenderer _cameraRenderer;

    private Texture2D _screenshotTexture;
    private string _screenshotsPath;

    private Resolution _screenshotResolution;
    private bool _isInitialized;
    private volatile bool _isWritingScreenshot;
    private Action _onWritingScreenshotComplete;

    void Awake() {
        _isInitialized = false;

        _isWritingScreenshot = false;
        _onWritingScreenshotComplete = () => {
            _isWritingScreenshot = false;
        };
        _screenshotsPath = Path.Combine(UnityFileBrowserUtil.VoloAirsportDir.Value, "screenshots");

        gameObject.AddComponent<ScreenResolutionNotifier>()
            .CurrentResolution
            .CombineLatest(_gameSettingsProvider.SettingChanges, (screenResolution, settings) => {
                return new Resolution(
                    width: Mathf.FloorToInt(Screen.width * settings.Graphics.ScreenshotMagnificationFactor),
                    height: Mathf.FloorToInt(Screen.height * settings.Graphics.ScreenshotMagnificationFactor));
            })
            .DistinctUntilChanged()
            .Subscribe(resolution => {
                //Debug.Log("setting resolution to " + resolution.Width + "x" + resolution.Height);
                _screenshotResolution = resolution;
                _screenshotTexture = new Texture2D(resolution.Width, resolution.Height,
                    TextureFormat.RGB24, mipmap: false, linear: true);
            });
    }

    void LateUpdate() {
        if (!_isInitialized && _screenshotTexture != null && _cameraRenderer != null) {
            _isInitialized = true;
        }
        if (_isInitialized) {
            TryTakeScreenshot();
        }
    }

    void TryTakeScreenshot() {
        var actionMap = _menuActionMapProvider.ActionMap.V;
        if (!_isWritingScreenshot && actionMap.PollButtonEvent(MenuAction.TakeScreenshot) == ButtonEvent.Down) {
            Debug.Log("Taking Screenshot");
            TakeScreenshot();
        }        
    }

    void TakeScreenshot() {
        _isWritingScreenshot = true;

        var renderTexture = RenderTexture.GetTemporary(_screenshotResolution.Width, _screenshotResolution.Height, 24, RenderTextureFormat.Default);
        _cameraRenderer.Render(renderTexture);

        try {
            var screenshot = ScreenshotUtils.CopyFromRenderTexture(renderTexture, _screenshotTexture);
            WriteScreenshot(screenshot);
        } catch (Exception) {
            _isWritingScreenshot = false;
            throw;
        } finally {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    private void WriteScreenshot(Screenshot screenshot) {
        Debug.Log(_screenshotsPath);
        ScreenshotUtils.Write2File(_screenshotsPath, screenshot, useAlphaChannel: false, onComplete: _onWritingScreenshotComplete);
    }
}
