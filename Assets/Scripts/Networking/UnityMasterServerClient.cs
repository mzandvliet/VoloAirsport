using System;
using System.Net;
using System.Reactive.Linq;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Padrone.Client;
using RamjetAnvil.Volo.Networking;
using UnityEngine;

public class UnityMasterServerClient : MonoBehaviour {

    [SerializeField] private VersionInfo _versionInfo;
    [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;
    [SerializeField] private GameSettingsProvider _gameSettingsProvider;
    [SerializeField] private float _requestTimeout = 3f;

    private PadroneClient _client;

    void Awake() {
        var appVersion = _versionInfo.VersionNumber;
        var authTokenProvider = Authenticators.All;
        _gameSettingsProvider.SettingChanges
            .Select(gameSettings => gameSettings.Other.MasterServerUrl)
            .DistinctUntilChanged()
            .Subscribe(url => {
                Debug.Log("masterserver url " + url);

                // Bug: The below fails if the GameSettings.json in MyDocs is invalid. Catch that.

                _client = new PadroneClient(url, appVersion,
                    authTokenProvider,
                    _coroutineScheduler,
                    TimeSpan.FromSeconds(_requestTimeout));
            });
    }

    public PadroneClient Client {
        get { return _client; }
    }
}
