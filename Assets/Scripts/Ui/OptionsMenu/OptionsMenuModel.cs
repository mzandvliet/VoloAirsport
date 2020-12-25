using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.States;
using StringLeakTest;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public enum Menu {
        Main, Options, Graphics, Input, Audio, Gameplay, TimeAndWeather, Other,
        Tutorial, FollowUs, Credits
    }

    public class OptionsMenuModel {

        public event Action<MenuActionId> OnMenuClosed;
        public event Action<OptionsMenuModel> Updated;
        public event Action<Menu> MenuLoaded;

        public readonly Stack<Menu> MenuStack;
        public readonly AudioSettingsViewModel AudioSettings;
        public readonly GraphicsSettingsViewModel GraphicsSettings;
        public readonly GameplaySettingsViewModel GameplaySettings;
        public readonly TimeAndWeatherSettingsViewModel TimeAndWeatherSettings;
        public readonly InputSettingsViewModel InputSettings;
        public readonly OtherSettingsViewModel OtherSettings;
        public readonly string AppVersion;

        private LanguageTable _languageTable;

        private InputBindingViewModel? _rebinding;
        private InputBindingViewModel[] _inputBindings;
        private readonly Action<InputBindingViewModel> _startRebind;
        private readonly Action<InputDefaults> _restoreInputToDefaults;

        private MenuId _menuState;
        private readonly GameSettingsProvider _gameSettingsProvider;
        private readonly GameSettingsUpdater _gameSettingsUpdater;

        public OptionsMenuModel(GameSettingsProvider gameSettingsProvider, Languages languages, string appVersion, 
            Action<InputBindingViewModel> startRebind,
            Action<InputDefaults> restoreInputToDefaults) {

            MenuStack = new Stack<Menu>();
            MenuStack.Push(Menu.Main);

            AppVersion = "v" + appVersion;
            _inputBindings = null;
            _menuState = MenuId.StartSelection;
            _gameSettingsProvider = gameSettingsProvider;
            _restoreInputToDefaults = restoreInputToDefaults;
            _startRebind = startRebind;
            _gameSettingsUpdater = new GameSettingsUpdater(gameSettingsProvider);
            AudioSettings = new AudioSettingsViewModel(_gameSettingsUpdater);
            GraphicsSettings = new GraphicsSettingsViewModel(_gameSettingsUpdater);
            GameplaySettings = new GameplaySettingsViewModel(_gameSettingsUpdater);
            TimeAndWeatherSettings = new TimeAndWeatherSettingsViewModel(_gameSettingsUpdater);
            InputSettings = new InputSettingsViewModel(_gameSettingsUpdater);
            OtherSettings = new OtherSettingsViewModel(_gameSettingsUpdater, languages);

            _gameSettingsProvider.SettingChanges.Subscribe(OnUpdate);
            _gameSettingsUpdater.OnUnappliedSettingsUpdated += unappliedSettings => OnUpdate();
        }

        public void PushMenu(Menu menu) {
            MenuStack.Push(menu);
            OnUpdate();
            if (MenuLoaded != null) {
                MenuLoaded(MenuStack.Peek());
            }
        }

        public void PopMenu() {
            if (!_rebinding.HasValue) {
                if (MenuStack.Count > 1) {
                    MenuStack.Pop();
                    OnUpdate();
                    if (MenuLoaded != null) {
                        MenuLoaded(MenuStack.Peek());
                    }
                } else {
                    CloseMenu(MenuActionId.Resume);
                }   
            }
        }

        public void RestoreGraphicsDefaults() {
            var settings = _gameSettingsProvider.ActiveSettings;
            settings.Graphics = GameSettings.DefaultSettings.Value.Graphics;
            _gameSettingsUpdater.Update(settings);
        }

        public void RestoreGameplayDefaults() {
            var previousSettings = _gameSettingsProvider.ActiveSettings;
            var settings = previousSettings;
            settings.Gameplay = GameSettings.DefaultSettings.Value.Gameplay;
            settings.Gameplay.Time = previousSettings.Gameplay.Time;
            settings.Gameplay.Weather = previousSettings.Gameplay.Weather;
            _gameSettingsUpdater.Update(settings);
        }

        public void RestoreTimeAndWeatherDefaults() {
            var settings = _gameSettingsProvider.ActiveSettings;
            settings.Gameplay.Time = GameSettings.DefaultSettings.Value.Gameplay.Time;
            settings.Gameplay.Weather = GameSettings.DefaultSettings.Value.Gameplay.Weather;
            _gameSettingsUpdater.Update(settings);
        }

        public void RestoreAudioDefaults() {
            var settings = _gameSettingsProvider.ActiveSettings;
            settings.Audio = GameSettings.DefaultSettings.Value.Audio;
            _gameSettingsUpdater.Update(settings);
        }

        public void RestoreInputDefaults() {
            var defaultsType = _gameSettingsUpdater.UnappliedSettings.InputDefaults;
            _restoreInputToDefaults(defaultsType);
            var settings = _gameSettingsProvider.ActiveSettings;
            settings.Input = GameSettings.DefaultSettings.Value.Input;
            _gameSettingsUpdater.Update(settings);
        }

        public void RestoreOtherDefaults() {
            var settings = _gameSettingsProvider.ActiveSettings;
            settings.Other = GameSettings.DefaultSettings.Value.Other;
            _gameSettingsUpdater.Update(settings);
        }

        public void Update(GameSettings settings) {
            _gameSettingsUpdater.Update(settings);
        }

        public MenuId MenuState {
            get { return _menuState; }
            set {
                _menuState = value;
                OnUpdate();
            }
        }

        public void OpenMenu() {
            if (MenuLoaded != null) {
                MenuLoaded(MenuStack.Peek());
            }
        }

        public void CloseMenu(MenuActionId action) {
            if (OnMenuClosed != null) {
                OnMenuClosed(action);
            }
        }

        public LanguageTable LanguageTable {
            get { return _languageTable; }
            set {
                _languageTable = value; 
                OnUpdate();
            }
        }

        public Menu ActiveMenu {
            get {
                return MenuStack.Peek();
            }
        }

        public bool NeedsRestart {
            get { return _gameSettingsUpdater.NeedsRestart; }
        }

        public InputBindingViewModel[] InputBindings {
            get { return _inputBindings; }
            set {
                _inputBindings = value;
                OnUpdate();
            }
        }

        public InputBindingViewModel? Rebinding {
            get { return _rebinding; }
            set {
                _rebinding = value; 
                OnUpdate();
            }
        }

        private void OnUpdate(GameSettings _) {
            OnUpdate();
        }

        private void OnUpdate() {
            if (Updated != null) {
                Updated(this);
            }
        }

        public void StartRebind(InputBindingViewModel binding) {
            _startRebind(binding);
        }

        public void ApplyGraphicsSettings() {
            var unappliedSettings = _gameSettingsUpdater.UnappliedSettings;
            var currentSettings = _gameSettingsUpdater.CurrentSettings;
            if (unappliedSettings.ScreenResolution.HasValue && 
                unappliedSettings.ScreenResolution != currentSettings.Graphics.ScreenResolution) {
                currentSettings.Graphics.ScreenResolution = unappliedSettings.ScreenResolution.Value;
                _gameSettingsUpdater.Update(currentSettings);
            }
        }

        public bool IsApplyGraphicsSettingsRequired {
            get {
                var unappliedResolution = _gameSettingsUpdater.UnappliedSettings.ScreenResolution;
                var currentSettings = _gameSettingsProvider.ActiveSettings.Graphics;
                return unappliedResolution.HasValue &&
                       unappliedResolution != currentSettings.ScreenResolution;
            }
        }
    }

    public class GameSettingsUpdater {

        public event Action<UnappliedSettings> OnUnappliedSettingsUpdated;

        private readonly GameSettings _initialSettings;
        private UnappliedSettings _unappliedSettings;
        private bool _needsRestart;

        private readonly GameSettingsProvider _settingsProvider;

        public GameSettingsUpdater(GameSettingsProvider settingsProvider) {
            _settingsProvider = settingsProvider;
            _initialSettings = settingsProvider.ActiveSettings;
            _unappliedSettings = new UnappliedSettings();
            _needsRestart = false;
        }

        public void Update(GameSettings settings) {
            _needsRestart = _initialSettings.Graphics.DetailObjectDensity != settings.Graphics.DetailObjectDensity
                || _initialSettings.Graphics.DetailObjectDistance != settings.Graphics.DetailObjectDistance;
            _settingsProvider.UpdateGameSettings(settings);
        }

        public bool NeedsRestart {
            get { return _needsRestart; }
        }

        public Action<T> Updater<T>(Func<GameSettings, T, GameSettings> update) {
            return value => Update(update(_settingsProvider.ActiveSettings, value));
        }

        public GameSettings CurrentSettings {
            get { return _settingsProvider.ActiveSettings; }
        }

        public UnappliedSettings UnappliedSettings {
            get { return _unappliedSettings; }
            set {
                _unappliedSettings = value;
                if (OnUnappliedSettingsUpdated != null) {
                    OnUnappliedSettingsUpdated(_unappliedSettings);
                }
            }
        }
    }

    public struct UnappliedSettings {
        public Resolution? ScreenResolution;
        public InputDefaults InputDefaults;
    }

    public enum InputDefaults {
        KeyboardAndMouse, KeyboardOnly, Xbox360, XboxOne, SteamController, Playstation4, XInput
    }

    public static class InputDefaultsExtensions {
        public static ControllerType ToControllerType(this InputDefaults inputDefaults) {
            switch (inputDefaults) {
                case InputDefaults.KeyboardAndMouse:
                case InputDefaults.KeyboardOnly:
                    return ControllerType.Other; 
                case InputDefaults.XInput:
                case InputDefaults.Xbox360:
                    return ControllerType.Xbox360;
                case InputDefaults.XboxOne:
                    return ControllerType.XboxOne;
                case InputDefaults.SteamController:
                    return ControllerType.SteamController;
                case InputDefaults.Playstation4:
                    return ControllerType.Playstation4;
                default:
                    throw new ArgumentOutOfRangeException("inputDefaults");
            }
        }

        public static InputDefaults? ToInputDefaults(this ControllerType controllerType) {
            InputDefaults? inputDefaults;
            switch (controllerType) {
                case ControllerType.Xbox360:
                    inputDefaults = InputDefaults.Xbox360;
                    break;
                case ControllerType.XboxOne:
                    inputDefaults = InputDefaults.XboxOne;
                    break;
                case ControllerType.SteamController:
                    inputDefaults = InputDefaults.SteamController;
                    break;
                case ControllerType.Playstation4:
                    inputDefaults = InputDefaults.Playstation4;
                    break;
                case ControllerType.Other:
                    inputDefaults = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("controllerType");
            }
            return inputDefaults;
        }

        public static InputDefaults VerifyController(this InputDefaults inputDefaults, ControllerId controllerId) {
            if (controllerId is ControllerId.XInput && 
                inputDefaults != InputDefaults.KeyboardAndMouse && 
                inputDefaults != InputDefaults.KeyboardOnly) {
                return InputDefaults.XInput;
            }
            return inputDefaults;
        }
    }
}

