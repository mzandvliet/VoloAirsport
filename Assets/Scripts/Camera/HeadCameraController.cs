using UnityEngine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;

public class HeadCameraController : MonoBehaviour {
    [Dependency, SerializeField] private PilotActionMapProvider _playerActionMapProvider;

    private Transform _transform;
    private Vector2 _mouseInput;

    private Quaternion _baseLocalRotation;

    private void Awake() {
        _transform = GetComponent<Transform>();
        _baseLocalRotation = _transform.localRotation;
    }

    // Todo: Not called at the moment!
    public void OnSpawn() {
        _lastRotation = _transform.parent.rotation * _baseLocalRotation;
    }

    private Quaternion _lastRotation;

    private void Update() {
        var playerActionMap = _playerActionMapProvider.ActionMap;

        Quaternion rotation = _transform.parent.rotation * _baseLocalRotation;
        rotation = Quaternion.Slerp(_lastRotation, rotation, 5f * Time.deltaTime);
        _lastRotation = rotation;

        Quaternion inputRotation;

        var input = new Vector2(playerActionMap.PollAxis(WingsuitAction.LookHorizontal),
                                playerActionMap.PollAxis(WingsuitAction.LookVertical));

        if (playerActionMap.PollButton(WingsuitAction.ActivateMouseLook) == ButtonState.Pressed) {
            _mouseInput += input * 0.5f;
            _mouseInput.x = Mathf.Clamp(_mouseInput.x, -50f, 50f);
            _mouseInput.y = Mathf.Clamp(_mouseInput.y, -50f, 50f);
            inputRotation = Quaternion.Euler(0f, -_mouseInput.x, 0f) * Quaternion.Euler(-_mouseInput.y, 0f, 0f);
        } else {
            _mouseInput = Vector2.zero;
            inputRotation = Quaternion.Euler(0f, -input.x * 50f, 0) * Quaternion.Euler(-input.y * 50f, 0f, 0f);
        }

        _transform.rotation = rotation * inputRotation;
    }
}
