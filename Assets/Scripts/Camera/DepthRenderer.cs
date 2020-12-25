using UnityEngine;

/* 
 * Script and shader based on: http://answers.unity3d.com/questions/760126/copying-depth-from-one-rendertexture-to-another.html
 *
 */

public class DepthRenderer : MonoBehaviour {
    private RenderTexture _texture;
    private Shader _depthShader;
    private Camera _depthCamera;

    public RenderTexture Texture {
        get { return _texture; }
    }

    private void Start() {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;

        _depthShader = Shader.Find("RenderDepth");

        _depthCamera = new GameObject("DepthCamera").AddComponent<Camera>();
        _depthCamera.transform.parent = transform;
        _depthCamera.transform.localPosition = Vector3.zero;
        _depthCamera.transform.localRotation = Quaternion.identity;
        _depthCamera.CopyFrom(GetComponent<Camera>());
        _depthCamera.enabled = false;

        int camWidth = (int)GetComponent<Camera>().pixelWidth;
        int camHeight = (int)GetComponent<Camera>().pixelHeight;
        CreateTexture(camWidth, camHeight);
    }

    private void Update() {
        UpdateCameraProperties();
        UpdateDepthTexture();
    }

    private void UpdateDepthTexture() {
        //RenderTextureFormat cameraFormat = camera.hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

        int camWidth = (int) GetComponent<Camera>().pixelWidth;
        int camHeight = (int) GetComponent<Camera>().pixelHeight;

        if (_texture.width != camWidth || _texture.height != camHeight) { // || _texture.format != cameraFormat
            CreateTexture(camWidth, camHeight);
        }
    }

    private void UpdateCameraProperties() {
        _depthCamera.fieldOfView = GetComponent<Camera>().fieldOfView;
    }

    private void CreateTexture(int width, int height) {
        if (_texture && _texture.IsCreated()) {
            _texture.Release();
        }
        _texture = new RenderTexture(width, height, 24, RenderTextureFormat.Depth);
        _texture.Create();
        _depthCamera.targetTexture = _texture;
    }

    private void OnDestroy() {
        _texture.Release();
    }

    private void OnPreRender() {
        _depthCamera.nearClipPlane = GetComponent<Camera>().nearClipPlane;
        _depthCamera.RenderWithShader(_depthShader, "RenderType");
    }
}
