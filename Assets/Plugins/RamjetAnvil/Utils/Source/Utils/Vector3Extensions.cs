using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public static class Vector3Extensions
    {
        public static Vector3 X(this Vector3 v, float value) {
            v.x = value;
            return v;
        }

        public static Vector3 Y(this Vector3 v, float value)
        {
            v.y = value;
            return v;
        }

        public static Vector3 Z(this Vector3 v, float value)
        {
            v.z = value;
            return v;
        }

        public static Vector3 Divide(this Vector3 lhs, Vector3 rhs) {
            return new Vector3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);    
        }
    }
}
