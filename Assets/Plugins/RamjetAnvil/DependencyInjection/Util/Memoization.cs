using System;
using System.Collections.Generic;

namespace RamjetAnvil.DependencyInjection
{
    public static class Memoization
    {
        public static Func<TArg, TResult> Memoize<TArg, TResult>(this Func<TArg, TResult> func)
        {
            var values = new Dictionary<TArg, TResult>();
            return param => {
                TResult value;
                if (!values.TryGetValue(param, out value)) {
                    value = func(param);
                    values[param] = value;
                }
                return value;
            };
        }
    }
}
