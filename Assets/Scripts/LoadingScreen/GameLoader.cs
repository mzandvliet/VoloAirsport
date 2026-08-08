using System;
using System.Collections;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using UnityEngine;

public class GameLoader : MonoBehaviour {
    [Dependency, SerializeField] private UnityCoroutineScheduler _scheduler;

    private void Awake() {

        // VR is not supported in this build; VoloModule.DetermineVrMode() always resolves to VrMode.None.
        var vrMode = VoloModule.DetermineVrMode();

        if (Application.runInBackground != true) {
            Application.runInBackground = true;
        }

        LogVersion();
        LogSystemInfo();
    }

    void Start() {
        _scheduler.Run(Load());
    }

    IEnumerator<WaitCommand> Load() {
        yield return WaitCommand.WaitSeconds(0.5f);
        _scheduler.Run(LoadAndCleanUp());
    }

    IEnumerator<WaitCommand> LoadAndCleanUp() {
        var upgradeScripts = new IUpgradeScript[] {new UpgradeCoursesV1ToV2()};
        for (int i = 0; i < upgradeScripts.Length; i++) {
            var upgradeScript = upgradeScripts[i];
            upgradeScript.Upgrade();
        }

        yield return WaitCommand.WaitForNextFrame;

        var module = new VoloModule();
        yield return module.Load().AsWaitCommand();
        gameObject.SetActive(false);
        yield return module.Run().AsWaitCommand();
    }

    private static void LogVersion() {
        VersionInfo localVersion = Resources.Load<VersionInfo>("versionInfo");
        Debug.Log("Volo Airsport version " + localVersion.VersionNumber);
    }

    private static void LogSystemInfo() {
        string systemInfo = "System Info...\n";
        systemInfo += "OS: " + SystemInfo.operatingSystem + "\n";
        systemInfo += "CPU: " + SystemInfo.processorType + ", Logical cores: " + SystemInfo.processorCount + "\n"; //  + ", " + SystemInfo.processorFrequency + "MHz\n";
        systemInfo += "RAM: " + SystemInfo.systemMemorySize + "MB\n";
        systemInfo += "GPU: " + SystemInfo.graphicsDeviceName + ", VRAM: " + SystemInfo.graphicsMemorySize + "MB, ShaderModel: " +
                      SystemInfo.graphicsShaderLevel + ", Driver: " + SystemInfo.graphicsDeviceVersion + ", Threaded: " +
                      SystemInfo.graphicsMultiThreaded + "\n";
        systemInfo += "Reverse-Z Support: " + (SystemInfo.usesReversedZBuffer ? "Yes" : "No") + "\n";

        systemInfo += "VR: not supported in this build\n";
        Debug.Log(systemInfo);
        if (!SystemInfo.usesReversedZBuffer) {
            Debug.LogError("Reverse-Z not supported");
        }
    }
}
