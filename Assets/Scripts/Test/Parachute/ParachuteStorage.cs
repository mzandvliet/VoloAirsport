using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Reactive;
using RamjetAnvil.Volo;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Volo {
    
    public class ParachuteStorage {

        public static readonly Lazy<string> DefaultChutesDir = new Lazy<string>(() => {
            return Path.Combine(Application.streamingAssetsPath, "Parachutes");
        });
        public static readonly Lazy<string> StorageDir = new Lazy<string>(() => {
            return Path.Combine(
                Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "parachutes"),
                "v" + ParachuteConfig.VersionNumber);
        });

        private readonly ParachuteConfig _configPrefab;
        private readonly AirfoilDefinition _hardcodedAirfoilDefinition;
        private readonly string _path;
        private readonly bool _isEditable;

        public ParachuteStorage(string path, ParachuteConfig configPrefab, AirfoilDefinition hardcodedAirfoilDefinition, bool isEditable) {
            Directory.CreateDirectory(path);
    
            _path = path;
            _configPrefab = configPrefab;
            _hardcodedAirfoilDefinition = hardcodedAirfoilDefinition;
            _isEditable = isEditable;
        }

        public IList<ParachuteConfig> StoredChutes {
            get
            {
                return Directory.GetFiles(_path, "*.json", SearchOption.TopDirectoryOnly)
                    .Select(file => {
                            var config = Object.Instantiate(_configPrefab);
                            if (File.Exists(file)) {
                                JsonUtility.FromJsonOverwrite(File.ReadAllText(file), config);
                            }
                            config.AirfoilDefinition = _hardcodedAirfoilDefinition;
                            config.IsEditable = _isEditable;
                            return config;
                    })
                    .ToList();
            }
        }

        public static ParachuteConfig SelectParachute(IList<ParachuteConfig> parachutes, string parachuteId) {
            for (int i = 0; i < parachutes.Count; i++) {
                var parachuteConfig = parachutes[i];
                if (parachuteConfig.Id == parachuteId) {
                    return parachuteConfig;
                }
            }
            return parachutes[0];
        }
    //
    //    public IObservable<IList<ParachuteConfig>> ParachuteChanges() {
    //        return FileWatching.TrackDirectory(FileWatching.WatcherSettings.Create(_path, "*.json"))
    //            .Select(files => {
    //                return files.Select(file => {
    //                        var config = Object.Instantiate(_configPrefab);
    //                        if (File.Exists(file.FullPath)) {
    //                            JsonUtility.FromJsonOverwrite(File.ReadAllText(file.FullPath), config);
    //                        }
    //                        config.AirfoilDefinition = _hardcodedAirfoilDefinition;
    //                        return config;
    //                    })
    //                .ToList();
    //            });
    //    }

        public void DeleteAllStoredParachutes() {
            var storedParachuteFiles = Directory.GetFiles(_path, "*.json", SearchOption.TopDirectoryOnly);
            foreach (var parachuteFile in storedParachuteFiles) {
                File.Delete(parachuteFile);
            }
        }

        public void StoreParachute(ParachuteConfig config, string json) {
            var filePath = Path.Combine(_path, config.Name + "-" + config.Id + ".json");
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
    }
}
