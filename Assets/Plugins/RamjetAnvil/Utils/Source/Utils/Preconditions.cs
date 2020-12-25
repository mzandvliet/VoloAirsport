using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public static class Preconditions
    {
        public static void CheckArgument(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new ArgumentException(message);  
            }
        }
    }
}
