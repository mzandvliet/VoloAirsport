using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using StringLeakTest;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public class TimeAndWeatherSettingsViewModel {
        
        public readonly GuiComponentDescriptor.Boolean IsTimeSimulated;
        public readonly GuiComponentDescriptor.Range CurrentTime;
        public readonly GuiComponentDescriptor.Range CurrentDate;
        public readonly GuiComponentDescriptor.Range DayLengthInMinutes;
        public readonly GuiComponentDescriptor.Boolean IsNightSkipped;

        public readonly GuiComponentDescriptor.Boolean IsWeatherSimulated;
        public readonly GuiComponentDescriptor.Range SnowAltitude;
        public readonly GuiComponentDescriptor.Range SnowfallIntensity;
        public readonly GuiComponentDescriptor.Range FogIntensity;
        public readonly GuiComponentDescriptor.Range DaysPerSeason;

        public TimeAndWeatherSettingsViewModel(GameSettingsUpdater settingsUpdater) {
            Func<GameSettings> s = () => settingsUpdater.CurrentSettings;
            Func<CultureInfo> currentCultureInfo = () => CultureInfoUtil.GetCulture(s().Other.Language);

            IsTimeSimulated = new GuiComponentDescriptor.Boolean(
                "simulate_time",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.Time.IsTimeSimulated = value;
                    return settings;
                }),
                isChecked: () => s().Gameplay.Time.IsTimeSimulated);
            CurrentTime = new GuiComponentDescriptor.Range(
                "current_time",
                minValue: 0f,
                maxValue: 23.999f,
                updateValue: settingsUpdater.Updater<float>((settings, hour) => {
                    var currentDateTime = settings.Gameplay.Time.CurrentDateTime;
                    settings.Gameplay.Time.CurrentDateTime = new DateTime(
                        currentDateTime.Year,
                        currentDateTime.Month,
                        currentDateTime.Day,
                        hour: 0,
                        minute: 0,
                        second: 0,
                        millisecond: 0,
                        kind: currentDateTime.Kind) + TimeSpan.FromHours(hour);
                    return settings; 
                }),
                currentValue: () => {
                    var currentDate = s().Gameplay.Time.CurrentDateTime;
                    return currentDate.Hour + (currentDate.Minute / 60f);
                },
                updateDisplayValue: (descritor, str) => {
                    var currentDateTime = s().Gameplay.Time.CurrentDateTime;
                    str.Append(currentDateTime.ToString("t", currentCultureInfo()));
                },
                stepSize: 0.16666667f);
            CurrentDate = new GuiComponentDescriptor.Range(
                "current_day",
                minValue: 1f,
                maxValue: 365f,
                updateValue: settingsUpdater.Updater<float>((settings, newDayOfYear) => {
                    var currentDateTime = settings.Gameplay.Time.CurrentDateTime;
                    settings.Gameplay.Time.CurrentDateTime = currentDateTime.SetDayOfYear((int) newDayOfYear);
                    return settings;
                }),
                currentValue: () => {
                    var currentDate = s().Gameplay.Time.CurrentDateTime;
                    return currentDate.DayOfYear;
                },
                updateDisplayValue: (newDayOfYear, str) => {
                    var currentDate = s().Gameplay.Time.CurrentDateTime;
                    str.Append(currentDate.ToString("m", currentCultureInfo()));
                });
            DayLengthInMinutes = new GuiComponentDescriptor.Range(
                "day_length_in_minutes",
                minValue: 1f,
                maxValue: 200f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.Time.DayLengthInMinutes = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.Time.DayLengthInMinutes,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(s().Gameplay.Time.DayLengthInMinutes, str, decimalPlaces: 0);
                });
            IsNightSkipped = new GuiComponentDescriptor.Boolean(
                "skip_night",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.Time.IsNightSkipped = value;
                    return settings;
                }),
                isChecked: () => s().Gameplay.Time.IsNightSkipped);
            IsWeatherSimulated = new GuiComponentDescriptor.Boolean(
                "simulate_weather",
                updateValue: settingsUpdater.Updater<bool>((settings, value) => {
                    settings.Gameplay.Weather.IsWeatherSimulated = value;
                    return settings;
                }),
                isChecked: () => s().Gameplay.Weather.IsWeatherSimulated);
            SnowAltitude = new GuiComponentDescriptor.Range(
                "snow_altitude",
                minValue: 2000f,
                maxValue: 3300,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.Weather.SnowAltitude = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.Weather.SnowAltitude,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.IsVisible = !s().Gameplay.Weather.IsWeatherSimulated;
                    GuiComponentDescriptor.DisplayNumber(s().Gameplay.Weather.SnowAltitude, str, decimalPlaces: 0, postFix: "m");
                },
                stepSize: 20f);
            SnowfallIntensity = new GuiComponentDescriptor.Range(
                "snowfall_intensity",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.Weather.SnowfallIntensity = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.Weather.SnowfallIntensity,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.IsVisible = !s().Gameplay.Weather.IsWeatherSimulated;
                    GuiComponentDescriptor.DisplayPercentage(s().Gameplay.Weather.SnowfallIntensity, str);
                },
                stepSize: 0.02f);
            FogIntensity = new GuiComponentDescriptor.Range(
                "fog_intensity",
                minValue: 0f,
                maxValue: 1f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.Weather.FogIntensity = value;
                    return settings; 
                }),
                currentValue: () => s().Gameplay.Weather.FogIntensity,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.IsVisible = !s().Gameplay.Weather.IsWeatherSimulated;
                    GuiComponentDescriptor.DisplayPercentage(s().Gameplay.Weather.FogIntensity, str);
                },
                stepSize: 0.02f);
            DaysPerSeason = new GuiComponentDescriptor.Range(
                "days_per_season",
                minValue: 1f,
                maxValue: 178f,
                updateValue: settingsUpdater.Updater<float>((settings, value) => {
                    settings.Gameplay.Weather.DaysPerSeason = value;
                    return settings;
                }),
                currentValue: () => s().Gameplay.Weather.DaysPerSeason,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.IsVisible = s().Gameplay.Weather.IsWeatherSimulated;
                    GuiComponentDescriptor.DisplayNumber(s().Gameplay.Weather.DaysPerSeason, str, decimalPlaces: 0);
                });


        }
    }
}
