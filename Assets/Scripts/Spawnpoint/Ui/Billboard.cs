using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Gui;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class Billboard : MonoBehaviour {

    [Dependency("cameraTransform"), SerializeField] private Transform _cameraTransform;

    void Update() {
        if (_cameraTransform.IsDestroyed()) {
            Debug.Log("camera transform is destroyed ");
            return;
        }

        // Set the bottom position of the GUI element as anchor point
        transform.position = transform.parent.position + _cameraTransform.up * (transform.localScale.y / 2);
        GuiPlacement.FaceTransform(transform, _cameraTransform);
        transform.Rotate(0, 180, 0, Space.Self);
    }
}
