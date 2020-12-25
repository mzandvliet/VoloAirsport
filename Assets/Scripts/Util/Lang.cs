using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public static class Lang
    {
        /// <summary>
        /// Simply executes the given function. Can be used as sugar to construct a variable in its own context.
        /// </summary>
        public static T Let<T>(Func<T> block)
        {
            return block();
        }
    }
}
