using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RamjetAnvil.Impero;

public class SpectatorCamera : MonoBehaviour, ISpawnable {

    [SerializeField] private AbstractUnityClock _clock;
    [SerializeField] private float _translationSpeed = 10f;
    [SerializeField] private float _translationAcceleration = 10f;
    [SerializeField] private float _maxTranslationSpeed = 400f;
    [SerializeField] private float _speedUpModifier = 3f;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _translationSmoothingSpeed = 4f;
    [SerializeField] private float _rotationSmoothingSpeed = 4f;
    [SerializeField] private bool _handleInput = true;

    private SpectatorActionMap _actionMap;

    private float _extraMovementSpeed;
    private Vector3 _lastTranslation;
    private Vector2 _lastRotationInput;

    void Awake() {
        _handleInput = true;
    }

    public void OnSpawn() {
        _extraMovementSpeed = 0f;
        _lastTranslation = Vector3.zero;
        _lastRotationInput = Vector3.zero;
        _handleInput = true;
    }

    public void OnDespawn() {
    }

    public void EnableInputProcessing() {
        _handleInput = true;
    }

    public void DisableInputProcessing() {
        _handleInput = false;
    }

	void Update() {
	    if (_actionMap != null) {
            Translate();
            Rotate();   
	    }
	}

    void Translate() {
        var translationInput = _handleInput ? _actionMap.Movement() : Vector3.zero;

        // Speed up if user keeps pressing the stick
        float inputMagnitude = Mathf.Clamp01(translationInput.magnitude);
        if (inputMagnitude > 0.1f) {
            _extraMovementSpeed += _translationAcceleration * _clock.DeltaTime * Mathf.Pow(inputMagnitude, 2f);
        } else {
            _extraMovementSpeed = 0f;
        }
        var additionalSpeed = _extraMovementSpeed * Mathf.Max(1f, _actionMap.SpeedUp() * _speedUpModifier);
        float actualTranslationSpeed = Mathf.Clamp(_translationSpeed + additionalSpeed, 0f, _maxTranslationSpeed);

        Vector3 translation = translationInput * (actualTranslationSpeed * _clock.DeltaTime);
	    translation = Vector3.Lerp(_lastTranslation, translation, _translationSmoothingSpeed * _clock.DeltaTime);

        // Side-effects
        transform.Translate(translation, Space.Self);
	    _lastTranslation = translation;
    }

    void Rotate() {
        var rotationInput = _handleInput ? _actionMap.LookDirection() : Vector2.zero;

        rotationInput = Vector2.Lerp(_lastRotationInput, rotationInput, _rotationSmoothingSpeed * _clock.DeltaTime);
	    
	    // Side-effects
        transform.Rotate(new Vector3(0f, rotationInput.x * _rotationSpeed * _clock.DeltaTime, 0f), Space.World);
        transform.Rotate(new Vector3(rotationInput.y * _rotationSpeed * _clock.DeltaTime, 0f, 0f), Space.Self);
        _lastRotationInput = rotationInput;
    }

    public SpectatorActionMap ActionMap {
        get { return _actionMap; }
        set { _actionMap = value; }
    }
}
