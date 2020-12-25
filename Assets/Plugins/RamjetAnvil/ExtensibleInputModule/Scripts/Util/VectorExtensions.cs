using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.InputModule {
    public static class VectorExtensions {

        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max) {
            return new Vector3(
                x: Mathf.Clamp(v.x, min.x, max.x),
                y: Mathf.Clamp(v.y, min.y, max.y),
                z: Mathf.Clamp(v.z, min.z, max.z));
        }
    }
}
