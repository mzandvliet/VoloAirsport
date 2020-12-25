using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class TemporalRechargePowerup : PowerUp {
        [SerializeField, Dependency] private AbstractUnityClock _gameClock;
        [SerializeField] private PilotActionMapProvider _actionMapProvider;
        [SerializeField] private PowerUp _controllingPowerUp;
        [SerializeField] private float _timeActive = 1f;
        [SerializeField] private float _timeRecharging = 1f;
        // Minimum charge needed to activate the powerup
        [SerializeField] private float _minActivationCharge = 0.10f;

        private bool _isPowerUpActive;
        private float _charge;

        void Awake() {
            IsActive = true;
            _charge = 1f;
        }

        void Update() {
            var actionMap = _actionMapProvider.ActionMap;
            if (actionMap.PollButtonEvent(WingsuitAction.ActivateSlowMo) == ButtonEvent.Down) {
                _isPowerUpActive = _charge > _minActivationCharge;
            } else if(actionMap.PollButtonEvent(WingsuitAction.ActivateSlowMo) == ButtonEvent.Up) {
                _isPowerUpActive = false;
            }

            if (IsActive && _isPowerUpActive && _charge > 0f) {
                _controllingPowerUp.IsActive = true;
                _charge -= _gameClock.DeltaTime / _timeActive;
            } else {
                _isPowerUpActive = false;
                _controllingPowerUp.IsActive = false;
                _charge += _gameClock.DeltaTime / _timeRecharging;
            }
            _charge = Mathf.Clamp(_charge, 0f, 1f);
        }

        public override bool IsActive { get; set; }

        public override float Amount {
            get { return _charge; }
        }
    }
}
