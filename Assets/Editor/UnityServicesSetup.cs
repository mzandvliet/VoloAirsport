using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace RamjetAnvil.UnityEditor {

    [InitializeOnLoad]
    public class UnityServicesSetup {
        static UnityServicesSetup() {
            var userId = Environment.UserName + " (developer)";
            Analytics.SetUserId(userId);
        }
    }
}
