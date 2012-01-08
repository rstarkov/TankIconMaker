using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;
using DI = System.Drawing.Imaging;

namespace TankIconMaker
{
    static partial class Ut
    {
        public unsafe static int PreciseLeft(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            byte* start = image + 3;
            byte* end = image + stride * (height - 1) + width * 4; // pointer to first byte beyond the last pixel
            for (int x = 0; x < width; x++, start += 4)
                for (byte* alpha = start; alpha < end; alpha += stride)
                    if (*alpha > alphaThreshold)
                        return x;
            return width;
        }

        public unsafe static int PreciseRight(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            byte* start = image + (width - 1) * 4 + 3;
            byte* end = image + stride * (height - 1) + width * 4; // pointer to first byte beyond the last pixel
            for (int x = width - 1; x >= 0; x--, start -= 4)
                for (byte* alpha = start; alpha < end; alpha += stride)
                    if (*alpha > alphaThreshold)
                        return x;
            return -1;
        }

        public unsafe static int PreciseTop(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            byte* start = image + left * 4 + 3;
            for (int y = 0; y < height; y++, start += stride)
            {
                byte* end = start + (width - left) * 4;
                for (byte* alpha = start; alpha < end; alpha += 4)
                    if (*alpha > alphaThreshold)
                        return y;
            }
            return height;
        }

        public unsafe static int PreciseBottom(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            byte* start = image + (height - 1) * stride + left * 4 + 3;
            for (int y = height - 1; y >= 0; y--, start -= stride)
            {
                byte* end = start + (width - left) * 4;
                for (byte* alpha = start; alpha < end; alpha += 4)
                    if (*alpha > alphaThreshold)
                        return y;
            }
            return -1;
        }

        public unsafe static PixelRect PreciseSize(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            int left = PreciseLeft(image, width, height, stride, alphaThreshold);
            int right = PreciseRight(image, width, height, stride, alphaThreshold);
            int top = PreciseTop(image, right + 1, height, stride, alphaThreshold, left);
            int bottom = PreciseBottom(image, right + 1, height, stride, alphaThreshold, left);
            return PixelRect.FromBounds(left, top, right, bottom);
        }

        public unsafe static PixelRect PreciseWidth(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            return PixelRect.FromLeftRight(
                PreciseLeft(image, width, height, stride, alphaThreshold),
                PreciseRight(image, width, height, stride, alphaThreshold));
        }

        public unsafe static PixelRect PreciseHeight(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            return PixelRect.FromTopBottom(
                PreciseTop(image, width, height, stride, alphaThreshold, left),
                PreciseBottom(image, width, height, stride, alphaThreshold, left));
        }

        public unsafe static PixelRect PreciseSize(this WriteableBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseSize((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseSize(this BitmapGdi image, int alphaThreshold = 0)
        {
            var result = PreciseSize((byte*) image.BytesPtr, image.PixelWidth, image.PixelHeight, image.Stride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseWidth(this WriteableBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseWidth((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseWidth(this BitmapGdi image, int alphaThreshold = 0)
        {
            var result = PreciseWidth((byte*) image.BytesPtr, image.PixelWidth, image.PixelHeight, image.Stride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseHeight(this WriteableBitmap image, int alphaThreshold = 0, int left = 0)
        {
            var result = PreciseHeight((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold, left);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseHeight(this BitmapGdi image, int alphaThreshold = 0, int left = 0)
        {
            var result = PreciseHeight((byte*) image.BytesPtr, image.PixelWidth, image.PixelHeight, image.Stride, alphaThreshold, left);
            GC.KeepAlive(image);
            return result;
        }
    }
}
