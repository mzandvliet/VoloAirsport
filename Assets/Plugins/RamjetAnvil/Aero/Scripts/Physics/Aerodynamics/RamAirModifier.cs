using RamjetAnvil.Unity.Utility;
using UnityEngine;

/* Ok, so this is where the wingman-pull-to-the-right bug really came from. */

public class RamAirModifier : MonoBehaviour {
    [SerializeField] private Airfoil1D _airfoil;
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField] private float _openDistance = 1f;
    [SerializeField] private AnimationCurve _efficiencyEnvelope = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public float NormalizedDistance { get; private set; }
    public float Efficiency { get; private set; }

    private float GetNormalizedDistance() {
        return Mathf.Clamp01((_pointA.position - _pointB.position).magnitude / _openDistance);
    }

    private float GetEfficiency(float normalizedDistance) {
        return _efficiencyEnvelope.Evaluate(normalizedDistance);
    }
    
    private void FixedUpdate() {
        NormalizedDistance = GetNormalizedDistance();
        Efficiency = GetEfficiency(NormalizedDistance);
        _airfoil.Efficiency = Efficiency;

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.Lerp(Color.black, Color.cyan, Efficiency);
        Gizmos.DrawLine(_pointA.position, _pointB.position);
    }
}
