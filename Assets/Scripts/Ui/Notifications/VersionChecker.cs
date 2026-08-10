using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.Volo;
using UnityEngine;

public class VersionChecker : MonoBehaviour
{
    [SerializeField] private NotificationList _notificationList;
    [SerializeField] private float _notificationTimeoutInS = 10f;

    public void CheckVersion() {
        StartCoroutine(CheckVersionInternal());
    }

    IEnumerator CheckVersionInternal() {
        var versionRequest = new WWW("https://volo-airsport.com/version-info.json");
        yield return versionRequest;

        if (!string.IsNullOrEmpty(versionRequest.error)) {
            const string errorMessage = "Failed to check for updates, is your internet connection okay?";
            _notificationList.AddTimedNotification(errorMessage, _notificationTimeoutInS.Seconds());
            Debug.LogError("Update check failed, reason: " + versionRequest.error);
            yield break;
        }
        
        // The version endpoint is unreachable/decommissioned - a 200 OK with a parked-domain
        // HTML body (not JSON) is expected here, same category as the master server and news
        // feed. Fail quietly rather than crash the calling coroutine.
        string latestVersion;
        try {
            var serverVersion = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.Default.GetString(versionRequest.bytes));
            latestVersion = serverVersion["version"];
        } catch (Exception e) {
            Debug.LogWarning("Version check response wasn't valid JSON, skipping: " + e.Message);
            yield break;
        }

        VersionInfo localVersion = Resources.Load<VersionInfo>("versionInfo");

        var isNewVersionAvailable = StringComparer.InvariantCulture.Compare(localVersion.VersionNumber, latestVersion) < 0;
        if (isNewVersionAvailable) {
            var versionStr = "V" + latestVersion;
            var updateMessage = versionStr + " of Volo Airsport is out! Check it at: volo-airsport.com";
            _notificationList.AddTimedNotification(updateMessage, _notificationTimeoutInS.Seconds());
        }
    }
}
