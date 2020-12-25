using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Cameras;
using UnityEngine;

public abstract class CameraRenderer : MonoBehaviour, ICameraRenderer {
    public abstract void Render(RenderTexture target);
}
