using System.Collections;
using FMODUnity;
using RamjetAnvil.Volo;
using UnityEngine;
using Random = UnityEngine.Random;

// Being hit from behind is not fun
// Try slow bullets, lots of them?
// Try slow guiding missiles 

public class Turret : MonoBehaviour {
    [SerializeField]
    private float _detectionRange = 300f;
    [SerializeField]
    private float _shotInterval = 2f;
    [SerializeField]
    private float _leadFactor = 500f;
    [SerializeField]
    private float _shotScattering = 0.02f;
    [SerializeField]
    private float _force = 500f;

    [SerializeField] private Transform _barrel;
    [SerializeField] private StudioEventEmitter _shotSound;
    [SerializeField] private StudioEventEmitter _detectSound;
    [SerializeField] private StudioEventEmitter _idleSound;
    [SerializeField] private Light _light;

    private Transform _transform;
    private AbstractUnityEventSystem _eventSystem;
    private Coroutine _firingRoutine;

    private FlightStatistics _target;
    private bool _attacking;

    private BulletManager _bulletManager;
    
    public void Initialize(AbstractUnityEventSystem evts) {
        _transform = gameObject.GetComponent<Transform>();
        _bulletManager = FindObjectOfType<BulletManager>(); // Todo: from module code?

        _eventSystem = evts;
        _eventSystem.Listen<Events.PlayerSpawned>(OnPlayerRespawned);

        Idle();
    }

    private void OnPlayerRespawned(Events.PlayerSpawned evt) {
        _target = evt.Player.FlightStatistics;
    }

    private void Update() {
        if (!_target) {
            return;
        }

        // Todo: Stagger updates for performance. Assume many turrets.

        bool inView = false;

        float dot = Vector3.Dot(_transform.forward, _target.transform.position - _transform.position);
        if (dot > 0f) {
            inView = true;
        }

        bool clearShot = false;
        Ray ray = new Ray();
        ray.origin = _barrel.transform.position + _barrel.transform.forward * 3f;
        ray.direction = Vector3.Normalize(_target.transform.position - ray.origin);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, _detectionRange, int.MaxValue)) {
            if (hitInfo.collider.CompareTag("Player")) {
                clearShot = true;
            }
        }
        
        if (clearShot && inView) {
            if (!_attacking) {
                Attack();
            }
        }
        else {
            if (_attacking) {
                Idle();   
            }
        }

        if (_attacking) {
            float distance = Vector3.Distance(_barrel.position, _target.transform.position);
            _barrel.LookAt(_target.GetInterpolatedTrajectory(distance / _leadFactor).Position);
        }
    }

    private void Attack() {
        if (_attacking) {
            return; // No need to attack when already attacking
        }

        Debug.Log("Attacking Target");

        _attacking = true;
        _firingRoutine = StartCoroutine(ShootRepeadely());

        _light.enabled = true;
        _detectSound.Play();
    }

    private void Idle() {
        if (!_attacking) {
            return; // No need to deactivate when already inactive
        }

        Debug.Log("Idling...");

        _attacking = false;
        if (_firingRoutine != null) {
            StopCoroutine(_firingRoutine);
        }

        _light.enabled = false;
        _idleSound.Play();
    }

    private IEnumerator ShootRepeadely() {
        yield return new WaitForSeconds(1f);
        while (true) {
            yield return new WaitForSeconds(_shotInterval + Random.value * _shotInterval);

            if (_target) {
                Fire();
            }
        }
    }

    private void Fire() {
        _bulletManager.Fire(
            _barrel.position + _barrel.forward * 12f,
            _barrel.rotation,
            (_barrel.forward + Random.insideUnitSphere * _shotScattering) * _force
            );

        _shotSound.Play();
    }
}