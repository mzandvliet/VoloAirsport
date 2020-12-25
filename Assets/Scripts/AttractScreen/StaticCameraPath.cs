using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo {
    public struct StaticCameraPath : ICameraPath {

        private readonly Routines.Animation _animation;
        private readonly TimeSpan _duration;
        private readonly ImmutableTransform _from;
        private readonly ImmutableTransform _to;

        public StaticCameraPath(Routines.Animation animation, TimeSpan duration, ImmutableTransform @from, ImmutableTransform to) {
            _animation = animation;
            _duration = duration;
            _from = @from;
            _to = to;
        }

        public Routines.Animation Animation {
            get { return _animation; }
        }

        public TimeSpan Duration {
            get { return _duration; }
        }

        public ImmutableTransform From {
            get { return _from; }
        }

        public ImmutableTransform To {
            get { return _to; }
        }
    }
}
