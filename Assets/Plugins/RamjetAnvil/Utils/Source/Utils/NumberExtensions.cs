using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Runs the given action n times, each time passing in the number of the current iteration.
        /// </summary>
        /// <param name="n">number of times to call the given action</param>
        /// <param name="action">the action to perform</param>
        public static void Times(this int n, Action<int> action)
        {
            for (int i = 0; i < n; i++)
            {
                action(i);
            }
        }
    }
}
