﻿using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using FrontEnd.Logging;

namespace FrontEnd
{
    /// <summary>
    /// Media related utility class 
    /// </summary>
    public class MediaUtils
    {
        /// <summary>
        /// Transform NV12 to bmp image so we can view how is it looks like. Note it's not NV12 to RBG conversion.
        /// </summary>
        /// <param name="data">NV12 sample data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public static Bitmap TransformNV12ToBmpFaster(byte[] data, int width = 1280, int height = 720)
        {
            int stride = (int)(4 * (((width * (Image.GetPixelFormatSize(PixelFormat.Format32bppRgb) / 8)) + 3) / 4));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int uvStart = (int)width * (int)height;
                for (int y = 0; y < (int)height; y++)
                {
                    int currentLine = y * bmpData.Stride;
                    for (int x = 0; x < (int)width; x++)
                    {
                        int vIndex = uvStart + (y >> 1) * (int)width + ((x >> 1) << 1);
                        byte grayscale = data[x + (y * width)];
                        byte blue = (byte)(data[vIndex] + grayscale - 127);
                        byte red = (byte)(data[vIndex + 1] + grayscale - 127);
                        byte green = (byte)(1.704 * grayscale - 0.510 * red - 0.194 * blue);

                        ptr[(x * 4) + y * stride] = blue;
                        ptr[(x * 4) + y * stride + 1] = green;
                        ptr[(x * 4) + y * stride + 2] = red;
                    }
                }
            }

            bmp.UnlockBits(bmpData);

            watch.Stop();
            Log.Info(new CallerInfo(), LogContext.Media, "Took {0} ms to lock and unlock", watch.ElapsedMilliseconds);

            return bmp;
        }
    }
}
