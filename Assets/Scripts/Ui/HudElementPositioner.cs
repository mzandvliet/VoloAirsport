using System.Collections.Generic;
using RamjetAnvil.Cameras;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Gui;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;

/* Todo:
 * - Different layout styles for VR and non-VR
 * - Context awareness?
 */

public class HudElementPositioner : MonoBehaviour {
    [SerializeField, Dependency] private GameSettingsProvider _settingsProvider;
    [SerializeField, Dependency] private CameraRig _cameraRig;
    [SerializeField] private Camera _camera;

    [SerializeField] private float _vrShrinkFactor = 2f;
    [SerializeField] private List<HudElement> _elements;

    void Awake() {
        _elements = _elements ?? new List<HudElement>();
    }

    private void LateUpdate() {
        if (_cameraRig == null || _settingsProvider == null) {
            return;
        }

        var cameraProperties = GetCameraProperties();

        for (int i = _elements.Count - 1; i >= 0; i--) {
            var element = _elements[i];
            if (element == null) {
                _elements.RemoveAt(i);
            } else {
                PositionElement(element, cameraProperties);
            }
        }
    }

    private CameraProperties GetCameraProperties() {
        CameraProperties cameraProperties;
        if (_camera != null) {
            cameraProperties = new CameraProperties(_camera.fieldOfView, _camera.aspect, _camera.transform.MakeImmutable());
        }
        else if (_cameraRig != null) {
            cameraProperties = new CameraProperties(_cameraRig.FieldOfView, _cameraRig.GetMainCamera().aspect,
                _cameraRig.transform.MakeImmutable());
        }
        else {
            cameraProperties = new CameraProperties(85f, 1f, ImmutableTransform.Identity);
        }
        return cameraProperties;
    }

    private void PositionElement(HudElement element, CameraProperties cameraProperties) {
        var viewportRect = GuiPlacement.CameraViewportRectangle(cameraProperties, distance: element.ViewportPoint.z);
        var guiTransform = GuiPlacement.ProjectPointOnPlane(viewportRect, element.ViewportPoint);

        guiTransform = guiTransform.Rotate(element.RotationOffset);

        if (element.AdjustToFieldOfView) {
            guiTransform = guiTransform.UpdateScale(element.Scale * GuiPlacement.FocalLength(cameraProperties.FieldOfView));
        } else {
            guiTransform = guiTransform.UpdateScale(element.Scale);
        }
        // Make everything smaller when VR is enabled
        if (_settingsProvider.ActiveVrMode != VrMode.None) {
            guiTransform = guiTransform.Scale(Vector3.one / _vrShrinkFactor);
        }

        var scale = guiTransform.Scale;
        scale.z = 1;
        guiTransform = guiTransform.UpdateScale(scale);

        element.transform.SetLocal(guiTransform);
    }

    public void AddHudElement(HudElement element) {
        _elements.Add(element);
    }
}
