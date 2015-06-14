using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
using D = System.Drawing;

namespace TankIconMaker
{
    abstract unsafe class BitmapBase : IDisposable
    {
        /// <summary>Gets the width of the image in pixels.</summary>
        public int Width { get; protected set; }
        /// <summary>Gets the height of the image in pixels.</summary>
        public int Height { get; protected set; }
        /// <summary>Gets the stride (the number of bytes to go one pixel down) of the bitmap.</summary>
        public int Stride { get; protected set; }
        /// <summary>Gets a pointer to the pixel data. This value is only valid between calls to <see cref="Acquire"/> and <see cref="Release"/>.</summary>
        public byte* Data { get; private set; }
        /// <summary>Gets a pointer to the first byte outside of the visible pixel data. This value is only valid between calls to <see cref="Acquire"/> and <see cref="Release"/>.</summary>
        public byte* DataEnd { get; private set; }
        /// <summary>Gets a pointer to the first byte outside of the writable pixel buffer. This value is only valid between calls to <see cref="Acquire"/> and <see cref="Release"/>.</summary>
        public byte* DataEndStride { get; private set; }

        #region Acquire / Release

        protected abstract IntPtr Acquire();
        protected abstract void Release();

        private int _acquiresRead = 0;
        private int _acquiresWrite = 0;
        private Thread _thread = null;
        private bool _multithreadedReading = false;

        private void acquire(bool write)
        {
            if (_acquiresRead == int.MinValue)
                throw new ObjectDisposedException("BitmapBase");
            if (write && IsReadOnly)
                throw new InvalidOperationException("Cannot use this bitmap for writing because it is marked as read-only.");
            if (_acquiresRead == 0 && _acquiresWrite == 0)
            {
                // First concurrent operation: always allow
                _thread = Thread.CurrentThread;
                // And also do the actual Acquire
                Data = (byte*) Acquire();
                DataEnd = Data + (Height - 1) * Stride + Width * 4;
                DataEndStride = Data + Height * Stride;
            }
            else if (_multithreadedReading)
            {
                if (write)
                    throw new InvalidOperationException("Cannot use this bitmap for writing because there exist several concurrent readers.");
            }
            else
            {
                // We have a single thread that is reading, writing, or both
                if (_thread != Thread.CurrentThread)
                {
                    // Same thread is allowed and nothing else is required. But this is a different thread.
                    if (!write && _acquiresWrite == 0)
                        _multithreadedReading = true;
                    else
                        throw new InvalidOperationException("Cannot use this bitmap because another thread is currently using it.");
                }
            }

            if (write)
                _acquiresWrite++;
            else
                _acquiresRead++;
        }

        private void release(bool write)
        {
            if (_acquiresRead == int.MinValue) // a manual dispose followed by a manual release: tell the programmer they messed up
                throw new ObjectDisposedException("BitmapBase");
            if (_acquiresRead <= 0 && _acquiresWrite <= 0)
                throw new Exception("4109876");

            if (write)
                _acquiresWrite--;
            else
                _acquiresRead--;

            if (_acquiresRead == 0 && _acquiresWrite == 0)
            {
                Release();
                Data = DataEnd = DataEndStride = null;
                _multithreadedReading = false;
                _thread = null;
            }
        }

        public BitmapReadReleaser UseRead()
        {
            return new BitmapReadReleaser(this);
        }

        public BitmapWriteReleaser UseWrite()
        {
            return new BitmapWriteReleaser(this);
        }

        public struct BitmapReadReleaser : IDisposable
        {
            private BitmapBase _bmp;
            public BitmapReadReleaser(BitmapBase bitmap) { _bmp = bitmap; lock (_bmp) _bmp.acquire(write: false); }
            public void Dispose() { lock (_bmp) _bmp.release(write: false); }
        }

        public struct BitmapWriteReleaser : IDisposable
        {
            private BitmapBase _bmp;
            public BitmapWriteReleaser(BitmapBase bitmap) { _bmp = bitmap; lock (_bmp) _bmp.acquire(write: true); }
            public void Dispose() { lock (_bmp) _bmp.release(write: true); }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                if (_acquiresRead > 0 || _acquiresWrite > 0)
                    Release();
                _acquiresRead = int.MinValue;
            }
        }

        ~BitmapBase()
        {
            Dispose();
        }

        #endregion

        public bool IsReadOnly { get; private set; }

        public virtual void MarkReadOnly()
        {
            IsReadOnly = true;
        }

        public void CopyPixelsFrom(byte* srcData, int srcWidth, int srcHeight, int srcStride, bool flipVertical = false)
        {
            using (this.UseWrite())
            {
                if (srcWidth <= 0 || srcHeight <= 0)
                    return;
                int copyBytes = srcWidth <= this.Width ? Math.Min(srcStride, this.Stride) : (this.Width * 4);
                if (!flipVertical)
                {
                    byte* dest = this.Data;
                    byte* src = srcData;
                    byte* srcDataEnd = srcData + srcStride * srcHeight;
                    do
                    {
                        Ut.MemCpy(dest, src, copyBytes);
                        dest += this.Stride;
                        src += srcStride;
                    }
                    while (dest < this.DataEnd && src < srcDataEnd);
                }
                else
                {
                    byte* dest = this.Data;
                    byte* src = srcData + srcStride * srcHeight;
                    do
                    {
                        src -= srcStride;
                        Ut.MemCpy(dest, src, copyBytes);
                        dest += this.Stride;
                    }
                    while (dest < this.DataEnd && src > srcData);
                }
            }
        }

        public void CopyPixelsFrom(byte[] srcData, int srcWidth, int srcHeight, int srcStride, bool flipVertical = false)
        {
            fixed (byte* srcDataPtr = srcData)
                CopyPixelsFrom(srcDataPtr, srcWidth, srcHeight, srcStride, flipVertical);
        }

        public void CopyPixelsFrom(BitmapBase source)
        {
            using (source.UseRead())
                CopyPixelsFrom(source.Data, source.Width, source.Height, source.Stride);
        }

        public void CopyPixelsFrom(BitmapSource source)
        {
            using (UseWrite())
            {
                if (source.Format != PixelFormats.Bgra32)
                    source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
                source.CopyPixels(new Int32Rect(0, 0, Math.Min(Width, source.PixelWidth), Math.Min(Height, source.PixelHeight)),
                    (IntPtr) Data, Stride * Height, Stride);
            }
        }

        public void Clear()
        {
            using (UseWrite())
            {
                Ut.MemSet(this.Data, 0, (int) (this.DataEnd - this.Data));
            }
        }

        public BitmapBase AsWritable()
        {
            return IsReadOnly ? ToBitmapRam() : this;
        }

        public BitmapBase AsWritableSame()
        {
            return IsReadOnly ? ToBitmapSame() : this;
        }

        public BitmapRam ToBitmapRam()
        {
            var result = new BitmapRam(Width, Height);
            result.CopyPixelsFrom(this);
            return result;
        }

        public BitmapGdi ToBitmapGdi()
        {
            var result = new BitmapGdi(Width, Height);
            result.CopyPixelsFrom(this);
            return result;
        }

        public BitmapWpf ToBitmapWpf()
        {
            var result = new BitmapWpf(Width, Height);
            result.CopyPixelsFrom(this);
            return result;
        }

        public abstract BitmapBase ToBitmapSame();

        public MagickImage ToMagickImage()
        {
            // Copy the pixel data while eliminating the Stride
            var data = new byte[Width * Height * 4];
            using (UseRead())
                fixed (byte* dataPtr = data)
                    for (int y = 0; y < Height; y++)
                        Ut.MemCpy(dataPtr + y * Width * 4, Data + y * Stride, Width * 4);

            // Construct a MagickImage by reading the BGRA pixel data
            // Cannot use GetWritablePixels due to an occasional access violation exception that's proved too elusive to track down within the available time.
            return new MagickImage(data, new MagickReadSettings { Width = Width, Height = Height, Format = MagickFormat.Bgra });
        }

        public void DrawImage(BitmapBase image, int destX = 0, int destY = 0, bool below = false)
        {
            DrawImage(image, destX, destY, 0, 0, image.Width, image.Height, below);
        }

        public void DrawImage(BitmapBase image, int destX, int destY, int srcX, int srcY, int width, int height, bool below)
        {
            using (UseWrite())
            using (image.UseRead())
            {
                if (width <= 0 || height <= 0)
                    return;
                if (destX < 0)
                {
                    srcX -= destX;
                    width += destX;
                    destX = 0;
                }
                if (destY < 0)
                {
                    srcY -= destY;
                    height += destY;
                    destY = 0;
                }
                if (srcX >= image.Width || srcY >= image.Height)
                    return;
                if (srcX < 0)
                {
                    destX -= srcX;
                    width += srcX;
                    srcX = 0;
                }
                if (srcY < 0)
                {
                    destY -= srcY;
                    height += srcY;
                    srcY = 0;
                }
                if (destX >= Width || destY >= Height)
                    return;
                if (destX + width > Width)
                    width = Width - destX;
                if (destY + height > Height)
                    height = Height - destY;
                if (srcX + width > image.Width)
                    width = image.Width - srcX;
                if (srcY + height > image.Height)
                    height = image.Height - srcY;
                if (width <= 0 || height <= 0) // cannot be negative at this stage, but just in case...
                    return;

                byte* dest = Data + destY * Stride + destX * 4;
                byte* src = image.Data + srcY * image.Stride + srcX * 4;

                for (int y = 0; y < height; y++, dest += Stride, src += image.Stride)
                {
                    byte* tgt = dest;
                    byte* btm = below ? src : dest;
                    byte* top = below ? dest : src;
                    byte* end = tgt + width * 4;
                    do
                    {
                        byte topA = *(top + 3);
                        byte btmA = *(btm + 3);
                        if (topA == 255 || btmA == 0)
                            *(int*) tgt = *(int*) top;
                        else if (topA == 0)
                            *(int*) tgt = *(int*) btm;
                        else if (btmA == 255)
                        {
                            // green
                            *(tgt + 1) = (byte) ((*(top + 1) * topA + *(btm + 1) * (255 - topA)) >> 8);
                            // red and blue
                            *(uint*) tgt = (*(uint*) tgt & 0xFF00FF00u) | (((((*(uint*) top) & 0x00FF00FFu) * topA + ((*(uint*) btm) & 0x00FF00FFu) * (uint) (255 - topA)) >> 8) & 0x00FF00FFu);
                            // alpha (only needed when "below" is true)
                            *(tgt + 3) = 255;
                        }
                        else // topA and btmA both >0 and <255
                        {
                            byte tgtAA = *(tgt + 3) = (byte) (topA + (btmA * (255 - topA) >> 8));
                            int btmAA = (btmA * (255 - topA)) / 255;
                            tgtAA += 1; // ensures the division below never results in a value greater than 255
                            *(tgt + 0) = (byte) ((*(top + 0) * topA + *(btm + 0) * btmAA) / tgtAA);
                            *(tgt + 1) = (byte) ((*(top + 1) * topA + *(btm + 1) * btmAA) / tgtAA);
                            *(tgt + 2) = (byte) ((*(top + 2) * topA + *(btm + 2) * btmAA) / tgtAA);
                        }
                        tgt += 4;
                        btm += 4;
                        top += 4;
                    }
                    while (tgt < end);
                }
            }
        }

        public void ReplaceColor(Color color)
        {
            using (UseWrite())
            {
                byte r = color.R;
                byte g = color.G;
                byte b = color.B;
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        *ptr++ = b;
                        *ptr++ = g;
                        *ptr++ = r;
                        ptr++;
                    }
                }
            }
        }

        /// <summary>Applies a "colorize" effect to this image.</summary>
        /// <param name="hue">The hue of the color to apply, 0..359</param>
        /// <param name="saturation">The saturation of the color to apply, 0..1</param>
        /// <param name="lightness">A lightness adjustment, -1..1</param>
        /// <param name="alpha">Overall strength of the effect, 0..1. A value of 0 keeps only the original image.</param>
        public void Colorize(int hue, double saturation, double lightness, double alpha)
        {
            using (UseWrite())
            {
                // http://stackoverflow.com/a/9177602/33080
                var color = Ut.BlendColors(Color.FromRgb(128, 128, 128), ColorHSV.FromHSV(hue, 100, 100).ToColorWpf(), saturation);
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        double pixel = Math.Max(*ptr, Math.Max(*(ptr + 1), *(ptr + 2))) / 255.0;
                        double position = lightness >= 0 ? (2 * (1 - lightness) * (pixel - 1) + 1) : 2 * (1 + lightness) * (pixel) - 1;
                        *ptr = (byte) (*ptr * (1 - alpha) + (position < 0 ? color.B * (position + 1) : (color.B * (1 - position) + 255 * position)) * alpha);
                        ptr++;
                        *ptr = (byte) (*ptr * (1 - alpha) + (position < 0 ? color.G * (position + 1) : (color.G * (1 - position) + 255 * position)) * alpha);
                        ptr++;
                        *ptr = (byte) (*ptr * (1 - alpha) + (position < 0 ? color.R * (position + 1) : (color.R * (1 - position) + 255 * position)) * alpha);
                        ptr += 2;
                    }
                }
            }
        }

        public void PreMultiply()
        {
            using (UseWrite())
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        byte alpha = ptr[3];
                        ptr[0] = (byte) ((ptr[0] * alpha) / 255);
                        ptr[1] = (byte) ((ptr[1] * alpha) / 255);
                        ptr[2] = (byte) ((ptr[2] * alpha) / 255);
                        ptr += 4;
                    }
                }
        }

        public void UnPreMultiply()
        {
            using (UseWrite())
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        byte alpha = ptr[3];
                        if (alpha > 0)
                        {
                            ptr[0] = (byte) ((ptr[0] * 255) / alpha);
                            ptr[1] = (byte) ((ptr[1] * 255) / alpha);
                            ptr[2] = (byte) ((ptr[2] * 255) / alpha);
                        }
                        ptr += 4;
                    }
                }
        }

        public void Blur(GaussianBlur blur, BlurEdgeMode edgeMode)
        {
            var temp = new BitmapRam(Width, Height);
            using (UseWrite())
            using (temp.UseWrite())
            {
                PreMultiply();
                blur.Horizontal(this, temp, edgeMode);
                blur.Vertical(temp, this, edgeMode);
                UnPreMultiply();
            }
        }

        public void Blur(GaussianBlur blur, BlurEdgeMode edgeMode, bool horz, bool vert)
        {
            if (!horz && !vert)
                return;
            if (horz && vert)
            {
                Blur(blur, edgeMode);
                return;
            }
            var temp = ToBitmapRam();
            using (temp.UseWrite())
            using (UseWrite())
            {
                temp.PreMultiply();
                if (horz)
                    blur.Horizontal(temp, this, edgeMode);
                else
                    blur.Vertical(temp, this, edgeMode);
                UnPreMultiply();
            }
        }

        public void ScaleOpacity(double adjustment, OpacityStyle style)
        {
            if (adjustment == 0)
                return;
            if (style == OpacityStyle.Auto)
                style = adjustment > 0 ? OpacityStyle.Additive : OpacityStyle.MoveEndpoint;

            var lut = new byte[256];
            for (int x = 0; x < 256; x++)
                switch (style)
                {
                    case OpacityStyle.MoveEndpoint:
                        if (adjustment < 0)
                            lut[x] = (byte) (x / (1.0 + -adjustment));
                        else
                            lut[x] = (byte) (255 - (255 - x) / (1.0 + adjustment));
                        break;
                    case OpacityStyle.MoveMidpoint:
                        lut[x] = (byte) (Math.Pow(x / 255.0, adjustment < 0 ? (1 - adjustment) : (1 / (1 + adjustment))) * 255);
                        break;
                    case OpacityStyle.Additive:
                        if (adjustment < 0)
                            lut[x] = (byte) Math.Max(0, 255 - (255 - x) * (1.0 - adjustment));
                        else
                            lut[x] = (byte) Math.Min(255, x * (1.0 + adjustment));
                        break;
                    default:
                        throw new Exception();
                }

            using (UseWrite())
            {
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride + 3;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        *ptr = lut[*ptr];
                        ptr += 4;
                    }
                }
            }
        }

        /// <summary>Makes the whole image more transparent by adjusting the alpha channel.</summary>
        /// <param name="opacity">The opacity to apply, 0..255. 0 makes the image completely transparent, while 255 makes no changes at all.</param>
        public void Transparentize(int opacity)
        {
            using (UseWrite())
            {
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride + 3;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        *ptr = (byte) ((*ptr * opacity) / 255);
                        ptr += 4;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new image which contains a 1 pixel wide black outline of the specified image.
        /// </summary>
        public void GetOutline(BitmapBase result, Color color, int threshold, bool inside)
        {
            const byte outside = 0;
            using (UseRead())
            using (result.UseWrite())
            {
                var src = Data;
                var tgt = result.Data;
                byte cr = color.R, cg = color.G, cb = color.B, ca = color.A;
                for (int y = 0; y < Height; y++)
                {
                    int b = y * Stride;
                    int left = outside;
                    int cur = src[b + 0 + 3];
                    int right;
                    for (int x = 0; x < Width; x++, b += 4)
                    {
                        right = x == Width - 1 ? outside : src[b + 4 + 3];
                        if ((src[b + 3] <= threshold) ^ inside)
                        {
                            if (
                                ((left > threshold) ^ inside) ||
                                ((right > threshold) ^ inside) ||
                                (((y == 0 ? outside : src[b - Stride + 3]) > threshold) ^ inside) ||
                                (((y == Height - 1 ? outside : src[b + Stride + 3]) > threshold) ^ inside)
                            )
                            {
                                tgt[b] = cb;
                                tgt[b + 1] = cg;
                                tgt[b + 2] = cr;
                                tgt[b + 3] = ca;
                            }
                        }
                        left = cur;
                        cur = right;
                    }
                }
            }
        }

        /// <summary>Flips the image horizontally.</summary>
        public void FlipHorz()
        {
            using (UseWrite())
            {
                for (int y = 0; y < Height; y++)
                {
                    uint* ptrL = (uint*) (Data + y * Stride);
                    uint* ptrR = (uint*) (Data + y * Stride + (Width - 1) * 4);
                    while (ptrL < ptrR)
                    {
                        uint temp = *ptrR;
                        *ptrR = *ptrL;
                        *ptrL = temp;
                        ptrL++;
                        ptrR--;
                    }
                }
            }
        }

        /// <summary>Flips the image vertically.</summary>
        public void FlipVert()
        {
            using (UseWrite())
            {
                int yTop = 0;
                int yBtm = Height - 1;
                byte* temp = stackalloc byte[Stride];
                while (yTop < yBtm)
                {
                    Ut.MemCpy(temp, Data + yTop * Stride, Stride);
                    Ut.MemCpy(Data + yTop * Stride, Data + yBtm * Stride, Stride);
                    Ut.MemCpy(Data + yBtm * Stride, temp, Stride);
                    yTop++;
                    yBtm--;
                }
            }
        }

        /// <summary>
        /// Finds the X coordinate of the leftmost pixel whose alpha channel exceeds the specified threshold.
        /// </summary>
        public int PreciseLeft(int alphaThreshold = 0)
        {
            using (UseRead())
            {
                byte* start = Data + 3;
                byte* end = Data + Stride * (Height - 1) + Width * 4; // pointer to first byte beyond the last pixel
                for (int x = 0; x < Width; x++, start += 4)
                    for (byte* alpha = start; alpha < end; alpha += Stride)
                        if (*alpha > alphaThreshold)
                            return x;
                return Width;
            }
        }

        /// <summary>
        /// Finds the X coordinate of the rightmost pixel whose alpha channel exceeds the specified threshold.
        /// </summary>
        public int PreciseRight(int alphaThreshold = 0)
        {
            using (UseRead())
            {
                byte* start = Data + (Width - 1) * 4 + 3;
                byte* end = Data + Stride * (Height - 1) + Width * 4; // pointer to first byte beyond the last pixel
                for (int x = Width - 1; x >= 0; x--, start -= 4)
                    for (byte* alpha = start; alpha < end; alpha += Stride)
                        if (*alpha > alphaThreshold)
                            return x;
                return -1;
            }
        }

        /// <summary>
        /// Finds the Y coordinate of the topmost pixel whose alpha channel exceeds
        /// the specified threshold. If the leftmost and/or rightmost such pixels are known, the search space can be
        /// reduced using the <paramref name="left"/> and <paramref name="width"/> arguments.
        /// </summary>
        public int PreciseTop(int alphaThreshold = 0, int left = 0, int width = -1)
        {
            if (width < 0)
                width = Width;
            using (UseRead())
            {
                byte* start = Data + left * 4 + 3;
                for (int y = 0; y < Height; y++, start += Stride)
                {
                    byte* end = start + (width - left) * 4;
                    for (byte* alpha = start; alpha < end; alpha += 4)
                        if (*alpha > alphaThreshold)
                            return y;
                }
                return Height;
            }
        }

        /// <summary>
        /// Finds the Y coordinate of the bottommost pixel whose alpha channel exceeds
        /// the specified threshold. If the leftmost and/or rightmost such pixels are known, the search space can be
        /// reduced using the <paramref name="left"/> and <paramref name="width"/> arguments.
        /// </summary>
        public int PreciseBottom(int alphaThreshold = 0, int left = 0, int width = -1)
        {
            if (width < 0)
                width = Width;
            using (UseRead())
            {
                byte* start = Data + (Height - 1) * Stride + left * 4 + 3;
                for (int y = Height - 1; y >= 0; y--, start -= Stride)
                {
                    byte* end = start + (width - left) * 4;
                    for (byte* alpha = start; alpha < end; alpha += 4)
                        if (*alpha > alphaThreshold)
                            return y;
                }
                return -1;
            }
        }

        /// <summary>
        /// Finds the smallest and largest X and Y coordinates of the pixels whose alpha channel exceeds the specified threshold.
        /// </summary>
        public PixelRect PreciseSize(int alphaThreshold = 0)
        {
            using (UseRead())
            {
                int left = PreciseLeft(alphaThreshold);
                int right = PreciseRight(alphaThreshold);
                int top = PreciseTop(alphaThreshold, left, right + 1);
                int bottom = PreciseBottom(alphaThreshold, left, right + 1);
                return PixelRect.FromBounds(left, top, right, bottom);
            }
        }

        /// <summary>
        /// Finds the smallest and largest X coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The Y coordinates of the result cover the entire image.
        /// </summary>
        public PixelRect PreciseWidth(int alphaThreshold = 0)
        {
            using (UseRead())
                return PixelRect.FromLeftRight(PreciseLeft(alphaThreshold), PreciseRight(alphaThreshold));
        }

        /// <summary>
        /// Finds the smallest and largest Y coordinates of the pixels whose alpha channel
        /// exceeds the specified threshold. The X coordinates of the result cover the entire image.
        /// </summary>
        public PixelRect PreciseHeight(int alphaThreshold = 0, int left = 0, int width = -1)
        {
            if (width < 0)
                width = Width;
            using (UseRead())
                return PixelRect.FromTopBottom(PreciseTop(alphaThreshold, left, width), PreciseBottom(alphaThreshold, left, width));
        }
    }

    sealed unsafe class BitmapRam : BitmapBase
    {
        private byte[] _bytes;
        private GCHandle _handle;

        public BitmapRam(int width, int height)
        {
            Width = width;
            Height = height;
            Stride = width * 4 + (16 - (width * 4) % 16) % 16; // pad to 16 bytes
            _bytes = new byte[Stride * Height];
        }

        protected override IntPtr Acquire()
        {
            _handle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
            return _handle.AddrOfPinnedObject();
        }

        protected override void Release()
        {
            _handle.Free();
        }

        public override BitmapBase ToBitmapSame()
        {
            return ToBitmapRam();
        }
    }

    /// <summary>
    /// Wrapper around a GDI Bitmap that allows access to its raw byte data. The API is intended to somewhat
    /// resemble that of the WPF BitmapSource. Intentionally supports just a single pixel format: 32bppArgb, aka Bgra32.
    /// </summary>
    sealed class BitmapGdi : BitmapBase, IDisposable
    {
        private SharedPinnedByteArray _bytes;

        /// <summary>
        /// Creates a new, blank BitmapGdi with the specified width and height. The pixel format is fixed: 32bppArgb, aka Bgra32.
        /// </summary>
        public BitmapGdi(int width, int height)
        {
            Width = width;
            Height = height;
            Stride = width * 4 + (16 - (width * 4) % 16) % 16; // pad to 16 bytes
            _bytes = new SharedPinnedByteArray(Stride * Height);
            Bitmap = new D.Bitmap(Width, Height, Stride, D.Imaging.PixelFormat.Format32bppArgb, _bytes.Address);
            Bitmap.SetResolution(96, 96);
        }

        /// <summary>Gets the bitmap bit buffer. Writes to this array modify the image; writes to the image modify this array.</summary>
        public byte[] BackBytes { get { return _bytes.Bytes; } }

        /// <summary>Gets a pointer to the buffer containing the bitmap bit buffer.</summary>
        public IntPtr BackBuffer { get { return _bytes.Address; } }

        /// <summary>
        /// Gets the underlying Bitmap that this BitmapGdi wraps. USAGE WARNING:
        /// DO NOT use this if the BitmapGdi wrapping it may have gone out of scope
        /// and disposed of. This will cause intermittent issues - when the BitmapGdi
        /// gets GC'd. Use <see cref="GetBitmapCopy"/> or GC.KeepAlive the wrapper.
        /// </summary>
        public D.Bitmap Bitmap { get; private set; }

        /// <summary>
        /// Use this to create a new Bitmap that is a copy of the image stored in this
        /// BitmapGdi. This can be passed around safely, unlike the wrapped bitmap
        /// returned by <see cref="Bitmap"/>.
        /// </summary>
        public D.Bitmap GetBitmapCopy()
        {
            var bmp = new D.Bitmap(Bitmap);
            using (var gr = D.Graphics.FromImage(bmp))
                gr.DrawImageUnscaled(Bitmap, 0, 0);
            return bmp;
        }

        /// <summary>Converts this bitmap to a WPF BitmapSource instance.</summary>
        public BitmapSource ToWpf()
        {
            var writable = ToWpfWriteable();
            writable.Freeze();
            return writable;
        }

        /// <summary>Converts this bitmap to a modifiable WPF WriteableBitmap instance.</summary>
        public WriteableBitmap ToWpfWriteable()
        {
            var writable = new WriteableBitmap(Width, Height, Bitmap.HorizontalResolution, Bitmap.VerticalResolution, PixelFormats.Bgra32, null);
            writable.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), BackBytes, Stride, 0);
            return writable;
        }

        /// <summary>Disposes of the underlying resources.</summary>
        public override void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
            }
            if (_bytes != null)
            {
                _bytes.ReleaseReference();
                _bytes = null;
            }
            base.Dispose();
        }

        protected override IntPtr Acquire()
        {
            return _bytes.Address;
        }

        protected override void Release()
        {
        }

        public override BitmapBase ToBitmapSame()
        {
            return ToBitmapGdi();
        }
    }

    /// <summary>
    /// This class represents a byte array which is pinned to avoid relocation
    /// by the GC and implements reference counting.
    /// </summary>
    sealed class SharedPinnedByteArray
    {
        private GCHandle _handle;
        private int _refCount;
        private bool _destroyed;

        /// <summary>Gets the allocated byte array.</summary>
        public byte[] Bytes { get; private set; }

        /// <summary>Gets the length of the byte array.</summary>
        public int Length { get { return Bytes.Length; } }

        /// <summary>Gets an unmanaged address of the first (index 0) byte of the byte array.</summary>
        public IntPtr Address { get; private set; }

        /// <summary>Returns an unmanaged address of the specified byte in the byte array.</summary>
        public IntPtr AddressOf(int index) { return Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, index); }

        /// <summary>
        /// Creates a new pinned array of the specified size, that can be accessed through <see cref="Bytes"/>.
        /// One reference is automatically added; call <see cref="ReleaseReference"/> when finished using this array.
        /// </summary>
        /// <param name="length">The number of bytes that the pinned array should contain.</param>
        public SharedPinnedByteArray(int length)
        {
            Bytes = new byte[length];
            _handle = GCHandle.Alloc(Bytes, GCHandleType.Pinned);
            Address = Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, 0);
            _refCount++;
        }

        /// <summary>
        /// Adds a reference to this array. One reference is counted when the array is created. It is deleted when
        /// all references are released using <see cref="ReleaseReference"/>.
        /// </summary>
        public void AddReference()
        {
            _refCount++;
        }

        /// <summary>
        /// Releases a reference to this array. When there are none left, the array is unpinned and can get garbage-collected.
        /// Note that the array is released the moment the ref count drops to zero for the first time.
        /// </summary>
        public void ReleaseReference()
        {
            _refCount--;
            if (_refCount <= 0)
                destroy();
        }

        private void destroy()
        {
            if (!_destroyed)
            {
                _handle.Free();
                Bytes = null;
                _destroyed = true;
            }
        }

        ~SharedPinnedByteArray()
        {
            destroy();
        }
    }

    sealed class BitmapWpf : BitmapBase
    {
        private WriteableBitmap _bitmap;

        public BitmapWpf(int width, int height)
        {
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            Width = width;
            Height = height;
            Stride = _bitmap.BackBufferStride;
        }

        public WriteableBitmap UnderlyingImage { get { return _bitmap; } }

        public override void MarkReadOnly()
        {
            base.MarkReadOnly();
            _bitmap.Freeze();
        }

        protected override IntPtr Acquire()
        {
            return _bitmap.BackBuffer;
        }

        protected override void Release()
        {
        }

        public override BitmapBase ToBitmapSame()
        {
            return ToBitmapWpf();
        }
    }

}
