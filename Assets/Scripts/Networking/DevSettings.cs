using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo.Networking {
    public class DevSettings {
        private readonly string _adminUsername;
        private readonly string _adminPassword;

        public DevSettings(string adminUsername, string adminPassword) {
            _adminUsername = adminUsername;
            _adminPassword = adminPassword;
            _adminUsername = adminUsername;
        }

        public string AdminUsername {
            get { return _adminUsername; }
        }

        public string AdminPassword {
            get { return _adminPassword; }
        }
    }

    public static class DevSettingsSerialization {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        public static Maybe<DevSettings> Deserialize() {
            var devSettingsPath = Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "DevSettings.json");
            if (File.Exists(devSettingsPath)) {
                using (var fileStream = new FileStream(devSettingsPath, FileMode.Open))
                using (var reader = new StreamReader(fileStream)) 
                using (var jsonReader = new JsonTextReader(reader)) {
                    return Maybe.Just(JsonSerializer.Deserialize<DevSettings>(jsonReader));    
                }
            }
            return Maybe.Nothing<DevSettings>();
        }
    }
}
