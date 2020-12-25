using System.Collections.Generic;
using UnityEngine;
using UnityNoise;

/* Todo:
 * Particle emitter lodding (turn them off very far away, modulate particle count by distance)
 * Use ParticleSystemRenderer.bounds to cull wind effectors in query using octree
 * Allow different kinds of WindEffectors. Linear, spiral, etc. (Data-oriented, sorted by type)
 * Also allow one or two dynamic windzones (that can move around), but use them sparingly
 * Use batched queries for all particles at once.
 * Turn wind effectors on/off dynamically. (Don't worry about transitions right now)
*/

public class WindManager : MonoBehaviour {
    ///* Air density in Kg/m3. */
    [SerializeField] float _airDensity = 1.293f;
    [SerializeField] private Vector3 _baseWindVelocity = Vector3.zero;
    [SerializeField] private Vector3 _borderBottomLeft = new Vector3(-4000f, 0f, -4000f);
    [SerializeField] private Vector3 _borderTopRight = new Vector3(4000f, 0, 4000f);
    [SerializeField] private float _borderSize = 200f;
    [SerializeField] private float _borderWindSpeed = 50f;
    [SerializeField] private float _borderWindPow = 1f;
    [SerializeField] private NoiseOctave[] _turbulence;
    [SerializeField] private float _verticalWindScale = 0.3f;
    [SerializeField] private float _minAltitude = 0f;
    [SerializeField] private float _maxAltitude = 5000f;

    [SerializeField] private bool _showOctreeDebug;

    private BoundsOctree<WindEffector> _effectors;
    private List<WindEffector> _queryResults;

    private BorderConfig _borderConfig;
    private Perlin _perlin;

    private void Awake() {
        RecalculateBorders();

        _effectors = new BoundsOctree<WindEffector>(8000f, Vector3.zero, 250f, 1.2f);
        _queryResults = new List<WindEffector>(8);

        _perlin = new Perlin(1234);
    }

    public void AddEffector(WindEffector effector) {
        _effectors.Add(effector, effector.Bounds);
    }

    public void RemoveEffector(WindEffector effector) {
        _effectors.Remove(effector);
    }

    public float GetAirDensity(Vector3 position) {
        return _airDensity;
    }

    public Vector3 GetWindVelocity(Vector3 position) {
        Vector3 windVelocity = _baseWindVelocity;

        Bounds bounds = new Bounds(position, Vector3.one);
        _queryResults.Clear();
        _effectors.GetColliding(bounds, _queryResults);

        windVelocity += GetLocalWindVelocity(_queryResults, position);
        windVelocity += GetBorderWindVelocity(_borderConfig, position);
        windVelocity += GetTurbulence(_perlin, _turbulence, _verticalWindScale, _minAltitude, _maxAltitude, position);

        return windVelocity;
    }

    public void GetWindVelocities(IList<Vector3> positions, IList<Vector3> velocities, Bounds bounds) {
        var perlin = _perlin;
        var turbulence = _turbulence;
        float minAlt = _minAltitude;
        float maxAlt = _maxAltitude;
        float vertScale = _verticalWindScale;
        var borderConf = _borderConfig;

        _queryResults.Clear();
        _effectors.GetColliding(bounds, _queryResults);

        for (int i = 0; i < positions.Count; i++) {
            velocities[i] = Vector3.zero;
            velocities[i] += GetBorderWindVelocity(borderConf, positions[i]);
            velocities[i] += GetTurbulence(perlin, turbulence, vertScale, minAlt, maxAlt, positions[i]);
        }
        GetLocalWindVelocities(_queryResults, positions, velocities);
    }
    
    public void GetWindVelocities(ParticleSystem.Particle[] particles, int startIndex, int endIndex, Bounds bounds, float velocityScale) {
        var perlin = _perlin;
        var turbulence = _turbulence;
        float minAlt = _minAltitude;
        float maxAlt = _maxAltitude;
        float vertScale = _verticalWindScale;
        var borderConf = _borderConfig;

        _queryResults.Clear();
        _effectors.GetColliding(bounds, _queryResults);

        for (int i = startIndex; i < endIndex; i++) {
            particles[i].velocity = Vector3.zero;
            particles[i].velocity += GetBorderWindVelocity(borderConf, particles[i].position);
            particles[i].velocity += GetTurbulence(perlin, turbulence, vertScale, minAlt, maxAlt, particles[i].position);
        }

        GetLocalWindVelocities(_queryResults, particles, startIndex, endIndex, velocityScale);
    }

    private static Vector3 GetLocalWindVelocity(List<WindEffector> effectors, Vector3 position) {
        Vector3 wind = Vector3.zero;

        for (int i = 0; i < effectors.Count; i++) {
            var zone = effectors[i];
            wind += GetLocalWindVelocity(zone, position);
        }
        
        return wind;
    }

    private static void GetLocalWindVelocities(List<WindEffector> effectors, IList<Vector3> positions, IList<Vector3> velocities) {
        for (int i = 0; i < effectors.Count; i++) {
            var zone = effectors[i];
            for (int j = 0; j < positions.Count; j++) {
                velocities[j] += GetLocalWindVelocity(zone, positions[j]);
            }
        }
    }

    private static void GetLocalWindVelocities(List<WindEffector> effectors, ParticleSystem.Particle[] particles, int startIndex, int endIndex, float velocityScale) {
        for (int i = 0; i < effectors.Count; i++) {
            var zone = effectors[i];
            for (int j = startIndex; j < endIndex; j++) {
                particles[j].velocity += GetLocalWindVelocity(zone, particles[j].position);
                particles[j].velocity *= velocityScale;

                // Todo: this deals with buggy winds, but we'd rather prevent those in the first place
                particles[j].velocity = Vector3.ClampMagnitude(particles[j].velocity, 100f);
            }
        }
    }

    private static Vector3 GetLocalWindVelocity(WindEffector effector, Vector3 pos) {
        Vector3 v = Vector3.zero;

        Vector3 localPos = pos - effector.Position;
        Vector3 verticalComponent = Vector3.Project(localPos, effector.Forward);
        localPos -= verticalComponent;

        float radialProximity = 1f - Mathf.Clamp01(localPos.magnitude / effector.OuterRadius);
        radialProximity = Mathf.Pow(radialProximity, 1.5f);
        v += Vector3.Cross(effector.Forward, localPos).normalized * radialProximity * effector.RotationalWindSpeed;
        v += effector.Forward * (radialProximity * effector.WindSpeed);
        float verticalProximity = 1f - Mathf.Clamp01((Mathf.Abs(verticalComponent.magnitude) - effector.InnerRadius) / (effector.OuterRadius - effector.InnerRadius));
        v *= verticalProximity;

        return v;
    }

    private static Vector3 GetBorderWindVelocity(BorderConfig border, Vector3 position) {
        // If position is near world borders, add wind that pushes back

        //if (position.x < _borderBottomLeft.x + _borderSize) {
        //  windVelocity.x += (_borderSize - (position.x - _borderBottomLeft.x)) / _borderSize * _borderWindSpeed;
        //}

        // Below is an optimized version of the above, for all four borders

        Vector3 windVelocity = new Vector3();

        const float verticalScale = 0.5f;

        if (position.x < border.BottomLeftInner.x) {
            float correction = (position.x - border.BottomLeftInner.x) * -border.SizeInverse;
            correction = Mathf.Pow(correction, border.WindPow) * border.WindSpeed;
            windVelocity.x += correction;
            windVelocity.y += Mathf.Abs(correction) * verticalScale;
        }
        if (position.z < border.BottomLeftInner.z) {
            float correction = (position.z - border.BottomLeftInner.z) * -border.SizeInverse;
            correction = Mathf.Pow(correction, border.WindPow) * border.WindSpeed;
            windVelocity.z += correction;
            windVelocity.y += Mathf.Abs(correction) * verticalScale;
        }
        if (position.x > border.TopRightInner.x) {
            float correction = ((position.x - border.TopRightInner.x)) * border.SizeInverse;
            correction = Mathf.Pow(correction, border.WindPow) * border.WindSpeed;
            windVelocity.x -= correction;
            windVelocity.y += Mathf.Abs(correction) * verticalScale;
        }
        if (position.z > border.TopRightInner.z) {
            float correction = ((position.z - border.TopRightInner.z)) * border.SizeInverse;
            correction = Mathf.Pow(correction, border.WindPow) * border.WindSpeed;
            windVelocity.z -= correction;
            windVelocity.y += Mathf.Abs(correction) * verticalScale;
        }
        return windVelocity;
    }

    private static Vector3 GetTurbulence(Perlin noise, NoiseOctave[] octaves, float verticalWindScale, float minAlt, float maxAlt, Vector3 position) {
        float altitudeFalloff = 0.5f + 0.5f * Mathf.InverseLerp(minAlt, maxAlt, position.y);

        Vector3 turbulence = Vector3.zero;

        for (int i = 0; i < octaves.Length; i++) {
            var octave = octaves[i];

            float amplitude = octave.Amplitude * altitudeFalloff;

            float timeVariation = Time.time * octave.Frequency;

            turbulence += new Vector3(
                noise.Noise(position.x * octave.Scale, timeVariation) * amplitude,
                noise.Noise(position.y * octave.Scale, timeVariation) * amplitude * verticalWindScale,
                noise.Noise(position.z * octave.Scale, timeVariation) * amplitude);
        }

        return turbulence;
    }

    private void RecalculateBorders() {
        Vector3 border = new Vector3(_borderSize, 0f, _borderSize);
        _borderConfig = new BorderConfig();
        _borderConfig.Size = _borderSize;
        _borderConfig.WindSpeed = _borderWindSpeed;
        _borderConfig.WindPow = _borderWindPow;
        _borderConfig.BottomLeftInner = _borderBottomLeft + border;
        _borderConfig.TopRightInner = _borderTopRight - border;
        _borderConfig.SizeInverse = 1f / _borderSize; // Optimizes out the division
    }

    private void OnDrawGizmos() {
        if (_showOctreeDebug) {
            Gizmos.color = Color.magenta;
            _effectors.DrawAllBounds();
            Gizmos.color = Color.green;
            _effectors.DrawAllObjects();
        }
    }

    [System.Serializable]
    public class NoiseOctave {
        [SerializeField]
        public float Scale = 1f;
        [SerializeField]
        public float Frequency = 1f;
        [SerializeField]
        public float Amplitude = 1f;
    }

    private struct BorderConfig {
        public float Size;
        public float WindSpeed;
        public float WindPow;
        public Vector3 BottomLeftInner;
        public Vector3 TopRightInner;
        public float SizeInverse;
    }
}
