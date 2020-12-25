using System;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class Gate :MonoBehaviour {
        public event Action<FlightStatistics> OnGateTriggered;

        void OnTriggerEnter(Collider collider) {
            if (OnGateTriggered != null) {
                OnGateTriggered(collider.GetComponentInParent<FlightStatistics>());
            }
        }
    }
}
