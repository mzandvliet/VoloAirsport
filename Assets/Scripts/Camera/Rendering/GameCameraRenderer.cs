using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameCameraRenderer : CameraRenderer {

    [SerializeField] private Camera _camera;

    void Awake() {
        _camera = _camera ?? GetComponent<Camera>();
    }

    public override void Render(RenderTexture target) {
        var cameraTexture = _camera.targetTexture;
        _camera.targetTexture = target;
        _camera.Render();
        _camera.targetTexture = cameraTexture;
    }
}
