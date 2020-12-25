using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class EditorOrbitCamera : MonoBehaviour, ICameraMount
{
    [SerializeField] private AbstractUnityClock _clock;

    [SerializeField] private Vector3 _center;
    [SerializeField] private float _orbitDistance = 2.5f;
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 5f;

    private float _angleCameraX;
    private float _angleCameraY;
    private Vector3 _smoothCenter;

    public Vector3 Center
    {
        get { return _center; }
        set { _center = value; }
    }

    public void Move(float x, float y)
    {
        _angleCameraX += x;
        _angleCameraY += y;
    }

    public void OnMount(ICameraRig rig) {
    }

    public void OnDismount(ICameraRig rig) {
    }

    public void UpdateTransform()
    {
        _angleCameraX = MathUtils.WrapAngle(_angleCameraX);
        _angleCameraY = MathUtils.ClampAngle(_angleCameraY, -90f, 90f, 180f);

        Quaternion inputRotation = Quaternion.Euler(_angleCameraY, _angleCameraX, 0f);

        Quaternion smoothRotation = Quaternion.Lerp(transform.rotation, inputRotation, _rotationSpeed * _clock.DeltaTime);
        Quaternion targetRotation = smoothRotation;

        _smoothCenter = Vector3.Lerp(_smoothCenter, _center, _movementSpeed * _clock.DeltaTime);
        Vector3 targetPosition = _smoothCenter + targetRotation * new Vector3(0f, 0f, -_orbitDistance);

        transform.rotation = targetRotation;
        transform.position = targetPosition;
    }
}