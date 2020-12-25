using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Util {

    public static class UnityUtil {

        public static Func<GameObject> ToFactory(this GameObject prefab) {
            return () => Object.Instantiate(prefab);
        } 
    }
}
