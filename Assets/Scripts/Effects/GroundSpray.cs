using UnityEngine;
using System.Collections;
using RamjetAnvil.Unity.Utility;
using UnityExecutionOrder;

[Run.After(typeof(FlightStatistics))]
public class GroundSpray : MonoBehaviour {
    [SerializeField] private FlightStatistics _statistics;
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private float _velocityLeadFactor = 0.1f;
    [SerializeField] private float _maxHeight = 50f;
    [SerializeField] private float _intensityPow = 2f;
    [SerializeField] private float _maxSpeed = 75f;
    [SerializeField] private float _maxStartSpeed = 20f;
    [SerializeField] private float _maxEmissionRate = 100f;

    private int _layerMask;
    private Transform _transform;

    private void Awake() {
        _layerMask = LayerMaskUtil.FullMask
            .Remove("Player")
            .Remove("Head")
            .Remove("UI");
        _transform = gameObject.GetComponent<Transform>();

        _particles.startSpeed = 0f;
        _particles.SetEmissionRate(0f);
    }

    // Update is called once per frame
    void Update() {
        RaycastHit hitInfo;
        Vector3 direction = Vector3.Normalize(Vector3.down + _statistics.WorldVelocity * _velocityLeadFactor);
        if (Physics.Raycast(_transform.position, direction, out hitInfo, _maxHeight, _layerMask)) {
            float closeness = 1f - Mathf.Min(hitInfo.distance / _maxHeight, 1f);
            float speedness = Mathf.Min(_statistics.WorldVelocity.magnitude / _maxSpeed, 1f);
            float intensity = Mathf.Pow(closeness * speedness, _intensityPow);
            _particles.startSpeed = intensity * _maxStartSpeed;
            _particles.SetEmissionRate(intensity * _maxEmissionRate);
            _particles.transform.position = hitInfo.point;
            _particles.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
        }
        else {
            _particles.startSpeed = 0f;
            _particles.SetEmissionRate(0f);
        }
    }
}
