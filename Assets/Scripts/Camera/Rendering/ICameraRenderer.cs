using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Cameras {

    /// <summary>
    /// Knows how the render the attached camera.
    /// </summary>
    interface ICameraRenderer {

        /// <summary>
        /// Renders the camera state to the given render texture.
        /// 
        /// If the render texture is null it is assumed that it has
        /// to render to the screen instead.
        /// </summary>
        /// <param name="target">the texture to render to. 
        /// If null, render target: screen is assumed.</param>
        void Render(RenderTexture target);
    }
}
