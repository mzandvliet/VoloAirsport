using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;

namespace RamjetAnvil.Volo.Ui {
    public class OtherSettingsViewModel {
        public readonly GuiComponentDescriptor.List Language;
        public readonly GuiComponentDescriptor.Boolean AutomaticServerPort;
        public readonly GuiComponentDescriptor.Range ServerPort;
        public readonly GuiComponentDescriptor.Boolean AutomaticClientPort;
        public readonly GuiComponentDescriptor.Range ClientPort;

        public OtherSettingsViewModel(GameSettingsUpdater settingsUpdater, Languages languages) {
            Func<GameSettings> s = () => settingsUpdater.CurrentSettings;

            var availableLanguages = languages.MetaInfo.Values.ToList();
            var cultureCodes = availableLanguages.Select(l => l.CultureCode.ToString()).ToList();

            Language = new GuiComponentDescriptor.List(
                "language",
                availableLanguages
                    .Select(languageInfo => languageInfo.EnglishName + " / " + languageInfo.Name)
                    .ToArray(),
                updateIndex: settingsUpdater.Updater<int>((settings, newIndex) => {
                    settings.Other.Language = cultureCodes[newIndex];
                    return settings; 
                }),
                currentIndex: () => cultureCodes.IndexOf(s().Other.Language));

            AutomaticServerPort = new GuiComponentDescriptor.Boolean(
                "automatic_server_port",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Other.AutomaticNetworkPort = value;
                    return settings; 
                }),
                isChecked: () => s().Other.AutomaticNetworkPort);
            ServerPort = new GuiComponentDescriptor.Range(
                "server_port",
                minValue: 1024f,
                maxValue: 65535f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Other.NetworkPort = (int) value;
                    return settings; 
                }),
                currentValue: () => s().Other.NetworkPort,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.IsVisible = !s().Other.AutomaticNetworkPort;
                    GuiComponentDescriptor.DisplayNumber(s().Other.NetworkPort, str, decimalPlaces: 0);
                });
        }
    }
}
