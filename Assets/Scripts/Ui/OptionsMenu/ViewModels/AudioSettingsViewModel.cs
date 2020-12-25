using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public class AudioSettingsViewModel {

        public readonly GuiComponentDescriptor.Boolean Mute;
        public readonly GuiComponentDescriptor.Range SoundEffectVolume;
        public readonly GuiComponentDescriptor.Range MusicVolume;

        public AudioSettingsViewModel(GameSettingsUpdater settingsUpdater) {
            Func<GameSettings> s = () => settingsUpdater.CurrentSettings;
            SoundEffectVolume = new GuiComponentDescriptor.Range(
                "sound_effect_volume",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    if (!settings.Audio.IsMuted) {
                        settings.Audio.SoundEffectsVolume = value;    
                    }
                    return settings;
                }),
                currentValue: () => s().Audio.SoundEffectsVolume,
                updateDisplayValue: (descriptor, str) => {
                    if (s().Audio.IsMuted) {
                        descriptor.IsEnabled = false;
                        str.Append("─"); // Utf-8 char: &#x2500;
                    } else {
                        descriptor.IsEnabled = true;
                        GuiComponentDescriptor.DisplayPercentage(s().Audio.SoundEffectsVolume, str);
                    }
                },
                stepSize: 0.02f);
            MusicVolume = new GuiComponentDescriptor.Range(
                "music_volume",
                minValue: 0f,
                maxValue: 0.7f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    if (!settings.Audio.IsMuted) {
                        settings.Audio.MusicVolume = value;    
                    }
                    return settings;
                }),
                currentValue: () => s().Audio.MusicVolume,
                stepSize: 0.02f,
                updateDisplayValue: (descriptor, str) => {
                    if (s().Audio.IsMuted) {
                        descriptor.IsEnabled = false;
                        str.Append("─"); // Utf-8 char: &#x2500;
                    } else {
                        descriptor.IsEnabled = true;
                        var musicVolumePercentage = Mathf.InverseLerp(descriptor.MinValue, descriptor.MaxValue,
                            s().Audio.MusicVolume);
                        GuiComponentDescriptor.DisplayPercentage(musicVolumePercentage, str);
                    }
                });
            Mute = new GuiComponentDescriptor.Boolean(
                "mute",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Audio.IsMuted = value;
                    return settings;
                }),
                isChecked: () => s().Audio.IsMuted);
        }
    }
}
