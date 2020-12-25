using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Gui;
using UnityEngine;

public class ScaleToFocalLength : MonoBehaviour {

    [SerializeField] private Vector3 _initialScale = Vector3.one;

    [Dependency, SerializeField]private ICameraRig _cameraRig;

    void Update() {
        // TODO Listen to field of view events
        var focalLength = GuiPlacement.FocalLength(_cameraRig.FieldOfView);
        transform.localScale = _initialScale * focalLength;
    }
}
