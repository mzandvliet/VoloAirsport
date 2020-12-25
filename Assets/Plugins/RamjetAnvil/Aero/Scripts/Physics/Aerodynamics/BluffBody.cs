using System;
using RamjetAnvil.DependencyInjection;
using UnityEngine;

/* Bluff bodies experience simple drag effects, irrespective of orientation */

public class BluffBody : MonoBehaviour, IAerodynamicSurface {
    [Dependency, SerializeField] private WindManager _wind;

    [SerializeField] private Rigidbody _body;
    [SerializeField] private float _cDrag = 1f;
    [SerializeField] private float _referenceArea = 1f;

    public float AngleOfAttack { get { return 0f; } }
    public Vector3 RelativeVelocity { get; private set; }
    public float AirSpeed { get; private set; }
    public Vector3 LiftForce { get; private set; } // Todo
    public Vector3 DragForce { get; private set; } // Todo
    public Vector3 MomentForce { get; private set; }

    public float Area {
        get { return _referenceArea; }
    }

    public Vector3 Center { get { return Vector3.zero; } }

    public event Action<IAerodynamicSurface> OnPreUpdate;
    public event Action<IAerodynamicSurface> OnPostUpdate;
    public event Action<IAerodynamicSurface> OnPreFixedUpdate;
    public event Action<IAerodynamicSurface> OnPostFixedUpdate;

    public void Clear() {}

    private void Update() {
        if (OnPreUpdate != null) {
            OnPreUpdate(this);
        }

        if (OnPostUpdate != null) {
            OnPostUpdate(this);
        }
    }

    private void FixedUpdate() {
        if (_wind == null) return;

        if (OnPreFixedUpdate != null) {
            OnPreFixedUpdate(this);
        }

        Vector3 centerPosition = transform.position;

        Vector3 windVelocity = _wind.GetWindVelocity(centerPosition);
        float airDensity = _wind.GetAirDensity(centerPosition);

        RelativeVelocity = _body.velocity - windVelocity;
        AirSpeed = RelativeVelocity.magnitude;

        Vector3 force = -RelativeVelocity.normalized * (0.5f * airDensity * _referenceArea * AirSpeed * AirSpeed * _cDrag); // Todo: optimize by not normalizing relative velocity

        _body.AddForceAtPosition(force, transform.position);

        if (OnPostFixedUpdate != null) {
            OnPostFixedUpdate(this);
        }
    }
}
