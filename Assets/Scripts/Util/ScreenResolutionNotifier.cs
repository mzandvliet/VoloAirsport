using System;
using System.Reactive.Subjects;
using UnityEngine;
using Resolution = RamjetAnvil.Volo.Resolution;

namespace RamjetAnvil.Unity.Utility {
    public class ScreenResolutionNotifier : MonoBehaviour {
        public event Action<Resolution> OnResolutionChanged;
        private ISubject<Resolution> _currentResolution;

        private bool _isInitialized;
        private Resolution _resolution;

        void Awake() {
            Initialize();
        }

        void Update() {
            var currentResolution = new Resolution(Screen.width, Screen.height);
            if (currentResolution != _resolution) {
                _resolution = currentResolution;

                if (OnResolutionChanged != null) {
                    OnResolutionChanged(_resolution);
                }
                _currentResolution.OnNext(_resolution);
            }
        }

        public IObservable<Resolution> CurrentResolution {
            get {
                Initialize();
                return _currentResolution;
            }
        }

        private void Initialize() {
            if (!_isInitialized) {
                _resolution = new Resolution(Screen.width, Screen.height);
                _currentResolution = new BehaviorSubject<Resolution>(_resolution);
                _isInitialized = true;
            }
        }
    }
}
