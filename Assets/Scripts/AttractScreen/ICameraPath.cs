using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo {

    public interface ICameraPath {
        Routines.Animation Animation { get; }
        TimeSpan Duration { get; }
        ImmutableTransform From { get; }
        ImmutableTransform To { get; }
    }
}
