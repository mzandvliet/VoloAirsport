using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

namespace RamjetAnvil
{
    public static class DefaultPath {

        public static readonly Expression<Func<Vector2, float>> Vector2X = v => v.x;
        public static readonly Expression<Func<Vector2, float>> Vector2Y = v => v.y;

        public static readonly Expression<Func<Vector3, float>> Vector3X = v => v.x;
        public static readonly Expression<Func<Vector3, float>> Vector3Y = v => v.y;
        public static readonly Expression<Func<Vector3, float>> Vector3Z = v => v.z;
    }
}
