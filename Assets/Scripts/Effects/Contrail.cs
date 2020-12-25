using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using UnityExecutionOrder;

[RequireComponent(typeof(AdaptiveTrailRenderer))]
[Run.Before(typeof(AdaptiveTrailRenderer))]
public class Contrail : MonoBehaviour, ISpawnable {
    [SerializeField] private Airfoil1D _wing;
    [SerializeField] private float _widthMultiplier = 2f;
    [SerializeField] private float _maxAoA = 20f;
    [SerializeField] private float _maxSpeed = 75f;

    private AdaptiveTrailRenderer _renderer;
    private float _baseWidth;

    private void Awake() {
        _renderer = GetComponent<AdaptiveTrailRenderer>();
    }

    private void Start() {
        _baseWidth = _renderer.WidthMultiplier;
        _renderer.Emit();
    }

    public void OnSpawn() {
        _renderer.Reset();
    }

    public void OnDespawn() {}

    private void Update() {
        float intensity = Mathf.Pow(_wing.AirSpeed / _maxSpeed, 2f) * Mathf.Abs(_wing.AngleOfAttack) / _maxAoA;
        _renderer.Opacity = intensity;
        _renderer.WidthMultiplier = _baseWidth + _baseWidth * intensity * _widthMultiplier;
    }
}
