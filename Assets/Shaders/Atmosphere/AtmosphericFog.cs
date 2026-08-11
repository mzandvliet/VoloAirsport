using UnityEngine;

/// <summary>
/// Full-screen aerial perspective / distance fog. Add to a camera; pair with an
/// AtmosphereController somewhere in the scene, which bakes the sky cubemap and pushes the
/// global shader parameters this reads.
///
/// Standalone on purpose: the previous version derived from UnityStandardAssets'
/// PostEffectsBase and drew its own immediate-mode GL quad. Both are legacy Standard Assets
/// machinery that the port is trying to shed, and neither is needed - the frustum corner
/// indexing in the shader works fine with a plain Graphics.Blit.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Atmospheric Fog")]
public class AtmosphericFog : MonoBehaviour {

    [SerializeField] private Shader _fogShader;

    private Material _fogMaterial;
    private Camera _camera;

    private Camera Cam {
        get {
            if (_camera == null) {
                _camera = GetComponent<Camera>();
            }
            return _camera;
        }
    }

    private void OnEnable() {
        // The fog needs scene depth to reconstruct world position per pixel.
        Cam.depthTextureMode |= DepthTextureMode.Depth;
    }

    private void OnDisable() {
        if (_fogMaterial != null) {
            DestroyImmediate(_fogMaterial);
            _fogMaterial = null;
        }
    }

    private bool EnsureResources() {
        if (_fogShader == null) {
            _fogShader = Shader.Find("Custom/AtmosphericFog");
        }
        if (_fogShader == null || !_fogShader.isSupported) {
            return false;
        }
        if (_fogMaterial == null || _fogMaterial.shader != _fogShader) {
            _fogMaterial = new Material(_fogShader) { hideFlags = HideFlags.HideAndDontSave };
        }
        return true;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (!EnsureResources()) {
            Graphics.Blit(source, destination);
            return;
        }

        _fogMaterial.SetMatrix("_FrustumCornersWS", BuildFrustumCorners(Cam));
        _fogMaterial.SetVector("_CameraWS", Cam.transform.position);

        Graphics.Blit(source, destination, _fogMaterial);
    }

    /// <summary>
    /// The four far-plane corner rays in world space, in the row order the shader's
    /// frustumIndex expects: 0 = bottom left, 1 = bottom right, 2 = top left, 3 = top right.
    /// Each row is a full camera-to-far-plane vector, so multiplying by Linear01Depth gives
    /// the world offset of the shaded pixel directly.
    /// </summary>
    private static Matrix4x4 BuildFrustumCorners(Camera cam) {
        var t = cam.transform;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float fovHalf = cam.fieldOfView * 0.5f;

        Vector3 toRight = t.right * near * Mathf.Tan(fovHalf * Mathf.Deg2Rad) * cam.aspect;
        Vector3 toTop = t.up * near * Mathf.Tan(fovHalf * Mathf.Deg2Rad);

        Vector3 topLeft = t.forward * near - toRight + toTop;
        // Scale so the ray reaches the far plane rather than the near plane.
        float scale = topLeft.magnitude * far / near;

        Vector3 topRight = (t.forward * near + toRight + toTop).normalized * scale;
        Vector3 bottomRight = (t.forward * near + toRight - toTop).normalized * scale;
        Vector3 bottomLeft = (t.forward * near - toRight - toTop).normalized * scale;
        topLeft = topLeft.normalized * scale;

        var corners = Matrix4x4.identity;
        corners.SetRow(0, bottomLeft);
        corners.SetRow(1, bottomRight);
        corners.SetRow(2, topLeft);
        corners.SetRow(3, topRight);
        return corners;
    }
}
