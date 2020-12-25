using System;
using System.Reactive.Disposables;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.Util.UnitOfMeasure;
using UnityEngine;

public class GameHud : MonoBehaviour {
    [SerializeField] private float _smoothingSpeed = 1f;
    [Dependency, SerializeField] private AbstractUnityEventSystem _eventSystem; // Todo: Can we decouple this from Event System?
    [Dependency("gameClock"), SerializeField] private AbstractUnityClock _gameClock;
    [SerializeField] private AvatarType _avatarType;
    [SerializeField] private FlightStatistics _flightStats;
    [Dependency, SerializeField] private GameSettingsProvider _activeGameSettings;
    [Dependency, SerializeField] private ActiveLanguage _activeLanguage;
    [SerializeField] private MetersView _view;

    private UnitSystem _unitSystem;
    private MetersViewModel _viewModel;
    private bool _isVisible;
    private bool _isActive;
    private Vector3 _prevSmoothAirSpeed;
    private ParachuteInput _parachuteInput;
    private IDisposable _settingUpdates;

    void Awake() {
        _viewModel = new MetersViewModel();
        _viewModel.DataUpdated += _view.SetData;
        _viewModel.TitlesUpdated += _view.SetTitles;
        _isVisible = true;
        Deactivate();

        var settingUpdates = new CompositeDisposable();

        var gameSettingUpdates = _activeGameSettings.SettingChanges.Subscribe(gameSettings => {
            UpdateSettings(gameSettings.Gameplay);
        });
        settingUpdates.Add(gameSettingUpdates);

        var languageUpdates = _activeLanguage.TableUpdates.Subscribe(languageTable => {
            SetLanguage(languageTable);
        });
        settingUpdates.Add(languageUpdates);

        _settingUpdates = settingUpdates;
    }

    void OnDestroy() {
        _viewModel.DataUpdated -= _view.SetData;
        _viewModel.TitlesUpdated -= _view.SetTitles;
        _settingUpdates.Dispose();
    }

    public void Activate() {
        _isActive = true;
        UpdateVisibility();
    }

    public void Deactivate() {
        _isActive = false;
        UpdateVisibility();
    }

    public void SetTarget(FlightStatistics target, AvatarType avatarType) {
        _avatarType = avatarType;
        _flightStats = target;
        _prevSmoothAirSpeed = Vector3.zero;
    }

    public void SetParachuteInput(ParachuteInput input) {
        _parachuteInput = input;
    }

    private void SetLanguage(LanguageTable language) {
        _viewModel.SetLanguage(language);
    }

    private void UpdateSettings(GameSettings.GameplaySettings settings) {
        _unitSystem = settings.UnitSystem;
        _isVisible = settings.ShowHud;
        UpdateVisibility();
    }

    void Update() {
        if (_flightStats != null) {
            var smoothAirSpeed = Vector3.Lerp(_prevSmoothAirSpeed, _flightStats.RelativeVelocity, _gameClock.DeltaTime * _smoothingSpeed);
            _prevSmoothAirSpeed = smoothAirSpeed;

            var nonGravitationalAcceleration = _flightStats.Acceleration;// - Vector3.down * 9.81f;
            var gforce = nonGravitationalAcceleration.magnitude / 9.81f;

            Vector2 linePull;
            ParachuteLine? selectedParachuteLines;
            if (_avatarType == AvatarType.Parachute) {
                linePull = _parachuteInput.SelectedLinePull;
                selectedParachuteLines = _parachuteInput.SelectedLine;
            } else {
                linePull = Vector2.zero;
                selectedParachuteLines = null;
            }

            if (_unitSystem == UnitSystem.Metric) {
                var flightData = FlightViewData<MeasureSystem.Metric>.Create(
                    smoothAirSpeed,
                    _flightStats.AltitudeGround,
                    _flightStats.GlideRatio, 
                    gforce,
                    selectedParachuteLines,
                    linePull);
                _viewModel.UpdateRawData(ref flightData);
            } else {
                var flightData = FlightViewData<MeasureSystem.Imperial>.Create(
                    smoothAirSpeed,
                    _flightStats.AltitudeGround,
                    _flightStats.GlideRatio, 
                    gforce,
                    selectedParachuteLines,
                    linePull);
                _viewModel.UpdateRawData(ref flightData);
            }
        }
    }

    private void UpdateVisibility() {
        if (_isActive && _isVisible) {
            _view.Show();
        } else {
            _view.Hide();
        }
    }

}
