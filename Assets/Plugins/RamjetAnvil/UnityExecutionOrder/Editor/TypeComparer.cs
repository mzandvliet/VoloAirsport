using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityExecutionOrder {
    public class TypeComparer : IComparer<Type> {
        public static readonly IComparer<Type> Default = new TypeComparer();

        private TypeComparer() {}

        public int Compare(Type x, Type y) {
            return String.Compare(x.FullName, y.FullName, StringComparison.Ordinal);
        }
    }
}
