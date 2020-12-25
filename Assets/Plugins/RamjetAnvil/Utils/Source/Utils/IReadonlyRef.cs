using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public interface IReadonlyRef<out T>
    {
        T V { get; }
    }
}
