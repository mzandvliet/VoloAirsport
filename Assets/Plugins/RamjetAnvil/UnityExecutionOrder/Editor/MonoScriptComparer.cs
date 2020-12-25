using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace UnityExecutionOrder {

    public class MonoScriptComparer : IEqualityComparer<MonoScript> {
        public static readonly IEqualityComparer<MonoScript> Default = new MonoScriptComparer();

        private MonoScriptComparer() {}

        public bool Equals(MonoScript x, MonoScript y) {
            return x.GetClass().FullName == y.GetClass().FullName;
        }

        public int GetHashCode(MonoScript obj) {
            return obj.GetClass().FullName.GetHashCode();
        }
    }
}
