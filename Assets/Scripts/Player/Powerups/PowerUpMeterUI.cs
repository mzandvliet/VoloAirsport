using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityExecutionOrder;

namespace RamjetAnvil.Volo {

    [Run.After(typeof(PowerUp))]
    public class PowerUpMeterUI : MonoBehaviour {
        [SerializeField] private Gradient _chargeGradient;
        [SerializeField] private Image _image;
        [SerializeField] private PowerUp _powerUp;

        void Update() {
            _image.color = _chargeGradient.Evaluate(_powerUp.Amount);
            _image.fillAmount = _powerUp.Amount;
        }
    }
}
