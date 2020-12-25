using UnityEngine;

namespace RamjetAnvil.Volo {
    public static class UnityParachuteMetrics {

        public static Vector3 GetOrbitCamCentroid(Parachute p) {
            return Vector3.Lerp(p.Pilot.Torso.position, p.Sections[p.Sections.Count/2].Cell.Body.position, 0.5f);
        }
    }
}