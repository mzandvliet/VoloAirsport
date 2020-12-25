using System;
using RamjetAnvil.Volo;
using UnityEngine;
using UnityEngine.UI;

class UICanvasManager : MonoBehaviour {
    [SerializeField] private Canvas _canvas;
    [SerializeField] private CanvasScaler _canvasScaler;
    [SerializeField] private float _vrUiDistance = 10f;

    private bool _isInitialized;
    private bool _isVr;
    private CameraManager _camManager;
    private RectTransform _transform;

    public void Initialize(CameraManager camManager) {
        if (!camManager.IsRigInitialized) {
            throw new Exception("UICanvasManager cannot initialize before CameraManager has created a Rig instance");
        }

        _camManager = camManager;
        _isVr = camManager.VrMode != VrMode.None;
        _transform = GetComponent<RectTransform>();

        if (_isVr) {
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
        }
        else {
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }

        _isInitialized = true;
    }

    private void LateUpdate() {
        if (!_isInitialized) {
            return;
        }

        Transform rigTransform = _camManager.Rig.GetComponent<Transform>();

        if (_isVr) {
            _transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _transform.position = rigTransform.position + rigTransform.forward * _vrUiDistance;
            _transform.rotation = Quaternion.LookRotation(rigTransform.forward, rigTransform.up);
        }
    }
}
