using UnityEngine;
using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Volo;
using Random = System.Random;

// Todo: n * log(n) complexity
// Todo: each boid respects a small random amount of other boids, not the whole group
// Todo: separate boids update and render updates
// Todo: introduce context-sensitive variance to randomness and cohesion parameters (like curiousity vs. danger)
// Todo: when player gets closer to center of the flock, start flocking with player

public class Boids : MonoBehaviour {
    private struct BoidSim {
        public Vector3 Position;
        public Vector3 TargetVelocity;
        public Vector3 SmoothVelocity;
    }

    [SerializeField, Dependency] private AbstractUnityEventSystem _eventSystem;
    [SerializeField, Dependency] private AbstractUnityClock _clock;

    [SerializeField] private GameObject _boidPrefab;
    [SerializeField] private int _numBoids = 32;
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _randomness = 1f;
    [SerializeField] private float _velocityCohesion = 1f;
    [SerializeField] private float _positionCohesion = 1f;

    private Boid[] _boidModels;
    private BoidSim[] _boidSims;

    private Vector3 _averagePosition;
    private int _layerMask;
    private FlightStatistics _player;

    public Vector3 AveragePosition {
        get { return _averagePosition; }
    }

    private void Awake() {
        _layerMask = ~LayerMask.GetMask("Boid");

        _boidSims = new BoidSim[_numBoids];
        _boidModels = new Boid[_numBoids];

        Vector3 position = transform.position + Vector3.up;
        Quaternion rotation = transform.rotation;

        for (int i = 0; i < _numBoids; i++) {
            _boidSims[i] = new BoidSim() {
                Position = position,
                TargetVelocity = new Vector3(0f, 0f, _maxSpeed * 0.5f)
            };

            var boid = (GameObject)Instantiate(_boidPrefab, position, rotation);
            _boidModels[i] = boid.GetComponent<Boid>();
        }
    }

    void OnEnable() {
        _eventSystem.Listen<Events.PlayerSpawned>(OnPlayerRespawned);
    }

    private void OnPlayerRespawned(Events.PlayerSpawned evt) {
        _player = evt.Player.FlightStatistics;
    }

    private void Update() {
        SteerSims();
        MoveSims();
        UpdateModels();

        _averagePosition = GetAveragePosition(_boidSims);
    }

    private int _subsetIndex;

    private void SteerSims() {
        // Todo: make time invariant

        int subsetSize = 16;
        int numSubsets = _numBoids / subsetSize;
        int subsetStartIndex = subsetSize * _subsetIndex;
        int subsetEndIndex = Mathf.Min(subsetStartIndex + subsetSize, _numBoids);

        Vector3 averageVelocity = GetAverageVelocity(_boidSims);
        Vector3 averagePosition = GetAveragePosition(_boidSims);

        Vector3 playerPosition = Vector3.zero;
        //Vector3 playerVelocity = Vector3.zero;

        if (_player) {
            playerPosition = _player.transform.position;
            //playerVelocity = _player.FlightStatistics.WorldVelocity;
        }

        for (int i = subsetStartIndex; i < subsetEndIndex; i++) { 
            // Steer in a random direction
            Vector3 randomVelocity = RandomVelocity() * _maxSpeed;
            _boidSims[i].TargetVelocity = Vector3.Slerp(_boidSims[i].TargetVelocity, randomVelocity, _randomness * numSubsets);

            // Steer back towards home
            Vector3 originVelocity = transform.position - _boidSims[i].Position;
            float originDist = originVelocity.magnitude;
            originVelocity /= originDist;
            float originDesire = Mathf.Min(originDist * 0.001f, 1f);
            originDesire = originDesire * originDesire * originDesire;
            _boidSims[i].TargetVelocity = Vector3.Slerp(_boidSims[i].TargetVelocity, originVelocity, originDesire * numSubsets);
            
            // Steer towards average velocity of the group
            _boidSims[i].TargetVelocity = Vector3.Slerp(_boidSims[i].TargetVelocity, averageVelocity, _velocityCohesion * numSubsets);
            
            Vector3 delta = averagePosition - _boidSims[i].Position;
            float dist = delta.magnitude;
            delta /= dist;
            delta *= Mathf.Pow(dist / 100f, 3f) * _maxSpeed;
            _boidSims[i].TargetVelocity = Vector3.Slerp(_boidSims[i].TargetVelocity, delta, _positionCohesion * numSubsets);

            // Steer away from obstacles
            const float visionLength = 50f;
            RaycastHit hitInfo;
            if (Physics.Raycast(_boidSims[i].Position, _boidSims[i].TargetVelocity, out hitInfo, visionLength, _layerMask)) {
                Vector3 avoidanceVelocity = hitInfo.normal * _maxSpeed;
                float priority = 1f - (hitInfo.distance / visionLength);
                _boidSims[i].TargetVelocity = Vector3.Slerp(_boidSims[i].TargetVelocity, avoidanceVelocity, priority * numSubsets);
            }

            if (_player) {
                Vector3 playerDelta = playerPosition - averagePosition;
                float playerDistance = playerDelta.magnitude;
                float playerProximity = 1f - Mathf.Min(playerDistance / 1000f, 1f);
                playerDelta = playerDelta / playerDistance;

                _boidSims[i].TargetVelocity = Vector3.Slerp(_boidSims[i].TargetVelocity, playerDelta * _maxSpeed, playerProximity);
            }
        }

        _subsetIndex = (_subsetIndex + 1) % numSubsets;

        // Todo: separation?
    }

    private void MoveSims() {
        float delta = 3f * Time.deltaTime;

        for (int i = 0; i < _boidSims.Length; i++) {
            _boidSims[i].SmoothVelocity = Vector3.Lerp(_boidSims[i].SmoothVelocity, _boidSims[i].TargetVelocity, delta);
            _boidSims[i].Position += _boidSims[i].SmoothVelocity * delta;
        }
    }

    private void UpdateModels() {
        for (int i = 0; i < _boidModels.Length; i++) {
            var boid = _boidModels[i];

            boid.Transform.position = _boidSims[i].Position;
            boid.Transform.rotation = Quaternion.LookRotation(_boidSims[i].SmoothVelocity, Vector3.up);

            boid.Animation["flap"].speed = _boidSims[i].SmoothVelocity.magnitude * 0.25f;
            Debug.DrawRay(boid.Transform.position, _boidSims[i].SmoothVelocity, Color.green);
        }
    }

    private static Vector3 GetAverageVelocity(BoidSim[] boids, Random random) {
        Vector3 velocity = Vector3.zero;

        const int range = 7;
        int startIndex = random.Next(0, boids.Length - range - 1);

        for (int i = startIndex; i < startIndex + range; i++) {
            velocity += boids[i].TargetVelocity;
        }
        return velocity / range;
    }

    private static Vector3 GetAveragePosition(BoidSim[] boids, Random random) {
        Vector3 velocity = Vector3.zero;

        const int range = 7;
        int startIndex = random.Next(0, boids.Length - range - 1);

        for (int i = startIndex; i < startIndex + range; i++) {
            velocity += boids[i].SmoothVelocity;
        }
        return velocity / range;
    }

    private static Vector3 GetAverageVelocity(IList<BoidSim> boids) {
        Vector3 position = Vector3.zero;
        for (int i = 0; i < boids.Count; i++) {
            position += boids[i].SmoothVelocity;
        }
        return position / (float)boids.Count;
    }

    private static Vector3 GetAveragePosition(IList<BoidSim> boids) {
        Vector3 position = Vector3.zero;
        for (int i = 0; i < boids.Count; i++) {
            position += boids[i].Position;
        }
        return position / (float)boids.Count;
    }

    private static Vector3 RandomVelocity() {
        return UnityEngine.Random.insideUnitCircle;
    }
}
