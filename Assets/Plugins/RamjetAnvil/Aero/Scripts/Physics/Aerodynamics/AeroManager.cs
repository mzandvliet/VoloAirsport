using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityNoise;

public class AeroManager : MonoBehaviour {
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

    private AeroWindZone[] _windZones;
    private BorderConfig _borderConfig;
    private Perlin _perlin;

    private void Awake() {
        RecalculateBorders();

        _perlin = new Perlin(1234);
        _windZones = new AeroWindZone[0];    
    }

    public void GatherWindZones() {
        _windZones = FindObjectsOfType<AeroWindZone>();
        Debug.Log("Aeromanager found " + _windZones.Length + " windzones");
    }

    public float GetAirDensity(Vector3 position) {
        return _airDensity;
    }

    public Vector3 GetWindVelocity(Vector3 position) {
        Vector3 windVelocity = _baseWindVelocity;

        windVelocity += GetLocalWindVelocity(_windZones, position);
        windVelocity += GetBorderWindVelocity(_borderConfig, position);
        windVelocity += GetTurbulence(_perlin, _turbulence, _verticalWindScale, _minAltitude, _maxAltitude, position);

        return windVelocity;
    }

    // Todo: make static
    public void GetWindVelocities(IList<Vector3> positions, IList<Vector3> velocities, Bounds bounds) {
        var perlin = _perlin;
        var turbulence = _turbulence;
        float minAlt = _minAltitude;
        float maxAlt = _maxAltitude;
        float vertScale = _verticalWindScale;
        var borderConf = _borderConfig;

        for (int i = 0; i < positions.Count; i++) {
            velocities[i] = Vector3.zero;
            velocities[i] += GetBorderWindVelocity(borderConf, positions[i]);
            velocities[i] += GetTurbulence(perlin, turbulence, vertScale, minAlt, maxAlt, positions[i]);
        }
        GetLocalWindVelocities(_windZones, positions, velocities, bounds);
    }

    private static Vector3 GetLocalWindVelocity(AeroWindZone[] zones, Vector3 position) {
        Vector3 wind = Vector3.zero;

        for (int i = 0; i < zones.Length; i++) {
            var zone = zones[i];
            wind += GetLocalWindVelocity(zone, position);
        }
        
        return wind;
    }

    private static void GetLocalWindVelocities(AeroWindZone[] zones, IList<Vector3> positions, IList<Vector3> velocities, Bounds bounds) {
        for (int i = 0; i < zones.Length; i++) {
            var zone = zones[i];
            if (MathUtils.Intersect(bounds.min, bounds.max, zone.Position, zone.OuterRadius)) {
                for (int j = 0; j < positions.Count; j++) {
                    velocities[j] += GetLocalWindVelocity(zone, positions[j]);
                }
            }
        }
    }

    private static Vector3 GetLocalWindVelocity(AeroWindZone zone, Vector3 pos) {
        float squareDist = (zone.Position - pos).sqrMagnitude;
        if (squareDist < zone.OuterRadius * zone.OuterRadius) {
            float border = zone.OuterRadius - zone.InnerRadius;
            float affect = 1f - ((Mathf.Sqrt(squareDist) - zone.InnerRadius) / border);
            return zone.Forward * (affect * zone.WindSpeed);
        }
        return Vector3.zero;
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
