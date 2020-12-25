using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GuiCameraRenderer : CameraRenderer {

    [SerializeField] private Camera _camera;
    [SerializeField] private Material _blitMaterial;

    public override void Render(RenderTexture target) {
//        // TODO Find out what the right texture format is
        RenderTextureFormat textureFormat = _camera.hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        const int depth = 24;
        RenderTexture uiTexture = target != null
            ? RenderTexture.GetTemporary(target.width, target.height, depth, RenderTextureFormat.Default) 
            : RenderTexture.GetTemporary(Screen.width, Screen.height, depth, textureFormat);

        var cameraTexture = _camera.targetTexture;
        _camera.targetTexture = uiTexture;
        _camera.Render();
        Graphics.Blit(uiTexture, target, _blitMaterial);
        _camera.targetTexture = cameraTexture;

        RenderTexture.ReleaseTemporary(uiTexture);
    }

}
