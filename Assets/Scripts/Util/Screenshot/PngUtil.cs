using System;
using Hjg.Pngcs;
using UnityEngine;

public static class PngUtil
{
    public static void WriteToFile(int imageWidth, int imageHeight, Color32[] pixels, String filename, bool hasAlpha)
    {
        ImageInfo imageInfo = new ImageInfo(imageWidth, imageHeight, 8, hasAlpha); // 8 bits per channel, with alpha 
        // open image for writing 
        PngWriter pngWriter = FileHelper.CreatePngWriter(filename, imageInfo, false);
        // add some optional metadata (chunks)
        pngWriter.GetMetadata().SetDpi(70.0);
        pngWriter.GetMetadata().SetTimeNow(0); // 0 seconds fron now = now

        ImageLine iline = new ImageLine(imageInfo);
        for (int row = 0; row < imageHeight; row++)
        {
            var bottomRow = (imageHeight - 1) - row;
            for (int col = 0; col < imageWidth; col++)
            {
                var pixel = pixels[col + (bottomRow * imageWidth)];
                if (hasAlpha) {
                    ImageLineHelper.SetPixel(iline, col, pixel.r, pixel.g, pixel.b, pixel.a);
                }
                else {
                    ImageLineHelper.SetPixel(iline, col, pixel.r, pixel.g, pixel.b);
                }
            }
            pngWriter.WriteRow(iline, row);
        }
        pngWriter.End();
    }
}
