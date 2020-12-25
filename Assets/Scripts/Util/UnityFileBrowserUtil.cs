using System;
using System.Diagnostics;
using System.IO;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Util {
    public static class UnityFileBrowserUtil {
        public static readonly string HomePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                                  Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public static readonly Lazy<string> VoloAirsportDir = new Lazy<string>(() => {
            var os = PlatformUtil.CurrentOs();
            string path;
            if (os == PlatformUtil.OperatingSystem.MacOsx) {
                path = Path.Combine(UnityFileBrowserUtil.HomePath, "Documents/VoloAirsport/");
            } else if (os == PlatformUtil.OperatingSystem.Windows) {
                path = Path.Combine(UnityFileBrowserUtil.HomePath, "Volo Airsport/");
            } else if (os == PlatformUtil.OperatingSystem.Linux) {
                path = Path.Combine(UnityFileBrowserUtil.HomePath, ".volo_airsport/");
            } else {
                throw new Exception("Unsupported operating system: " + os);
            }
            return path;
        }); 

        public static void OpenInMac(string path) {
            bool openInsidesOfFolder = false;

            // try mac
            string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

            if (System.IO.Directory.Exists(macPath)) { // if path requested is a folder, automatically open insides of that folder
                openInsidesOfFolder = true;
            }

            if (!macPath.StartsWith("\"")) {
                macPath = "\"" + macPath;
            }

            if (!macPath.EndsWith("\"")) {
                macPath = macPath + "\"";
            }

            string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

            try {
                Process.Start("open", arguments);
            } catch (System.ComponentModel.Win32Exception e) {
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        public static void OpenInWin(string path) {
            bool openInsidesOfFolder = false;

            // try windows
            string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

            if (System.IO.Directory.Exists(winPath)) { // if path requested is a folder, automatically open insides of that folder
                openInsidesOfFolder = true;
            }

            try {
                Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
            } catch (System.ComponentModel.Win32Exception e) {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        public static void Open(string path) {
            var os = PlatformUtil.CurrentOs();
            if (os == PlatformUtil.OperatingSystem.Windows) {
                OpenInWin(path);
            } else if (os == PlatformUtil.OperatingSystem.MacOsx) {
                OpenInMac(path);
            } else {
                UnityEngine.Debug.LogWarning("Opening file browser not supported for OS: " + os);
            }
        }
    }
}
