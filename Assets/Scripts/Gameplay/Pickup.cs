using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo.Util;
using UnityEngine;

public class Pickup : MonoBehaviour {
    [SerializeField, Dependency] private ParticleSystemPool _particleSystemPool;
    [SerializeField, Dependency] private UnityCoroutineScheduler _coroutineScheduler;

    private Transform _transform;

    void Awake() {
        _transform = GetComponent<Transform>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            var flightStats = other.gameObject.GetComponentInParent<FlightStatistics>();
            _coroutineScheduler.Run(EmitParticles(flightStats));
            // Todo: Play a sound
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    IEnumerator<WaitCommand> EmitParticles(FlightStatistics player) {
        using (var ps = _particleSystemPool.Take()) {
            var velocity = player.WorldVelocity * 2f;
            Debug.Log("player velocty " + player.WorldVelocity);
            var psVelocity = ps.Instance.velocityOverLifetime;
            psVelocity.xMultiplier = velocity.x;
            psVelocity.yMultiplier = velocity.y;
            psVelocity.zMultiplier = velocity.z;
            ps.Instance.transform.position = _transform.position;
            ps.Instance.Play();
            // Wait until the last particle is emitted before disposing it
            yield return WaitCommand.WaitSeconds(
                ps.Instance.main.duration + 
                ps.Instance.main.startLifetime.constant);
        }
    }
}
