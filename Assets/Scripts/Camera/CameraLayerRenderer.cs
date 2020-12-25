using UnityEngine;

[ExecuteInEditMode]
public class CameraLayerRenderer : MonoBehaviour {
    [SerializeField] private CameraRenderer _cameraRenderer;

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        _cameraRenderer.Render(destination);
    }
}
