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
        var versionRequest = new WWW("http://volo-airsport.com/version-info.json");
        yield return versionRequest;

        if (!string.IsNullOrEmpty(versionRequest.error)) {
            const string errorMessage = "Failed to check for updates, is your internet connection okay?";
            _notificationList.AddTimedNotification(errorMessage, _notificationTimeoutInS.Seconds());
            Debug.LogError("Update check failed, reason: " + versionRequest.error);
            yield break;
        }
        
        var serverVersion = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.Default.GetString(versionRequest.bytes));
        var latestVersion = serverVersion["version"];
        //var downloadUrl = serverVersion["downloadUrl"];

        VersionInfo localVersion = Resources.Load<VersionInfo>("versionInfo");

        var isNewVersionAvailable = StringComparer.InvariantCulture.Compare(localVersion.VersionNumber, latestVersion) < 0;
        if (isNewVersionAvailable) {
            var versionStr = "V" + latestVersion;
            var updateMessage = versionStr + " of Volo Airsport is out! Check it at: volo-airsport.com";
            _notificationList.AddTimedNotification(updateMessage, _notificationTimeoutInS.Seconds());
        }
    }
}
