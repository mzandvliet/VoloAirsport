using UnityEngine;

namespace RamjetAnvil.Volo {
    public class SlowMotionPowerUp : PowerUp {
        [SerializeField] private AbstractUnityClock _realtimeClock;
        [SerializeField] private AbstractUnityClock _gameClock;
        [SerializeField] private AbstractUnityClock _playerClock;
        [SerializeField] private AbstractUnityClock _fixedClock;

        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _activationTime = 1f;
        [SerializeField] private float _playerSlowMotionSpeed = 0.5f;
        [SerializeField] private float _environmentSlowMotionSpeed = 0.3f;

        private float _slowMoAmount;

        void Update() {
            _slowMoAmount += (IsActive ? 1f : -1f) * _realtimeClock.DeltaTime / _activationTime;
            _slowMoAmount = Mathf.Clamp(_slowMoAmount, 0f, 1f);

            var playerSpeed = Mathf.Lerp(1f, _playerSlowMotionSpeed, _curve.Evaluate(_slowMoAmount));
            var environmentSpeed = Mathf.Lerp(1f, _environmentSlowMotionSpeed, _curve.Evaluate(_slowMoAmount));
            _gameClock.TimeScale = environmentSpeed;
            _playerClock.TimeScale = playerSpeed;
            _fixedClock.TimeScale = playerSpeed;
        }

        public override bool IsActive { get; set; }

        public override float Amount {
            get { return _slowMoAmount; }
        }
    }
}
