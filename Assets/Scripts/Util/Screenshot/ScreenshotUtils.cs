using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RamjetAnvil.Reactive;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public struct Dimension {
        public int Width;
        public int Height;
    }

    public struct Screenshot {
        public Dimension Dimension;
        public Color32[] Pixels;
    }

    public static class ScreenshotUtils {
        
        public static Screenshot CopyFromRenderTexture(RenderTexture renderTexture, Texture2D renderTarget) {
            var prevRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            renderTarget.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            renderTarget.Apply();
            RenderTexture.active = prevRenderTexture;
            return new Screenshot {
                Dimension = new Dimension { Width = renderTarget.width, Height = renderTarget.height },
                Pixels = renderTarget.GetPixels32(),
            };
        }

        public static void Write2File(string folder, Screenshot s, bool useAlphaChannel, Action onComplete = null) {
            AsyncUtil.DoWorkAsync(() => {
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                var filename = Path.Combine(folder, string.Format("screenshot_{0}.png", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")));
                PngUtil.WriteToFile(s.Dimension.Width, s.Dimension.Height, s.Pixels, filename, useAlphaChannel);

                if (onComplete != null) {
                    onComplete();
                }
            });
        }

        private static Color32[] ToGammaSpace(Color32[] linearPixels, float gamma) {
            var gammaPixels = new Color32[linearPixels.Length];

            for (int i = 0; i < linearPixels.Length; i++) {
                Color pixel = linearPixels[i];
                pixel.r = LinearToGamma(pixel.r, gamma);
                pixel.g = LinearToGamma(pixel.g, gamma);
                pixel.b = LinearToGamma(pixel.b, gamma);
                gammaPixels[i] = pixel;
            }

            return gammaPixels;
        }

        private static float LinearToGamma(float linearValue, float gamma) {
            return linearValue < 0.00313f ? linearValue : (float)Math.Pow(linearValue, gamma);
        }

    }
}
