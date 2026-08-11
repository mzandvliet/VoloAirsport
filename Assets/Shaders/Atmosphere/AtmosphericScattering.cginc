#ifndef ATMOSPHERIC_SCATTERING_INCLUDED
#define ATMOSPHERIC_SCATTERING_INCLUDED

// Shared single-scattering atmosphere model, replacing the stripped Time of Day plugin
// (TOD_Base.cginc / TOD_Scattering.cginc).
//
// One integrator serves both the sky and the distance fog: the sky is simply the view ray
// integrated until it leaves the atmosphere, the fog is the same ray stopped at scene
// geometry. Because it is literally the same function with a different upper bound, distant
// terrain dissolves into the sky with no seam at the horizon - there is no second model to
// disagree with. Sun-angle response (blue zenith, warm hazy horizon, red at sunset) falls
// out of the wavelength-dependent Rayleigh extinction rather than being authored.
//
// Deliberately simplified: single scattering only (no multiple-scattering or ozone terms),
// short ray march, no clouds. Accuracy is traded for cost and legibility.

#define ATMOS_PI 3.14159265359

// Raymarch resolution. Cost is ATMOS_VIEW_STEPS * ATMOS_SUN_STEPS per pixel, so these are
// the first knobs to turn if the fullscreen fog pass is too expensive. A shader may #define
// them before including this file to opt into a cheaper integration.
#ifndef ATMOS_VIEW_STEPS
#define ATMOS_VIEW_STEPS 8
#endif
#ifndef ATMOS_SUN_STEPS
#define ATMOS_SUN_STEPS 3
#endif

float3 _AtmosSunDir;      // normalized, pointing *toward* the sun
float4 _AtmosSunColor;    // light colour * intensity
float4 _AtmosNightColor;  // floor colour so the sky doesn't go pure black at night

float _AtmosPlanetRadius;
float _AtmosAtmosphereHeight;
float _AtmosGroundHeight;         // world Y treated as sea level
float _AtmosRayleighScaleHeight;
float _AtmosMieScaleHeight;

float3 _AtmosRayleighCoeff;       // per-metre, wavelength dependent (this is what makes it blue)
float _AtmosMieCoeff;             // per-metre, roughly wavelength independent
float _AtmosMieG;                 // forward-scattering anisotropy

float _AtmosDensityMultiplier;    // weather-driven haze
float _AtmosExposure;

// Stand-in planet surface, so view rays that pass below the horizon without hitting real
// terrain terminate on something instead of integrating out into empty space. Only ever
// visible beyond the terrain tiles, where aerial perspective has completely saturated - so
// these mostly set the tint of the far haze rather than shading anything legible.
float4 _AtmosGroundAlbedo;
float _AtmosGroundBrightness;
float _AtmosGroundAmbient;

// Lifts world space into planet space, where the origin is the planet centre. Horizontal
// distance is included, so curvature drops the far horizon away correctly over long views.
float3 AtmosWorldToPlanet(float3 worldPos) {
    return float3(worldPos.x,
                  worldPos.y - _AtmosGroundHeight + _AtmosPlanetRadius,
                  worldPos.z);
}

// The quadratic's constant term, written as (|o|-r)(|o|+r) rather than dot(o,o) - r*r.
// Algebraically identical, but at planet scale the latter subtracts two ~4e13 values whose
// difference is ~1e10, which float32 cannot represent - it catastrophically cancels and the
// horizon jitters. This form keeps the small quantity (altitude) small throughout.
float AtmosSphereQuadraticC(float3 origin, float radius) {
    float len = length(origin);
    return (len - radius) * (len + radius);
}

// Distance from a point inside a sphere to its surface along dir.
float AtmosRaySphereExit(float3 origin, float3 dir, float radius) {
    float b = dot(origin, dir);
    float c = AtmosSphereQuadraticC(origin, radius);
    float d = b * b - c;
    if (d < 0.0) {
        return 0.0;
    }
    return max(-b + sqrt(d), 0.0);
}

// Distance to the nearest intersection with a sphere, or -1 if the ray misses it.
// Used to stop view rays at the planet surface instead of letting them run to infinity.
float AtmosRaySphereNear(float3 origin, float3 dir, float radius) {
    float b = dot(origin, dir);
    float c = AtmosSphereQuadraticC(origin, radius);
    float d = b * b - c;
    if (d < 0.0) {
        return -1.0;
    }
    float sqrtD = sqrt(d);
    float t0 = -b - sqrtD;
    if (t0 >= 0.0) {
        return t0;
    }
    float t1 = -b + sqrtD;
    return t1 >= 0.0 ? t1 : -1.0;
}

// (Rayleigh, Mie) relative density at a planet-space point.
float2 AtmosDensityAt(float3 p) {
    float h = max(length(p) - _AtmosPlanetRadius, 0.0);
    return float2(exp(-h / _AtmosRayleighScaleHeight),
                  exp(-h / _AtmosMieScaleHeight));
}

// (Rayleigh, Mie) optical depth from p toward the sun, out through the atmosphere. This is
// what reddens the light near sunrise/sunset: a low sun means a long path, which strips the
// blue end out of the sunlight before it ever gets scattered toward the viewer.
float2 AtmosSunOpticalDepth(float3 p) {
    float atmosRadius = _AtmosPlanetRadius + _AtmosAtmosphereHeight;
    float dist = AtmosRaySphereExit(p, _AtmosSunDir, atmosRadius);
    float ds = dist / ATMOS_SUN_STEPS;

    float2 od = 0.0;
    [unroll]
    for (int i = 0; i < ATMOS_SUN_STEPS; i++) {
        float3 s = p + _AtmosSunDir * (ds * (i + 0.5));
        od += AtmosDensityAt(s) * ds;
    }
    return od;
}

/// Core integrator. rayLength is how far to march before stopping - pass a huge value for
/// sky rays that should escape to infinity, or the distance to scene geometry for fog.
/// Returns light scattered *into* the ray, and how much of what lies beyond survives.
void AtmosIntegrate(float3 planetOrigin, float3 rayDir, float rayLength,
                    out float3 inscatter, out float3 transmittance) {
    float atmosRadius = _AtmosPlanetRadius + _AtmosAtmosphereHeight;
    float maxDist = AtmosRaySphereExit(planetOrigin, rayDir, atmosRadius);
    float dist = min(rayLength, maxDist);

    float ds = dist / ATMOS_VIEW_STEPS;

    float2 odView = 0.0;
    float3 sumR = 0.0;
    float3 sumM = 0.0;

    [loop]
    for (int i = 0; i < ATMOS_VIEW_STEPS; i++) {
        float3 p = planetOrigin + rayDir * (ds * (i + 0.5));

        float2 dens = AtmosDensityAt(p) * ds * _AtmosDensityMultiplier;
        odView += dens;

        float2 odSun = AtmosSunOpticalDepth(p) * _AtmosDensityMultiplier;

        // Extinction over the full path: viewer -> sample point -> sun. The 1.1 factor on
        // Mie is the usual approximation for absorption on top of scattering.
        float3 tau = _AtmosRayleighCoeff * (odView.x + odSun.x)
                   + _AtmosMieCoeff * 1.1 * (odView.y + odSun.y);
        float3 t = exp(-tau);

        sumR += dens.x * t;
        sumM += dens.y * t;
    }

    float cosTheta = dot(rayDir, _AtmosSunDir);
    float cos2 = cosTheta * cosTheta;

    float phaseR = 3.0 / (16.0 * ATMOS_PI) * (1.0 + cos2);

    float g = _AtmosMieG;
    float g2 = g * g;
    float phaseM = 3.0 / (8.0 * ATMOS_PI)
                 * ((1.0 - g2) * (1.0 + cos2))
                 / ((2.0 + g2) * pow(max(1.0 + g2 - 2.0 * g * cosTheta, 1e-4), 1.5));

    inscatter = (sumR * _AtmosRayleighCoeff * phaseR + sumM * _AtmosMieCoeff * phaseM)
              * _AtmosSunColor.rgb * _AtmosExposure;

    float3 tauView = _AtmosRayleighCoeff * odView.x + _AtmosMieCoeff * 1.1 * odView.y;
    transmittance = exp(-tauView);

    // Night floor, added where the atmosphere is thick enough to have blocked the view of
    // whatever lies beyond. Applied inside the integrator so sky and fog agree about it.
    inscatter += _AtmosNightColor.rgb * (1.0 - transmittance);
}

/// Lambertian shading for the stand-in planet surface, lit by sunlight that has been
/// reddened by its own path down through the atmosphere - so the far ground warms at sunset
/// along with everything else.
float3 AtmosGroundColor(float3 planetPos) {
    float3 normal = normalize(planetPos);
    float ndotl = saturate(dot(normal, _AtmosSunDir));

    float2 odSun = AtmosSunOpticalDepth(planetPos) * _AtmosDensityMultiplier;
    float3 sunTransmittance = exp(-(_AtmosRayleighCoeff * odSun.x
                                  + _AtmosMieCoeff * 1.1 * odSun.y));

    return _AtmosGroundAlbedo.rgb * _AtmosGroundBrightness
         * (sunTransmittance * ndotl + _AtmosGroundAmbient);
}

/// Sky: integrate along the view ray until it either leaves the atmosphere or meets the
/// planet. Rays that pass below the horizon terminate on the stand-in ground rather than
/// running out into space - without that they accumulate almost no scattering and read as a
/// black void wherever real terrain doesn't cover them.
float3 AtmosSkyColor(float3 worldCamPos, float3 rayDir) {
    float3 origin = AtmosWorldToPlanet(worldCamPos);

    float groundDist = AtmosRaySphereNear(origin, rayDir, _AtmosPlanetRadius);
    bool hitsGround = groundDist > 0.0;

    float3 inscatter, transmittance;
    AtmosIntegrate(origin, rayDir, hitsGround ? groundDist : 1e9, inscatter, transmittance);

    // Nothing behind a ray that escapes, so the background is simply black space.
    float3 background = hitsGround ? AtmosGroundColor(origin + rayDir * groundDist) : 0.0;

    return background * transmittance + inscatter;
}

/// Aerial perspective: attenuate what the camera can see of worldPos, and add what the air
/// between them scatters in. At long distances transmittance goes to zero and this converges
/// on exactly AtmosSkyColor - which is what makes the horizon seamless.
float3 AtmosApplyFog(float3 sceneColor, float3 worldCamPos, float3 worldPos) {
    float3 seg = worldPos - worldCamPos;
    float dist = length(seg);
    if (dist < 1e-3) {
        return sceneColor;
    }
    float3 rayDir = seg / dist;

    float3 inscatter, transmittance;
    AtmosIntegrate(AtmosWorldToPlanet(worldCamPos), rayDir, dist, inscatter, transmittance);

    return sceneColor * transmittance + inscatter;
}

#endif // ATMOSPHERIC_SCATTERING_INCLUDED
