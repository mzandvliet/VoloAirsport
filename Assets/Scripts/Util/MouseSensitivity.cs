using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo;
using UnityEngine;

// TODO Maybe just expose all the current game settings in this place
public class MouseSensitivity : MonoBehaviour {

    [SerializeField] private float _sensitivity = 0.2f;

    private bool _isInitialized = false;

    public float Value {
        get {
            if (!_isInitialized) {
                _sensitivity = GameSettings.ReadSettingsFromDisk().Input.WingsuitMouseSensitivity;
                _isInitialized = true;
            }
            return _sensitivity;
        }
        set { _sensitivity = value; }
    }
}
