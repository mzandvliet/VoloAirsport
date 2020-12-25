using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;

public class InputBinder : MonoBehaviour {

    [Dependency, SerializeField] private JoystickActivator _joystickActivator;
    [SerializeField] private float joystickAxisMappingThreshold = 0.7f;
    [SerializeField] private float mouseAxisMappingThreshold = 30f;

    private InputSource _cancelInput; 
    private InputMap<InputSource, ButtonEvent> _candidateInput;

    private Action<Maybe<InputSource>> _rebind;
    private bool _isRebinding;

    void Awake() {
        _cancelInput = InputSource.Key(KeyCode.Escape);
        _candidateInput = InputMap<InputSource, ButtonEvent>.Empty;
        _joystickActivator.ActiveController.Subscribe(controllerId => {
            _candidateInput = InputRebinding
                .CreateCandidateInputMap(
                    controllerId.HasValue ? controllerId.Value.Id : null,
                    joystickAxisMappingThreshold,
                    mouseAxisMappingThreshold)
                .Adapt(() => Adapters.ButtonEvents(() => Time.frameCount));
        });
    }

    void Update() {
        for (int i = 0; i < _candidateInput.KeyValuePairs.Length && _isRebinding; i++) {
            var kvPair = _candidateInput.KeyValuePairs[i];
            var inputSource = kvPair.Key;
            var pollSource = kvPair.Value;
            if (pollSource() == ButtonEvent.Down) {
                if (!inputSource.Equals(_cancelInput)) {
                    _rebind(Maybe.Just(inputSource));
                } else {
                    _rebind(Maybe.Nothing<InputSource>());
                }
                _isRebinding = false;
            }
        }    
    }

    public void StartRebind(Action<Maybe<InputSource>> rebind) {
        _rebind = rebind;
        _isRebinding = true;
    }

    public void StopRebind() {
        _isRebinding = false;
    }
}
