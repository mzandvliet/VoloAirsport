/**
 * Created by Martijn Zandvliet, 2014
 * 
 * Use of the source code in this document is governed by the terms
 * and conditions described in the Ramjet Anvil End-User License Agreement.
 * 
 * A copy of the Ramjet Anvil EULA should have been provided in the purchase
 * of the license to this source code. If you do not have a copy, please
 * contact me.
 * 
 * For any inquiries, contact: martijn@ramjetanvil.com
 */

using UnityEngine;

namespace RamjetAnvil.Volo {
    public class PIDController {
        private float _integral;
        private float _lastError;
        private PIDConfig _config;

        public PIDController(PIDConfig config) {
            _config = config;
        }

        public PIDOutput Update(float currentError) {
            _integral = Mathf.Clamp(_integral + currentError * Config.IntegralGain, -1f, 1f);
            _integral -= _integral * 1f * Time.deltaTime;
            float errorDelta = (currentError - _lastError);
            _lastError = currentError;

            var output = new PIDOutput();
            output.ProportionalCorrection = currentError * Config.ProportionalGain;
            output.IntegralCorrection = _integral;
            output.DerivativeCorrection = errorDelta * Config.DerivativeGain;
            return output;
        }

        public void Reset() {
            _integral = 0f;
            _lastError = 0f;
        }

        public PIDConfig Config {
            get { return _config; }
        }

        public struct PIDState {
            public float Integral;
            public float LastError;
        }

        public struct PIDOutput {
            public float ProportionalCorrection;
            public float IntegralCorrection;
            public float DerivativeCorrection;

            public float Sum() {
                return ProportionalCorrection + IntegralCorrection + DerivativeCorrection;
            }
        }
    }

    [System.Serializable]
    public class PIDConfig {
        public float ProportionalGain;
        public float IntegralGain;
        public float DerivativeGain;

        public PIDConfig(float proportionalGain, float integralGain, float derivativeGain) {
            ProportionalGain = proportionalGain;
            IntegralGain = integralGain;
            DerivativeGain = derivativeGain;
        }
    }
}

