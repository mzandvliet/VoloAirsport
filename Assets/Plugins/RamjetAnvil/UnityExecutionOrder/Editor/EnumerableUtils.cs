using System.Collections.Generic;
using System.Linq;

namespace UnityExecutionOrder {
    public static class EnumerableUtils {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> e, T value) {
            return e.Concat(new[] {value});
        }

        public static string JoinToString<T>(this IEnumerable<T> e, string separator) {
            return e.Aggregate("", (acc, value) => {
                if (acc == "") {
                    return value.ToString();
                }
                return acc + separator + value;
            });
        }
    }
}
