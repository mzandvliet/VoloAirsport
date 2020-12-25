using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using UnityEngine.Profiling;

public class KeyboardPoller : MonoBehaviour {

    private KeyCode[] _keyCodes;
    private ISubject<ButtonEvent> _buttonEvents;

    void Awake() {
        _keyCodes = EnumUtils.GetValues<KeyCode>()
            // Includes keyboard and mouse buttons
            .Where(keyCode => (int)keyCode >= 8 && (int)keyCode <= 329) 
            .ToArray();
        _buttonEvents = new Subject<ButtonEvent>();
    }

    void Update() {
        for (int i = 0; i < _keyCodes.Length; i++) {
            var keyCode = _keyCodes[i];
            if (Input.GetKeyDown(keyCode)) {
                _buttonEvents.OnNext(ButtonEvent.Down);
            }
            if (Input.GetKeyUp(keyCode)) {
                _buttonEvents.OnNext(ButtonEvent.Up);
            }
        }
    }

    public IObservable<ButtonEvent> ButtonEvents {
        get { return _buttonEvents; }
    }
}
