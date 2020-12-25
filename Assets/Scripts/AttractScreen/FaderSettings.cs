using System;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class FaderSettings : MonoBehaviour {
        [SerializeField] private float _fadeInDurationInS;
        [SerializeField] private float _fadeOutDurationInS;

        public TimeSpan FadeInDuration {
            get { return TimeSpan.FromSeconds(_fadeInDurationInS); }
        }

        public TimeSpan FadeOutDuration {
            get { return TimeSpan.FromSeconds(_fadeOutDurationInS); }
        }
    }
}
