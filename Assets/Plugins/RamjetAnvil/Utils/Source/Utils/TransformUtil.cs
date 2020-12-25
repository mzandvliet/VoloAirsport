using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    public static class TransformUtil
    {
        public static void CopyTo(this Transform source, Transform target) {
            target.position = source.position;
            target.rotation = source.rotation;
            target.localScale = source.localScale;
        }
    }
}
