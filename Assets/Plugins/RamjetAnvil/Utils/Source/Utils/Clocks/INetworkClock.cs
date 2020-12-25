using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility {

    public interface INetworkClock : IClock {
        double TargetTimeInterpolationSpeed { get; set; }
        void SetCurrentTime(double time);
        void SetCurrentTimeInterpolated(double time);
    }
}
