using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ParticleSystemExtension {
    public static void EnableEmission(this ParticleSystem particleSystem, bool enabled) {
        var emission = particleSystem.emission;
        emission.enabled = enabled;
    }
     
    public static float GetEmissionRate(this ParticleSystem particleSystem) {
        return particleSystem.emission.rate.constantMax;
    }
     
    public static void SetEmissionRate(this ParticleSystem particleSystem, float emissionRate) {
        var emission = particleSystem.emission;
        var rate = emission.rate;
        rate.constantMax = emissionRate;
        emission.rate = rate;
//        particleSystem.emissionRate = emissionRate;
    }
}
