using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Vr {
    public class OculusResonseCallbackPoller : MonoBehaviour {

        [SerializeField] private uint _messageLimitPerFrame = 3;

        void Update() {
            if (Oculus.Platform.Core.IsInitialized()) {
                Oculus.Platform.Request.RunCallbacks(_messageLimitPerFrame);
            }
        }

        public uint MessageLimitPerFrame {
            get { return _messageLimitPerFrame; }
            set { _messageLimitPerFrame = value; }
        }
    }
}

