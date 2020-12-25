using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Impero.Unity;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public class InputLogger : MonoBehaviour {

    [SerializeField] private int _controllerId = 0;

    private InputMap<InputSource, ButtonState> _inputMap; 

    void Awake() {
        _inputMap = InputRebinding.CreateCandidateInputMap(new ControllerId.Unity(_controllerId));
    }

    void Update() {
        foreach (var inputPair in _inputMap.Source) {
            var pollable = inputPair.Value;
            if (pollable() == ButtonState.Pressed) {
                Debug.Log("Received input from: " + inputPair.Key);
            }
        }
    }
}
