/**
 * Created by Martijn Zandvliet, 2014
 * 
 * Use of the source code in this document is governed by the terms
 * and conditions described in the Ramjet Anvil End-User License Agreement.
 * 
 * A copy of the Ramjet Anvil EULA should have been provided in the purchase
 * of the license to this source code. If you do not have a copy, please
 * contact me.
 * 
 * For any inquiries, contact: martijn@ramjetanvil.com
 */

using System;
using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

// Todo: Rename to something that doesn't sound like a value type
public class FlightStatistics : MonoBehaviour, ISpawnable {
    [Dependency, SerializeField] private WindManager _wind;

    [SerializeField]
    private Rigidbody _body;
    [SerializeField]
    private Vector3 _pitchAxis = Vector3.right; // Todo: Make this a pattern, it's common
    [SerializeField]
    private Vector3 _rollAxis = Vector3.forward; // *same
    [SerializeField]
    private float _trajectorySimTime = 5f;
    [SerializeField]
    private int _trajectoryRaycastSteps = 16;
    [SerializeField]
    private int _trajectorySegments = 64;

    /* The direction the player is flying in. */
    private Vector3 _relativeVelocity;

    /* The angle from the player to the relative wind. */
    private float _angleOfAttack;

    /* Magnitude of player's velocity in the relative wind. */
    private float _trueAirspeed;

    /* Y distance to sea level, which is just 0 for now. */
    private float _altitudeSeaLevel;

    /* Y distance to ground, locally. */
    private float _altitudeGround;

    /* Position derivatives */
    private Vector3 _acceleration;

    /* Angular Velocity, local. */
    private Vector3 _localAngularVelocity;
    private Vector3 _localAngularAcceleration;

    /* Glide Ratio. */
    private float _glideRatio;

    /* Angles to ground. */
    private float _angleToGroundRoll;
    private float _angleToGroundPitch;

    private Vector3 _prevVelocity;
    private Vector3 _prevAcceleration;
    private int _layerMask;
    private bool _collisionPredicted;
    private Vector3 _collisionPoint;
    private Vector3 _collisionNormal;
    private float _normalizedCollisionIndex;

    private RigidbodyState[] _predictionBuffer;
    private ICircularBuffer<RigidbodyState> _historyBuffer;

    public Vector3 RelativeVelocity { get { return _relativeVelocity; } }
    public Vector3 WorldVelocity { get { return _body.velocity; } } 
    public float AngleOfAttack { get { return _angleOfAttack; } }
    public float TrueAirspeed { get { return _trueAirspeed; } }
    public float AltitudeSeaLevel { get { return _altitudeSeaLevel; } }
    public float AltitudeGround { get { return _altitudeGround; } }
    public Vector3 Acceleration { get { return _acceleration; } }
    public Vector3 LocalAngularVelocity { get { return _localAngularVelocity; } }
    public Vector3 LocalAngularAcceleration { get { return _localAngularAcceleration; } }
    public float GlideRatio { get { return _glideRatio; } }
    public float AngleToGroundRoll { get { return _angleToGroundRoll; } }
    public float AngleToGroundPitch { get { return _angleToGroundPitch; } }

    public Vector3 PitchAxis {
        get { return _pitchAxis; }
    }

    public Vector3 RollAxis {
        get { return _rollAxis; }
    }

    public float TrajectorySimTime {
        get { return _trajectorySimTime; }
    }

    public int TrajectorySegments {
        get { return _trajectorySegments; }
    }

    public RigidbodyState[] PredictionBuffer {
        get {
            if (_predictionBuffer == null) {
                InitBuffers();
            }
            return _predictionBuffer;
        }
    }

    public float NormalizedCollisionIndex {
        get { return _normalizedCollisionIndex; }
    }

    public Vector3 CollisionPoint {
        get { return _collisionPoint; }
    }

    public Vector3 CollisionNormal {
        get { return _collisionNormal; }
    }

    public bool CollisionPredicted {
        get { return _collisionPredicted; }
    }

    public RigidbodyState GetInterpolatedTrajectory(float time) {
        if (_predictionBuffer == null) {
            InitBuffers();
        }

        float normalizedTime = Mathf.Clamp01(time / _trajectorySimTime);
        int index = Mathf.FloorToInt(normalizedTime * (_predictionBuffer.Length-1));
        //Debug.Log("velocity index " + index + " buffer size " + _predictionBuffer.Length + " requesting time " + time + " for total duration " + _trajectorySimTime + " in frame " + Time.frameCount);

        //float frac = (normalizedTime * _trajectoryBuffer.Length) % 1f;
        return PredictionBuffer[index];
        //return RigidbodyState.Lerp(_trajectoryBuffer[index], _trajectoryBuffer[Mathf.Clamp(index + 1, 0, _trajectoryBuffer.Length-1)], frac);
    }

    private void Awake() {
        _layerMask = LayerMaskUtil.CreateLayerMask("Player", "Head", "UI", "Interactive").Invert();
        if (_predictionBuffer == null || _historyBuffer == null) {
            InitBuffers();
        }
    }

    private void InitBuffers() {
        _predictionBuffer = new RigidbodyState[_trajectorySegments];
        _historyBuffer = new CircularBuffer<RigidbodyState>(60);

        ClearBuffers();
    }

    private void ClearBuffers() {
        for (int i = 0; i < _predictionBuffer.Length; i++) {
            _predictionBuffer[i] = new RigidbodyState();
        }

        _historyBuffer.Clear();
        _historyBuffer.Enqueue(RigidbodyState.ToImmutable(_body, Time.fixedTime));
    }

    public void OnSpawn() {
        _prevVelocity = Vector3.zero;
        _prevAcceleration = Vector3.zero;

        ClearBuffers();
    }

    public void OnDespawn() {
    }


    public void FixedUpdate() {
        if (_body == null || _wind == null) {
            return;
        }

        UpdateStatistics();
    }

    private void UpdateStatistics() {
        /* Note: Calculating position derivatives in Update caused framerate dependent effects to happen
         * (which is what caused the OrbitCamera to lag behind at low fps) */

        /* Update altitudes. */
        _altitudeSeaLevel = _body.transform.position.y;
        RaycastHit hit;
        if (Physics.Raycast(_body.transform.position, Vector3.down, out hit, Mathf.Infinity, _layerMask)) {
            _altitudeGround = hit.distance;
            //Debug.Log("I hit this thang: " + hit.collider.name);
        }

        /* Find position derivatives. */
        Vector3 velocity = _body.velocity;
        Vector3 oldVelocity = RigidbodyState.Lerp(_historyBuffer, Time.fixedTime - 0.25f).Velocity;
        _acceleration = (velocity - oldVelocity) / 0.25f;

        Vector3 localAngularVelocity = _body.transform.InverseTransformDirection(_body.angularVelocity);
        _localAngularAcceleration = localAngularVelocity - _localAngularVelocity;
        _localAngularVelocity = localAngularVelocity;

        /* Determine relative wind vector. */
        Vector3 windVelocity = _wind.GetWindVelocity(_body.transform.position);
        _relativeVelocity = _body.velocity - windVelocity;
        _trueAirspeed = _relativeVelocity.magnitude;

        /* Determine angle of attack. */
        Vector3 v1 = _body.transform.TransformDirection(_rollAxis);
        Vector3 v2 = _relativeVelocity.normalized;
        Vector3 n = _body.transform.TransformDirection(_pitchAxis);
        _angleOfAttack = MathUtils.AngleAroundAxis(v1, v2, n);

        /* Determine glide ratio. */
        float horizontalDistance = Mathf.Sqrt(Mathf.Pow(_body.velocity.x, 2f) + Mathf.Pow(_body.velocity.z, 2f));
        float verticalDistance = -_body.velocity.y;
        _glideRatio = horizontalDistance / verticalDistance;

        /* Determine Angle to ground for local axes. */
        Vector3 rollAxisCenter = _body.transform.forward;
        _angleToGroundRoll = MathUtils.AngleAroundAxis(Vector3.down, rollAxisCenter,
            _body.transform.TransformDirection(_rollAxis));
        Vector3 forward = _body.transform.TransformDirection(_rollAxis);
        Vector3 projectedForward = new Vector3(forward.x, 0f, forward.z);
        _angleToGroundPitch = MathUtils.AngleAroundAxis(projectedForward, forward,
            _body.transform.TransformDirection(_pitchAxis));

        _historyBuffer.Enqueue(RigidbodyState.ToImmutable(_body, Time.fixedTime));
    }

    void Update() {
        UpdatePrediction();
    }

    private void UpdatePrediction() {
        // The used acceleration stays constant over the whole simulation. We massage it quite a bit to get a stable trajectory
        Vector3 velocity = Vector3.Lerp(_prevVelocity, WorldVelocity, 1f * Time.deltaTime);
        Vector3 acceleration = Vector3.Lerp(_prevAcceleration, Acceleration * 1.33f, 0.5f * Time.deltaTime);
        _prevVelocity = velocity;
        _prevAcceleration = acceleration;
        acceleration = acceleration + Vector3.down; // Add a bit of gravity

        // Setup state for the raycast simulation
        float stepTime = _trajectorySimTime / _trajectoryRaycastSteps;
        Vector3 position = transform.position;

        _collisionPredicted = false;
        _normalizedCollisionIndex = -1f;

        // If collision, contains distance along line where 0.0 is start, and 1.0 is end of line.

        // Todo: decrease path resolution as distance from player increases to save performance

        // Check for collision along trajectory
        for (int i = 0; i < _trajectoryRaycastSteps && !_collisionPredicted; i++) {
            // Integrate velocity
            velocity += acceleration * stepTime;
            Vector3 deltaPos = velocity * stepTime;
            float deltaDistance = deltaPos.magnitude;

            // Check for collision along this segment
            RaycastHit hitInfo;
            if (Physics.SphereCast(position, 1f, deltaPos, out hitInfo, deltaDistance, _layerMask)) {
                _collisionPoint = hitInfo.point;
                _collisionNormal = hitInfo.normal;
                _normalizedCollisionIndex = (i + ((hitInfo.point - position).magnitude / deltaDistance)) /
                                            (float) _trajectoryRaycastSteps;
                _collisionPredicted = true;
            }

            // Integrate position
            position += deltaPos;
        }

        // Setup state for the visual simulation
        stepTime = _trajectorySimTime / _trajectorySegments;
        position = transform.position;
        velocity = _prevVelocity;

        for (int i = 0; i < _trajectorySegments; i++) {
            var state = new RigidbodyState();
            state.Position = position;
            state.Velocity = velocity;
            _predictionBuffer[i] = state;

            // Integrate velocity
            velocity += acceleration * stepTime;
            Vector3 deltaPos = velocity * stepTime;

            // Integrate position
            position += deltaPos;
        }
    }
}

[Serializable]
public struct RigidbodyState {
    public float Timestamp;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Vector3 AngularVelocity;
    public Vector3 AngularAcceleration;

    /* Todo: Generalize lerp patterns for better code reuse */

    public static RigidbodyState Lerp(RigidbodyState a, RigidbodyState b, float positionLerp, float rotationLerp) {
        return new RigidbodyState {
            Timestamp = Mathf.Lerp(a.Timestamp, b.Timestamp, positionLerp),

            Position = Vector3.Lerp(a.Position, b.Position, positionLerp),
            Velocity = Vector3.Lerp(a.Velocity, b.Velocity, positionLerp),
            Acceleration = Vector3.Lerp(a.Acceleration, b.Acceleration, positionLerp),

            Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, rotationLerp),
            AngularVelocity = Vector3.Lerp(a.AngularVelocity, b.AngularVelocity, rotationLerp),
            AngularAcceleration = Vector3.Lerp(a.AngularAcceleration, b.AngularAcceleration, rotationLerp)
        };
    }

    public static RigidbodyState Lerp(IList<RigidbodyState> stateBuffer, double time) {
        if (stateBuffer.Count == 0) {
            throw new ArgumentOutOfRangeException("stateBuffer", "is empty");
        }

        if (time < stateBuffer[0].Timestamp) {
            //Debug.LogWarning("Time is older than any state in buffer");
            return stateBuffer[0];
        }

        // Run through states starting with the oldest
        for (int i = 1; i < stateBuffer.Count; i++) {
            var rhs = stateBuffer[i];
            if (time < rhs.Timestamp) {
                var lhs = stateBuffer[i - 1];
                float lerp = Mathf.InverseLerp(lhs.Timestamp, rhs.Timestamp, (float)time);
                var result = Lerp(lhs, rhs, lerp, lerp);
                return result;
            }
        }

        Debug.LogWarning("Time is newer than any state in buffer");
        return stateBuffer[stateBuffer.Count - 1];
    }

    public static RigidbodyState ToImmutable(Rigidbody body) {
        return new RigidbodyState {
            Timestamp = 0f,
            Position = body.position,
            Velocity = body.velocity,
            Acceleration = Vector3.zero,
            Rotation = body.rotation,
            AngularVelocity = body.angularVelocity,
            AngularAcceleration = Vector3.zero
        };
    }

    public static RigidbodyState ToImmutable(Rigidbody body, float timestamp) {
        return new RigidbodyState {
            Timestamp = timestamp,
            Position = body.position,
            Velocity = body.velocity,
            Acceleration = Vector3.zero,
            Rotation = body.rotation,
            AngularVelocity = body.angularVelocity,
            AngularAcceleration = Vector3.zero
        };
    }

    public static RigidbodyState ToImmutable(Rigidbody body, Vector3 acceleration, Vector3 angularAcceleration, float timestamp) {
        return new RigidbodyState {
            Timestamp = timestamp,
            Position = body.position,
            Velocity = body.velocity,
            Acceleration = acceleration,
            Rotation = body.rotation,
            AngularVelocity = body.angularVelocity,
            AngularAcceleration = angularAcceleration
        };
    }

    public static void Apply(Rigidbody body, RigidbodyState state) {
        body.position = state.Position;
        body.velocity = state.Velocity;
        body.rotation = state.Rotation;
        body.angularVelocity = state.AngularVelocity;
    }

    public override string ToString() {
        return string.Format("Position: {0}, Rotation: {1}, Velocity: {2}, Acceleration: {3}, AngularVelocity: {4}", Position, Rotation, Velocity, Acceleration, AngularVelocity);
    }
}