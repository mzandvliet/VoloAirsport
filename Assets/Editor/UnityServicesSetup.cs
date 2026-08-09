using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace RamjetAnvil.UnityEditor {

    [InitializeOnLoad]
    public class UnityServicesSetup {
        static UnityServicesSetup() {
            // Analytics.SetUserId was removed from the Analytics API; no replacement wired up.
        }
    }
}
