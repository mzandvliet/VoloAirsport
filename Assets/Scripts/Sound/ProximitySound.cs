using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityExecutionOrder;

[Run.After(typeof(FlightStatistics))]
public class ProximitySound : MonoBehaviour, ISpawnable {
    [SerializeField]
    private FlightStatistics _statistics;

    public void OnSpawn() {
    }

    public void OnDespawn() {
    }
}