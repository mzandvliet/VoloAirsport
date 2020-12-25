using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Cameras;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class MultiCameraRenderer : CameraRenderer {

    [SerializeField] private CameraRenderer[] _cameraRenderers;

    private CameraRenderer _lastRenderer;

    public override void Render(RenderTexture target) {
        for (int i = 0; i < _cameraRenderers.Length; i++) {
            _cameraRenderers[i].Render(target);
        }
    }
}
