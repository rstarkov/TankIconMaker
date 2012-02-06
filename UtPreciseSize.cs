using System;
using System.Windows.Media.Imaging;

namespace TankIconMaker
{
    static partial class Ut
    {
        /// <summary>
        /// Given a BGRA32 bitmap data, finds the X coordinate of the leftmost pixel whose alpha channel exceeds
        /// the specified threshold.
        /// </summary>
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

        /// <summary>
        /// Given a BGRA32 bitmap data, finds the X coordinate of the rightmost pixel whose alpha channel exceeds
        /// the specified threshold.
        /// </summary>
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

        /// <summary>
        /// Given a BGRA32 bitmap data, finds the Y coordinate of the topmost pixel whose alpha channel exceeds
        /// the specified threshold. If the leftmost and/or rightmost such pixels are known, the search space can be
        /// reduced using the <paramref name="left"/> and <paramref name="width"/> arguments.
        /// </summary>
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

        /// <summary>
        /// Given a BGRA32 bitmap data, finds the Y coordinate of the bottommost pixel whose alpha channel exceeds
        /// the specified threshold. If the leftmost and/or rightmost such pixels are known, the search space can be
        /// reduced using the <paramref name="left"/> and <paramref name="width"/> arguments.
        /// </summary>
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

        /// <summary>
        /// Given a BGRA32 bitmap data, finds the smallest and largest X and Y coordinates of the pixels whose alpha
        /// channel exceeds the specified threshold.
        /// </summary>
        public unsafe static PixelRect PreciseSize(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            int left = PreciseLeft(image, width, height, stride, alphaThreshold);
            int right = PreciseRight(image, width, height, stride, alphaThreshold);
            int top = PreciseTop(image, right + 1, height, stride, alphaThreshold, left);
            int bottom = PreciseBottom(image, right + 1, height, stride, alphaThreshold, left);
            return PixelRect.FromBounds(left, top, right, bottom);
        }

        /// <summary>
        /// Given a BGRA32 bitmap data, finds the smallest and largest X coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The Y coordinates of the result cover the entire image.
        /// </summary>
        public unsafe static PixelRect PreciseWidth(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            return PixelRect.FromLeftRight(
                PreciseLeft(image, width, height, stride, alphaThreshold),
                PreciseRight(image, width, height, stride, alphaThreshold));
        }

        /// <summary>
        /// Given a BGRA32 bitmap data, finds the smallest and largest Y coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The X coordinates of the result cover the entire image.
        /// </summary>
        public unsafe static PixelRect PreciseHeight(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            return PixelRect.FromTopBottom(
                PreciseTop(image, width, height, stride, alphaThreshold, left),
                PreciseBottom(image, width, height, stride, alphaThreshold, left));
        }

        /// <summary>
        /// Finds the smallest and largest X and Y coordinates of the pixels whose alpha
        /// channel exceeds the specified threshold.
        /// </summary>
        public unsafe static PixelRect PreciseSize(this WriteableBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseSize((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        /// <summary>
        /// Finds the smallest and largest X and Y coordinates of the pixels whose alpha
        /// channel exceeds the specified threshold.
        /// </summary>
        public unsafe static PixelRect PreciseSize(this BitmapGdi image, int alphaThreshold = 0)
        {
            var result = PreciseSize((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        /// <summary>
        /// Finds the smallest and largest X coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The Y coordinates of the result cover the entire image.
        /// </summary>
        public unsafe static PixelRect PreciseWidth(this WriteableBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseWidth((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        /// <summary>
        /// Finds the smallest and largest X coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The Y coordinates of the result cover the entire image.
        /// </summary>
        public unsafe static PixelRect PreciseWidth(this BitmapGdi image, int alphaThreshold = 0)
        {
            var result = PreciseWidth((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        /// <summary>
        /// Finds the smallest and largest Y coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The X coordinates of the result cover the entire image.
        /// </summary>
        public unsafe static PixelRect PreciseHeight(this WriteableBitmap image, int alphaThreshold = 0, int left = 0)
        {
            var result = PreciseHeight((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold, left);
            GC.KeepAlive(image);
            return result;
        }

        /// <summary>
        /// Finds the smallest and largest Y coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The X coordinates of the result cover the entire image.
        /// </summary>
        public unsafe static PixelRect PreciseHeight(this BitmapGdi image, int alphaThreshold = 0, int left = 0)
        {
            var result = PreciseHeight((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold, left);
            GC.KeepAlive(image);
            return result;
        }
    }
}
