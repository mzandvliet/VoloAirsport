using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;

namespace RamjetAnvil.Volo {
    public static class VoloAirsportFileStorage {

        public static readonly Lazy<string> StorageDir = new Lazy<string>(() => {
            var os = PlatformUtil.CurrentOs();
            string path;
            if (os == PlatformUtil.OperatingSystem.MacOsx) {
                path = Path.Combine(UnityFileBrowserUtil.HomePath, "Documents/VoloAirsport");
            } else if (os == PlatformUtil.OperatingSystem.Windows) {
                path = Path.Combine(UnityFileBrowserUtil.HomePath, "Volo Airsport");
            } else if (os == PlatformUtil.OperatingSystem.Linux) {
                path = Path.Combine(UnityFileBrowserUtil.HomePath, ".volo_airsport");
            } else {
                throw new Exception("Unsupported operating system: " + os);
            }

            return Path.GetFullPath(path);
        });
    }
}
